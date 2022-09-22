extern alias VS;
using ColossalFramework;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using VS::Bridge_WE2VS;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Rendering;
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

            GetTargetDescriptor(thiz.m_info, vehicleID, out _, out LayoutDescriptorVehicleXml targetDescriptor);

            if (targetDescriptor != null)
            {
                Vehicle.Flags flags = VehicleManager.instance.m_vehicles.m_buffer[vehicleID].m_flags;
                Matrix4x4 vehicleMatrix = thiz.m_info.m_vehicleAI.CalculateBodyMatrix(flags, ref position, ref rotation, ref scale, ref swayPosition);
                MaterialPropertyBlock materialBlock = VehicleManager.instance.m_materialBlock;
                materialBlock.Clear();

                RenderDescriptor(ref vehicleData, cameraInfo, vehicleID, position, ref vehicleMatrix, ref targetDescriptor);
            }
        }

        internal static void GetTargetDescriptor(VehicleInfo vehicle, int vehicleId, out ConfigurationSource source, out LayoutDescriptorVehicleXml target, string skin = null)
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
                WTSVehicleData.Instance.CityDescriptors[info.name] = desc;
            }
            WTSVehicleData.Instance.CleanCache();
        }

        private void RenderDescriptor(ref Vehicle v, RenderManager.CameraInfo cameraInfo, ushort vehicleId, Vector3 position, ref Matrix4x4 vehicleMatrix, ref LayoutDescriptorVehicleXml targetDescriptor)
        {
            var instance = VehicleManager.instance;
            for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
            {
                if (targetDescriptor.TextDescriptors[j] is BoardTextDescriptorGeneralXml descriptor && cameraInfo.CheckRenderDistance(position, WETextRenderer.RENDER_DISTANCE_FACTOR * descriptor.TextLineHeight * (descriptor.IlluminationConfig?.IlluminationType == MaterialType.OPAQUE ? 1 : 2)))
                {
                    var flags = v.m_flags;
                    if ((flags & Vehicle.Flags.Inverted) != 0)
                    {
                        flags ^= Vehicle.Flags.Reversed;
                    }
                    ref Vehicle vehicle = ref instance.m_vehicles.m_buffer[vehicleId];
                    var parentColor = vehicle.Info.m_vehicleAI.GetColor(vehicleId, ref vehicle, InfoManager.InfoMode.None);
                    WETextRenderer.RenderTextMesh(vehicleId,
                        0,
                        0,
                        ref parentColor,
                        targetDescriptor,
                        descriptor,
                        ref vehicleMatrix,
                        vehicle.Info,
                        (int)flags,
                        (int)v.m_flags2,
                        false,
                        ref VehicleManager.instance.m_drawCallData.m_batchedCalls
                        );
                }
            }

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
                                KwyttoDialog.ShowModal( new KwyttoDialog.BindProperties
                                {
                                    title = KStr.comm_errorTitle,
                                    message = string.Format(Str.we_errorLoadingVehicleLayout_msg, info is null ? "global" : $"asset \"{info}\"",i + 1, configs.Descriptors.Length),
                                    scrollText = sr.ReadToEnd(),
                                    buttons = KwyttoDialog.basicOkButtonBar
                                }                                   );
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


    }
}
