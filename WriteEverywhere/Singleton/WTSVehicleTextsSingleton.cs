
using ColossalFramework;
using ColossalFramework.Threading;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.ModShared;
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

        public void Start() => LoadAllVehiclesConfigurations(false);


        #endregion


        public void AfterRenderExtraStuff(VehicleAI thiz, ushort vehicleID, ref Vehicle vehicleData, RenderManager.CameraInfo cameraInfo, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, ref Vector3 swayPosition)
        {
            if (thiz.m_info == null || thiz.m_info.m_vehicleAI == null || thiz.m_info.m_subMeshes == null)
            {
                return;
            }

            GetTargetDescriptor(thiz.m_info, vehicleID, out _, out ILayoutDescriptorVehicleXml targetDescriptor, vehicleData.m_sourceBuilding);

            if (targetDescriptor is LayoutDescriptorVehicleXml layoutDescriptor)
            {
                Vehicle.Flags flags = SceneUtils.IsAssetEditor ? (Vehicle.Flags)AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags1 : VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags;
                Matrix4x4 vehicleMatrix = thiz.m_info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
                MaterialPropertyBlock materialBlock = VehicleManager.instance.m_materialBlock;
                materialBlock.Clear();

                RenderDescriptor(ref vehicleData, cameraInfo, vehicleID, position, thiz.m_info, ref vehicleMatrix, ref layoutDescriptor);
            }
            else if (WTSVehicleLiteUI.Instance.Visible)
            {
                UpdateCameraFocus(ref vehicleData, vehicleID, position, thiz.m_info);
            }
        }

        internal static void GetTargetDescriptor(VehicleInfo vehicle, int vehicleId, out ConfigurationSource source, out ILayoutDescriptorVehicleXml target, ushort buildingId, string skin = null)
        {
            if (SceneUtils.IsAssetEditor && vehicle != null)
            {
                vehicle = WTSVehicleLiteUI.Instance.CurrentTrailerList?.FirstOrDefault(x => x.name.EndsWith(vehicle.name));
            }

            if (vehicle == null)
            {
                source = ConfigurationSource.NONE;
                target = null;
                return;
            }

            var connVS = ModInstance.Controller.ConnectorVS;
            if (connVS.IsAvailable && (!WEFacade.IsWEVehicleEditorOpen || vehicleId != WEFacade.CurrentGrabbedVehicleId || !WEFacade.CurrentSelectedSkin.IsNullOrWhiteSpace()))
            {
                if (vehicleId >= 0 && connVS.GetSkinLayout(vehicle, (ushort)vehicleId, false, out target, buildingId))
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
        internal static void SetAssetDescriptor(VehicleInfo info, LayoutDescriptorVehicleXml desc)
        {
            if (desc is null)
            {
                if (WTSVehicleData.Instance.AssetsDescriptors.ContainsKey(info.name))
                {
                    WTSVehicleData.Instance.AssetsDescriptors.Remove(info.name);
                }
            }
            else
            {
                desc.VehicleAssetName = info.name;
                WTSVehicleData.Instance.AssetsDescriptors[info.name] = desc;
            }
            WTSVehicleData.Instance.CleanCache();
        }

        private ref Vehicle[] Buffer => ref VehicleManager.instance.m_vehicles.m_buffer;

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
            for (int i = 0; i < Buffer.Length; i++)
            {

                if (info == Buffer[i].Info && (Buffer[i].m_flags & (Vehicle.Flags.Created | Vehicle.Flags.Spawned)) == (Vehicle.Flags.Created | Vehicle.Flags.Spawned))
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

        private void RenderDescriptor(ref Vehicle vehicle, RenderManager.CameraInfo cameraInfo, ushort vehicleId, Vector3 position, VehicleInfo info, ref Matrix4x4 vehicleMatrix, ref LayoutDescriptorVehicleXml targetDescriptor)
        {
            ushort currentSelectedInstanceId = CalculateCurrentSelection(ref vehicle, vehicleId);
            for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
            {
                if (targetDescriptor.TextDescriptors[j] is TextToWriteOnXml descriptor && cameraInfo.CheckRenderDistance(position, WETextRenderer.RENDER_DISTANCE_FACTOR * descriptor.TextLineHeight * (descriptor.IlluminationConfig?.IlluminationType == MaterialType.OPAQUE ? 1 : 2)))
                {
                    var flags = SceneUtils.IsAssetEditor ? (Vehicle.Flags)AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags1 : vehicle.m_flags;
                    if ((flags & Vehicle.Flags.Inverted) != 0)
                    {
                        flags ^= Vehicle.Flags.Reversed;
                    }
                    var parentColor = vehicle.Info.m_vehicleAI.GetColor(vehicleId, ref vehicle, InfoManager.InfoMode.None);
                    bool currentTextSelected = !hasFixedCamera && WTSVehicleLiteUI.Instance.Visible && (SceneUtils.IsAssetEditor ? WTSVehicleLiteUI.Instance.CurrentEditingInfo?.name.EndsWith(info.name) ?? false : currentSelectedInstanceId == vehicleId) && j == WTSVehicleLiteUI.Instance.CurrentTextSel;
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
                          SceneUtils.IsAssetEditor ? AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags2 : (int)vehicle.m_flags2,
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
            CheckFocus(vehicleId, position, currentSelectedInstanceId, info);
        }


        private void UpdateCameraFocus(ref Vehicle vehicle, ushort vehicleId, Vector3 position, VehicleInfo info)
        {
            ushort currentSelectedInstanceId = CalculateCurrentSelection(ref vehicle, vehicleId);
            CheckFocus(vehicleId, position, currentSelectedInstanceId, info);
        }

        private void CheckFocus(ushort vehicleId, Vector3 position, ushort currentSelectedInstanceId, VehicleInfo info)
        {
            if ((SceneUtils.IsAssetEditor ? WTSVehicleLiteUI.Instance.CurrentEditingInfo?.name.EndsWith(info.name) ?? false : currentSelectedInstanceId == vehicleId) && WTSVehicleLiteUI.Instance.Visible && (WTSVehicleLiteUI.Instance.CurrentTextSel < 0 || !hasFixedCamera))
            {
                ToolsModifierControl.cameraController.m_targetPosition.x = position.x;
                ToolsModifierControl.cameraController.m_targetPosition.z = position.z;
                targetHeight = position.y + info.m_mesh.bounds.center.y;
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

        private static string DefaultFilename { get; } = $"{WEMainController.m_defaultFileNameVehiclesXml}.xml";
        public bool IsReady => LoadingCoroutineLocal is null && LoadingCoroutineAssets is null;

        private static ActionThread LoadingCoroutineAssets;
        private static ActionThread LoadingCoroutineLocal;

        public Coroutine LoadAllVehiclesConfigurations(bool sharedOnly) => IsReady ? StartCoroutine(LoadAllVehiclesConfigurations_Coroutine(sharedOnly)) : null;

        public IEnumerator LoadAllVehiclesConfigurations_Coroutine(bool sharedOnly)
        {
            LogUtils.DoLog("LOADING VEHICLE CONFIG START -----------------------------");

            var assets = new SimpleXmlDictionary<string, LayoutDescriptorVehicleXml>();
            var shared = new SimpleXmlDictionary<string, LayoutDescriptorVehicleXml>();
            if (!sharedOnly)
            {
                LoadingCoroutineAssets = ThreadHelper.CreateThread(() => ReloadVehicleAssets_thread(assets));
            }
            if (!SceneUtils.IsAssetEditor)
            {
                LoadingCoroutineLocal = ThreadHelper.CreateThread(() => ReloadVehicleShared_thread(shared));
            }
            else
            {
                Data.GlobalDescriptors.Clear();
                Data.CityDescriptors.Clear();
            }
            yield return new WaitUntil(() => IsReady);
            Data.GlobalDescriptors = shared;
            if (!sharedOnly)
            {
                Data.AssetsDescriptors = assets;
            }
            Data.CleanCache();
            LogUtils.DoLog("LOADING VEHICLE CONFIG END -----------------------------");
        }

        private static void ReloadVehicleShared_thread(SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> target)
        {
            target.Clear();
            var errorList = new List<string>();

            foreach (string filename in Directory.GetFiles(WEMainController.DefaultVehiclesConfigurationFolder, "*.xml"))
            {
                try
                {
                    Dispatcher.main.Dispatch(() => LogUtils.DoInfoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}"));
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXmlCommon(stream, target);
                    }
                }
                catch (Exception e)
                {
                    Dispatcher.main.Dispatch(() => LogUtils.DoWarnLog($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}\n{e}"));
                    errorList.Add($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}");
                }
                Dispatcher.main.Dispatch(() => LogUtils.FlushBuffer());
            }

            LoadingCoroutineLocal = null;
            if (errorList.Count > 0)
            {
                Dispatcher.main.Dispatch(() =>
                    KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                    {
                        title = "WE - Errors loading vehicle Files",
                        scrollText = string.Join("\r\n", errorList.ToArray()),
                        buttons = KwyttoDialog.basicOkButtonBar,
                        showClose = true
                    })
                );
            }
        }

        private static void ReloadVehicleAssets_thread(SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> target)
        {
            target.Clear();
            foreach (var asset in VehiclesIndexes.instance.PrefabsData.Where(x => x.Value.PackageName.TrimToNull() != null))
            {
                LoadDescriptorsFromXmlAsset(asset.Value.Info as VehicleInfo, target);
                Dispatcher.main.Dispatch(() => LogUtils.FlushBuffer());
            }
            LoadingCoroutineAssets = null;
        }

        private static void LoadDescriptorsFromXmlCommon(FileStream stream, SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> target) => LoadSingleDescriptorFromXml(stream, null, ref target);
        private static void LoadDescriptorsFromXmlAsset(VehicleInfo info, SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> target)
        {
            if (WEMainController.GetDirectoryForAssetOwn(info) is string str)
            {
                var filePath = Path.Combine(str, DefaultFilename);
                if (File.Exists(filePath))
                {
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        LoadSingleDescriptorFromXml(stream, info, ref target);
                    }
                }
            }
        }

        private static void LoadSingleDescriptorFromXml(FileStream stream, VehicleInfo info, ref SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> referenceDic)
        {
            var serializer = new XmlSerializer(typeof(LayoutDescriptorVehicleXml));

            if (ModInstance.DebugMode)
            {
                Dispatcher.main.Dispatch(() => LogUtils.DoLog($"trying deserialize: {info}"));
            }

            if (serializer.Deserialize(stream) is LayoutDescriptorVehicleXml config)
            {
                if (info != null)
                {
                    config.VehicleAssetName = info.name;
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
                            Dispatcher.main.Dispatch(() =>
                            {
                                try
                                {
                                    KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                                    {
                                        title = KStr.comm_errorTitle,
                                        message = string.Format(Str.we_errorLoadingVehicleLayout_msgSingle, info is null ? "global" : $"asset \"{info}\""),
                                        scrollText = sr.ReadToEnd(),
                                        buttons = KwyttoDialog.basicOkButtonBar
                                    });
                                }
                                catch { }
                            });
                        }
                    }

                }
                else
                {
                    referenceDic[config.VehicleAssetName] = config;
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
