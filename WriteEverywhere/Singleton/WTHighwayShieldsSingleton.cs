using Kwytto;
using Kwytto.Utils;
using WriteEverywhere.Data;
using WriteEverywhere.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Kwytto.LiteUI;

namespace WriteEverywhere.Singleton
{
    public class WTSHighwayShieldsSingleton : MonoBehaviour
    {
        public WTSHighwayShieldsData Data => WTSHighwayShieldsData.Instance;

        public SimpleXmlDictionary<string, HighwayShieldDescriptor> CityDescriptors => Data.CityDescriptors;
        public SimpleXmlDictionary<string, HighwayShieldDescriptor> GlobalDescriptors => Data.GlobalDescriptors;

        #region Initialize
        public void Awake()
        {
        }

        public void Start() => LoadAllShieldsConfigurations();
        #endregion


        internal static void GetTargetDescriptor(string layoutName, out ConfigurationSource source, out HighwayShieldDescriptor target)
        {
            if (layoutName == null)
            {
                source = ConfigurationSource.NONE;
                target = null;
                return;
            }
            layoutName = layoutName.Trim();

            if (WTSHighwayShieldsData.Instance.CityDescriptors.ContainsKey(layoutName))
            {
                source = ConfigurationSource.CITY;
                target = WTSHighwayShieldsData.Instance.CityDescriptors[layoutName];
                return;
            }
            if (WTSHighwayShieldsData.Instance.GlobalDescriptors.ContainsKey(layoutName))
            {
                source = ConfigurationSource.GLOBAL;
                target = WTSHighwayShieldsData.Instance.GlobalDescriptors[layoutName];
                return;
            }
            source = ConfigurationSource.NONE;
            target = null;
        }

        #region IO 
        public void LoadAllShieldsConfigurations()
        {
            LogUtils.DoLog("LOADING HW SHIELDS CONFIG START -----------------------------");
            var errorList = new List<string>();
            Data.GlobalDescriptors.Clear();
            LogUtils.DoLog($"DefaultHwShieldsConfigurationFolder = {WEMainController.DefaultHwShieldsConfigurationFolder}");
            foreach (string filename in Directory.GetFiles(WEMainController.DefaultHwShieldsConfigurationFolder, "*.xml"))
            {
                try
                {
                    if (ModInstance.DebugMode)
                    {
                        LogUtils.DoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}");
                    }
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXmlCommon(stream);
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
            ModInstance.Controller.HighwayShieldsAtlasLibrary.PurgeShields();
            LogUtils.DoLog("LOADING HW SHIELDS CONFIG END -----------------------------");
        }

        private void LoadDescriptorsFromXmlCommon(FileStream stream) => LoadDescriptorsFromXml(stream, Data.GlobalDescriptors);
        private void LoadDescriptorsFromXml(FileStream stream, SimpleXmlDictionary<string, HighwayShieldDescriptor> referenceDic)
        {
            var serializer = new XmlSerializer(typeof(HighwayShieldDescriptor));
            if (serializer.Deserialize(stream) is HighwayShieldDescriptor config)
            {
                referenceDic[config.SaveName] = config;
            }
            else
            {
                throw new Exception("The file wasn't recognized as a valid descriptor!");
            }
        }
        #endregion


    }
}
