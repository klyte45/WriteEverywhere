extern alias VS;
using ColossalFramework;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VS::Bridge_WE2VS;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Rendering;
using WriteEverywhere.UI;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Singleton
{
    public class WTSVehicleTextsSingleton : MonoBehaviour
    {
        public WTSVehicleData Data => WTSVehicleData.Instance;

        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> CityDescriptors => Data.CityDescriptors;
        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> GlobalDescriptors => Data.GlobalDescriptors;
        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> AssetsDescriptors => Data.AssetsDescriptors;

        #region Initialize
        public void Awake()
        {
        }

        public void Start() => LoadAllVehiclesConfigurations();


        #endregion


        public void AfterRenderExtraStuff(VehicleAI thiz, ushort vehicleID, ref Vehicle vehicleData, RenderManager.CameraInfo cameraInfo, InstanceID id, Vector3 position, Quaternion rotation, Vector4 tyrePosition, Vector4 lightState, Vector3 scale, Vector3 swayPosition, bool underground, bool overground)
        {


            if (thiz.m_info == null || thiz.m_info.m_vehicleAI == null || thiz.m_info.m_subMeshes == null)
            {
                return;
            }

            GetTargetDescriptor(thiz.m_info, vehicleID, out _, out ILayoutDescriptorVehicleXml targetDescriptor);

            if (targetDescriptor is LayoutDescriptorVehicleXml layoutDescriptor)
            {
                Vehicle.Flags flags = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags;
                Matrix4x4 vehicleMatrix = thiz.m_info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
                MaterialPropertyBlock materialBlock = VehicleManager.instance.m_materialBlock;
                materialBlock.Clear();

                RenderDescriptor(ref vehicleData, cameraInfo, vehicleID, position, ref vehicleMatrix, ref layoutDescriptor);
            }
            else if (WTSVehicleLiteUI.Instance.Visible)
            {
                UpdateCameraFocus(ref vehicleData, vehicleID, position);
            }
        }

        internal static void GetTargetDescriptor(VehicleInfo vehicle, int vehicleId, out ConfigurationSource source, out ILayoutDescriptorVehicleXml target, string skin = null)
        {
            if (vehicle == null)
            {
                source = ConfigurationSource.NONE;
                target = null;
                return;
            }

            var connVS = ModInstance.Controller.ConnectorVS;
            if (connVS.IsAvailable)
            {
                if (vehicleId >= 0 && connVS.GetSkinLayout(vehicle, (ushort)vehicleId, false, out target))
                {
                    source = ConfigurationSource.SKIN;
                    return;
                }
                if (!skin.IsNullOrWhiteSpace())
                {
                    target = connVS.GetSkin(vehicle, skin);
                    source = ConfigurationSource.SKIN;
                    return;
                }
            }

            if (WTSVehicleData.Instance.CityDescriptors.ContainsKey(vehicle.name))
            {
                source = ConfigurationSource.CITY;
                target = WTSVehicleData.Instance.CityDescriptors[vehicle.name];
                return;
            }

            if (WTSVehicleData.Instance.GlobalDescriptors.ContainsKey(vehicle.name))
            {
                source = ConfigurationSource.GLOBAL;
                target = WTSVehicleData.Instance.GlobalDescriptors[vehicle.name];
                return;
            }

            if (WTSVehicleData.Instance.AssetsDescriptors.ContainsKey(vehicle.name))
            {
                source = ConfigurationSource.ASSET;
                target = WTSVehicleData.Instance.AssetsDescriptors[vehicle.name];
                return;
            }

            source = ConfigurationSource.NONE;
            target = null;
        }

        internal static void SetCityDescriptor(VehicleInfo info, LayoutDescriptorVehicleXml desc)
        {
            if (desc is null)
            {
                if (WTSVehicleData.Instance.CityDescriptors.ContainsKey(info.name))
                {
                    WTSVehicleData.Instance.CityDescriptors.Remove(info.name);
                }
            }
            else
            {
                desc.VehicleAssetName = info.name;
                WTSVehicleData.Instance.CityDescriptors[info.name] = desc;
            }
            WTSVehicleData.Instance.CleanCache();
        }

        private ref Vehicle[] buffer => ref VehicleManager.instance.m_vehicles.m_buffer;

        private bool hasFixedCamera = false;

        private Coroutine currentGrabCoroutine;

        public void AskForGrab(VehicleInfo info, Action<ushort> callback)
        {
            if (currentGrabCoroutine == null)
            {
                currentGrabCoroutine = ModInstance.Controller.StartCoroutine(TryGrabCoroutine(info, callback));
            }
        }

        private IEnumerator TryGrabCoroutine(VehicleInfo info, Action<ushort> callback)
        {
            yield return null;
            for (int i = 0; i < buffer.Length; i++)
            {

                if (info == buffer[i].Info && (buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) == (Vehicle.Flags.Created | Vehicle.Flags.Spawned))
                {
                    callback((ushort)i);
                    currentGrabCoroutine = null;
                    yield break;
                }
                if (i % 1000 == 0)
                {
                    yield return null;
                }
            }
            callback(0);
            currentGrabCoroutine = null;
        }

        public bool WaitingGrab => currentGrabCoroutine != null;

        private void RenderDescriptor(ref Vehicle vehicle, RenderManager.CameraInfo cameraInfo, ushort vehicleId, Vector3 position, ref Matrix4x4 vehicleMatrix, ref LayoutDescriptorVehicleXml targetDescriptor)
        {
            ushort currentSelectedInstanceId = CalculateCurrentSelection(ref vehicle, vehicleId);
            for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
            {
                if (targetDescriptor.TextDescriptors[j] is TextToWriteOnXml descriptor && cameraInfo.CheckRenderDistance(position, WETextRenderer.RENDER_DISTANCE_FACTOR * descriptor.TextLineHeight * (descriptor.IlluminationConfig?.IlluminationType == MaterialType.OPAQUE ? 1 : 2)))
                {
                    var flags = vehicle.m_flags;
                    if ((flags & Vehicle.Flags.Inverted) != 0)
                    {
                        flags ^= Vehicle.Flags.Reversed;
                    }
                    var parentColor = vehicle.Info.m_vehicleAI.GetColor(vehicleId, ref vehicle, InfoManager.InfoMode.None);
                    bool currentTextSelected = !hasFixedCamera && WTSVehicleLiteUI.Instance.Visible && currentSelectedInstanceId == vehicleId && j == WTSVehicleLiteUI.Instance.CurrentTextSel;
                    var textPos = WETextRenderer.RenderTextMesh(null,
                          vehicleId,
                          0,
                          0,
                          ref parentColor,
                          targetDescriptor,
                          descriptor,
                          ref vehicleMatrix,
                          vehicle.Info,
                          (int)flags,
                          (int)vehicle.m_flags2,
                          currentTextSelected && WTSVehicleLiteUI.Instance.IsOnTextDimensionsView,
                          ref VehicleManager.instance.m_drawCallData.m_batchedCalls
                          );
                    if (currentTextSelected && textPos != default)
                    {
                        ToolsModifierControl.cameraController.m_targetPosition.x = textPos.x;
                        ToolsModifierControl.cameraController.m_targetPosition.z = textPos.z;
                        targetHeight = textPos.y;
                        lastFrameOverriden = SimulationManager.instance.m_currentTickIndex;
                        hasFixedCamera = true;
                        ToolsModifierControl.cameraController.SetTarget(default, default, false);
                    }
                }
            }
            CheckFocus(ref vehicle, vehicleId, position, currentSelectedInstanceId);
        }


        private void UpdateCameraFocus(ref Vehicle vehicle, ushort vehicleId, Vector3 position)
        {
            ushort currentSelectedInstanceId = CalculateCurrentSelection(ref vehicle, vehicleId);
            CheckFocus(ref vehicle, vehicleId, position, currentSelectedInstanceId);
        }

        private void CheckFocus(ref Vehicle vehicle, ushort vehicleId, Vector3 position, ushort currentSelectedInstanceId)
        {
            if (currentSelectedInstanceId == vehicleId && WTSVehicleLiteUI.Instance.Visible && (WTSVehicleLiteUI.Instance.CurrentTextSel < 0 || !hasFixedCamera))
            {
                ToolsModifierControl.cameraController.m_targetPosition.x = position.x;
                ToolsModifierControl.cameraController.m_targetPosition.z = position.z;
                targetHeight = position.y + vehicle.Info.m_mesh.bounds.center.y;
                lastFrameOverriden = SimulationManager.instance.m_currentTickIndex;
                hasFixedCamera = true;
                ToolsModifierControl.cameraController.SetTarget(default, default, false);
            }
        }

        private ushort CalculateCurrentSelection(ref Vehicle vehicle, ushort vehicleId)
        {
            ushort currentSelectedInstanceId = 0;
            if (!hasFixedCamera || lastFrameOverriden != SimulationManager.instance.m_currentTickIndex)
            {
                hasFixedCamera = false;
                currentSelectedInstanceId = WTSVehicleLiteUI.Instance.CurrentGrabbedId;
                if (currentSelectedInstanceId != 0 && WTSVehicleLiteUI.Instance.TrailerSel > 0 && WTSVehicleLiteUI.Instance.CurrentEditingInfo == vehicle.Info && vehicle.GetFirstVehicle(vehicleId) == currentSelectedInstanceId)
                {
                    currentSelectedInstanceId = vehicleId;
                }
            }

            return currentSelectedInstanceId;
        }

        #region IO 

        private static string DefaultFilename { get; } = $"{MainController.m_defaultFileNameVehiclesXml}.xml";

        public void LoadAllVehiclesConfigurations()
        {
            LogUtils.DoLog("LOADING VEHICLE CONFIG START -----------------------------");
            var errorList = new List<string>();
            Data.GlobalDescriptors.Clear();
            Data.AssetsDescriptors.Clear();
            KFileUtils.ScanPrefabsFolders<VehicleInfo>(DefaultFilename, LoadDescriptorsFromXmlAsset);
            LogUtils.DoLog($"DefaultVehiclesConfigurationFolder = {MainController.DefaultVehiclesConfigurationFolder}");
            foreach (string filename in Directory.GetFiles(MainController.DefaultVehiclesConfigurationFolder, "*.xml"))
            {
                try
                {
                    if (ModInstance.DebugMode)
                    {
                        LogUtils.DoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}");
                    }
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXmlCommon(stream, null);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoWarnLog($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}\n{e}");
                    errorList.Add($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}");
                }
            }

            if (errorList.Count > 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    title = "WTS - Errors loading vehicle Files",
                    message = string.Join("\r\n", errorList.ToArray()),
                    buttons = KwyttoDialog.basicOkButtonBar,
                    showClose = true

                });

            }

            Data.CleanCache();

            LogUtils.DoLog("LOADING VEHICLE CONFIG END -----------------------------");
        }

        private void LoadDescriptorsFromXmlCommon(FileStream stream, VehicleInfo info) => LoadDescriptorsFromXml(stream, info, ref Data.GlobalDescriptors);
        private void LoadDescriptorsFromXmlAsset(FileStream stream, VehicleInfo info) => LoadDescriptorsFromXml(stream, info, ref Data.AssetsDescriptors);
        private void LoadDescriptorsFromXml(FileStream stream, VehicleInfo info, ref SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> referenceDic)
        {
            var serializer = new XmlSerializer(typeof(ExportableLayoutDescriptorVehicleXml));

            LogUtils.DoLog($"trying deserialize: {info}");
            if (serializer.Deserialize(stream) is ExportableLayoutDescriptorVehicleXml configs && !(configs.Descriptors is null))
            {
                for (int i = 0; i < configs.Descriptors.Length; i++)
                {
                    LayoutDescriptorVehicleXml config = configs.Descriptors[i];
                    if (info != null)
                    {
                        string[] propEffName = info.name.Split(".".ToCharArray(), 2);
                        string[] xmlEffName = config.VehicleAssetName.Split(".".ToCharArray(), 2);
                        if (propEffName.Length == 2 && xmlEffName.Length == 2 && xmlEffName[1] == propEffName[1])
                        {
                            config.VehicleAssetName = info.name;
                        }
                    }
                    else if (config.VehicleAssetName == null)
                    {
                        throw new Exception("Vehicle name not set at file!!!!");
                    }
                    if (!config.IsValid())
                    {
                        if (ModInstance.DebugMode)
                        {
                            stream.Position = 0;
                            using (var sr = new StreamReader(stream))
                            {
                                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                                {
                                    title = KStr.comm_errorTitle,
                                    message = string.Format(Str.we_errorLoadingVehicleLayout_msg, info is null ? "global" : $"asset \"{info}\"", i + 1, configs.Descriptors.Length),
                                    scrollText = sr.ReadToEnd(),
                                    buttons = KwyttoDialog.basicOkButtonBar
                                });
                            }
                        }

                    }
                    else
                    {
                        referenceDic[config.VehicleAssetName] = config;
                    }
                }
            }
            else
            {
                throw new Exception("The file wasn't recognized as a valid descriptor!");
            }
        }
        #endregion
        private static float targetHeight;
        private uint lastFrameOverriden;

        public static void AfterUpdateTransformOverride(CameraController __instance)
        {
            if (LoadingManager.instance.m_loadingComplete && SimulationManager.instance.m_currentTickIndex - ModInstance.Controller.VehicleTextsSingleton.lastFrameOverriden > 24)
            {
                return;
            }
            __instance.m_minDistance = 1;
            __instance.m_unlimitedCamera = true;

            Vector3 vector = __instance.transform.position;
            vector.y = targetHeight + (Mathf.Sin(__instance.m_currentAngle.y * Mathf.Deg2Rad) * __instance.m_targetSize);
            __instance.transform.position = vector;
        }

    }
}
