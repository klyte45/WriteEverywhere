using ColossalFramework;
using ColossalFramework.Globalization;
using ICities;
using Kwytto;
using Kwytto.Data;
using Kwytto.Utils;
using WriteEverywhere.Rendering;
using WriteEverywhere.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using static Kwytto.Utils.XmlUtils;
using Kwytto.LiteUI;

namespace WriteEverywhere.Data
{
    [XmlRoot("PropLayoutData")]
    public class WTSPropLayoutData : DataExtensionLibBase<WTSPropLayoutData, LibableWriteOnXml>
    {

        public override string SaveId => "K45_WE_PropLayoutData";

        protected override void Save()
        {
           // WTSRoadNodesData.Instance.ResetCacheDescriptors();
            base.Save();
        }

        public override void LoadDefaults(ISerializableData serializableData)
        {
            base.LoadDefaults(serializableData);
            ReloadAllPropsConfigurations();
        }

        public IEnumerator FilterBy(string input, TextRenderingClass? renderClass, Wrapper<string[]> result)
        {
            yield return result.Value = m_savedDescriptorsSerialized
.Where((x) => (renderClass == null || renderClass == x.Value.m_allowedRenderClass) && (input.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Key, input, CompareOptions.IgnoreCase) >= 0))
.OrderBy((x) => ((int)(4 - x.Value.m_configurationSource)) + x.Key)
.Select(x => x.Key)
.OrderBy(x => x)
.ToArray();
        }

        [XmlElement("descriptorsData")]
        public override ListWrapper<LibableWriteOnXml> SavedDescriptorsSerialized
        {
            get => new ListWrapper<LibableWriteOnXml>() { listVal = m_savedDescriptorsSerialized.Values.Where(x => x.m_configurationSource == ConfigurationSource.CITY).ToList() };
            set => ReloadAllPropsConfigurations(value);
        }

        private static string DefaultFilename { get; } = $"{MainController.m_defaultFileNamePropsXml}.xml";
        public void ReloadAllPropsConfigurations() => ReloadAllPropsConfigurations(null);
        private void ReloadAllPropsConfigurations(ListWrapper<LibableWriteOnXml> fromCity)
        {
            m_savedDescriptorsSerialized = fromCity?.listVal?.Select(x => { x.m_configurationSource = ConfigurationSource.CITY; return x; }).GroupBy(x => x.SaveName).ToDictionary(x => x.Key, x => x.First()) ?? m_savedDescriptorsSerialized.Where(x => x.Value.m_configurationSource == ConfigurationSource.CITY).ToDictionary(x => x.Key, x => x.Value);
            LogUtils.DoLog("LOADING PROPS CONFIG START -----------------------------");
            var errorList = new List<string>();
            LogUtils.DoLog($"DefaultBuildingsConfigurationFolder = {MainController.DefaultPropsLayoutConfigurationFolder}");
            KFileUtils.EnsureFolderCreation(MainController.DefaultPropsLayoutConfigurationFolder);
            KFileUtils.ScanPrefabsFolders<PropInfo>(DefaultFilename, LoadDescriptorsFromXml);
            foreach (string filename in Directory.GetFiles(MainController.DefaultPropsLayoutConfigurationFolder, "*.xml"))
            {
                try
                {
                    if (ModInstance.DebugMode)
                    {
                        LogUtils.DoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}");
                    }
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXml(stream, null);
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
                    title = "WTS - Errors loading Files",
                    scrollText = string.Join("\r\n", errorList.ToArray()),
                    buttons = KwyttoDialog.basicOkButtonBar,
                    showClose = true

                });

            }

            LogUtils.DoLog("LOADING PROPS CONFIG END -----------------------------");
            m_savedDescriptorsSerialized = m_savedDescriptorsSerialized.Values
                .GroupBy(p => p.SaveName)
                .Select(g => g.OrderBy(x => -1 * (int)x.m_configurationSource).First())
                .ToDictionary(x => x.SaveName, x => x);
        }
        private void LoadDescriptorsFromXml(FileStream stream, PropInfo info)
        {
            var serializer = new XmlSerializer(typeof(ListWrapper<LibableWriteOnXml>));

            LogUtils.DoLog($"trying deserialize: {info}");

            if (serializer.Deserialize(stream) is ListWrapper<LibableWriteOnXml> config)
            {
                var result = new List<LibableWriteOnXml>();
                foreach (var item in config.listVal)
                {
                    if (info != null)
                    {
                        string[] propEffName = info.name.Split(".".ToCharArray(), 2);
                        string[] xmlEffName = item.PropName?.Split(".".ToCharArray(), 2);
                        if (propEffName?.Length == 2 && xmlEffName?.Length == 2 && xmlEffName[1] == propEffName[1])
                        {
                            item.PropName = info.name;
                            item.m_configurationSource = ConfigurationSource.ASSET;
                            item.SaveName = propEffName[0] + "/" + item.SaveName;
                            result.Add(item);
                        }
                        else if (propEffName[0] == xmlEffName[0])
                        {
                            item.m_configurationSource = ConfigurationSource.ASSET;
                            item.SaveName = propEffName[0] + "/" + item.SaveName;
                            result.Add(item);
                        }
                        else
                        {
                            LogUtils.DoWarnLog($"PROP NAME WAS NOT MATCHING PROP INFO! SKIPPING! (prop folder: {$"{info}" ?? "<GLOBAL>"}| descriptor: {item.SaveName} | item.m_propName: {item.PropName})");
                        }
                    }
                    else if (item.PropName == null)
                    {
                        LogUtils.DoErrorLog($"PROP NAME WAS NOT SET! (prop folder: {$"{info}" ?? "<GLOBAL>"}| descriptor: {item.SaveName})");
                        continue;
                    }
                    else
                    {
                        item.m_configurationSource = ConfigurationSource.GLOBAL;
                        result.Add(item);
                    }
                }
                m_savedDescriptorsSerialized = m_savedDescriptorsSerialized.Values.Concat(result).GroupBy(x => x.SaveName).Select(g => g.OrderByDescending(x => x.m_configurationSource).First()).ToDictionary(x => x.SaveName, x => x);
            }
            else
            {
                LogUtils.DoErrorLog("The file wasn't recognized as a valid descriptor!");
            }
        }

    }
}
