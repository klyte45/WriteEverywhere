extern alias TLM;

using Kwytto.Data;
using Kwytto.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Layout;

namespace WriteEverywhere.Data
{

    [XmlRoot("BuildingData")]
    public class WTSBuildingData : DataExtensionBase<WTSBuildingData>
    {

        public override string SaveId => "K45_WE_BuildingData";

        [XmlElement]
        public SimpleXmlDictionary<string, WriteOnBuildingXml> CityDescriptors = new SimpleXmlDictionary<string, WriteOnBuildingXml>();
        [XmlIgnore]
        public SimpleXmlDictionary<string, WriteOnBuildingXml> GlobalDescriptors = new SimpleXmlDictionary<string, WriteOnBuildingXml>();
        [XmlIgnore]
        public SimpleXmlDictionary<string, WriteOnBuildingXml> AssetsDescriptors = new SimpleXmlDictionary<string, WriteOnBuildingXml>();

        [XmlAttribute("defaultFont")]
        public virtual string DefaultFont { get; set; }
        public void CleanCache()
        {
            BuildingCachedPositionsData.Clear();
        }
        internal void OnBuildingChangedPosition(ushort buildingId)
        {
            cooldown = 10;
            clearCacheQueue.Add(buildingId);

            if (currentCacheCoroutine is null)
            {
                currentCacheCoroutine = ModInstance.Controller?.StartCoroutine(ClearCacheQueue());
            }
        }
        private int cooldown = 0;
        private readonly HashSet<ushort> clearCacheQueue = new HashSet<ushort>();
        private Coroutine currentCacheCoroutine;
        public readonly SimpleNonSequentialList<PropLayoutCachedBuildingData[]> BuildingCachedPositionsData = new SimpleNonSequentialList<PropLayoutCachedBuildingData[]>();
        private IEnumerator ClearCacheQueue()
        {
            yield return 0;
            do
            {

                var list = clearCacheQueue.ToList();
                foreach (var buildingId in list)
                {
                    if (--cooldown > 0)
                    {
                        yield return 0;
                    }
                    if (BuildingCachedPositionsData.TryGetValue(buildingId, out var bbcb))
                    {
                        foreach (var item in bbcb)
                        {
                            item.ClearCache();
                        }
                    }
                    CacheData.PurgeBuildingCache(buildingId);
                    clearCacheQueue.Remove(buildingId);
                    yield return 0;
                }
            } while (clearCacheQueue.Count > 0);
            currentCacheCoroutine = null;
        }

        public WTSBuildingDataCaches CacheData { get; } = new WTSBuildingDataCaches();


        [XmlElement("InsanceParameters")]
        public SimpleNonSequentialList<BuildingParametersData> Parameters
        {
            get
            {
                return m_parameters;
            }
            set
            {
                m_parameters.Clear();
                var buffer = BuildingManager.instance.m_buildings.m_buffer;
                foreach (var k in value?.Keys)
                {
                    ref Building building = ref buffer[k];
                    if (building.Info.name == value[k].assetName && (building.m_flags & Building.Flags.Created) != 0)
                    {
                        m_parameters[k] = value[k];
                    }
                }
            }
        }
        private readonly SimpleNonSequentialList<BuildingParametersData> m_parameters = new SimpleNonSequentialList<BuildingParametersData>();
    }

}
