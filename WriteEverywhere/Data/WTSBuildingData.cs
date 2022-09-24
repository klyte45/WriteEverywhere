extern alias TLM;

using Kwytto.Data;
using Kwytto.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TLM::Bridge_WE2TLM;
using UnityEngine;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Data
{

    [XmlRoot("WTSBuildingData")]
    public class WTSBuildingData : DataExtensionBase<WTSBuildingData>
    {

        public override string SaveId => "K45_WE_WTSBuildingsData";

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
            BoardsContainers.Clear();
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
        public readonly SimpleNonSequentialList<PropLayoutCachedBuildingData[]> BoardsContainers = new SimpleNonSequentialList<PropLayoutCachedBuildingData[]>();
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
                    if (BoardsContainers.TryGetValue(buildingId, out var bbcb))
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
    }

    public class WTSBuildingDataCaches
    {
        internal Dictionary<uint, ushort> m_stopsBuildingsCache = new Dictionary<uint, ushort>();

        internal void PurgeBuildingCache(ushort buildingId)
        {
            foreach (KeyValuePair<uint, ushort> item in m_stopsBuildingsCache.Where(kvp => kvp.Value == buildingId).ToList())
            {
                m_stopsBuildingsCache.Remove(item.Key);
            }
            WTSCacheSingleton.ClearCacheBuildingAll(buildingId);
        }
        internal void PurgeStopCache(ushort stopId)
        {
            foreach (KeyValuePair<uint, ushort> item in m_stopsBuildingsCache.Where(kvp => (kvp.Key & 0xFFFF) == stopId).ToList())
            {
                m_stopsBuildingsCache.Remove(item.Key);
            }
        }
        internal void PurgeLineCache(ushort lineId)
        {
            foreach (KeyValuePair<uint, ushort> item in m_stopsBuildingsCache.Where(kvp => ((kvp.Key & 0xFFFF0000) >> 16) == lineId).ToList())
            {
                m_stopsBuildingsCache.Remove(item.Key);
            }
        }

        public ushort GetStopBuilding(ushort stopId, WTSLine line)
        {
            var id = line.GetUniqueStopId(stopId);
            if (!m_stopsBuildingsCache.TryGetValue(id, out ushort buildingId))
            {
                buildingId = ModInstance.Controller.ConnectorTLM.GetStopBuildingInternal(stopId, line);
                m_stopsBuildingsCache[id] = buildingId;
            }
            return buildingId;
        }

        internal static readonly Color[] m_colorOrder = new Color[]
        {
            Color.red,
            Color.Lerp(Color.red, Color.yellow,0.5f),
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.Lerp(Color.blue, Color.magenta,0.5f),
            Color.magenta,
            Color.white,
            Color.black,
            Color.Lerp( Color.red,                                    Color.black,0.5f),
            Color.Lerp( Color.Lerp(Color.red, Color.yellow,0.5f),     Color.black,0.5f),
            Color.Lerp( Color.yellow,                                 Color.black,0.5f),
            Color.Lerp( Color.green,                                  Color.black,0.5f),
            Color.Lerp( Color.cyan,                                   Color.black,0.5f),
            Color.Lerp( Color.blue,                                   Color.black,0.5f),
            Color.Lerp( Color.Lerp(Color.blue, Color.magenta,0.5f),   Color.black,0.5f),
            Color.Lerp( Color.magenta,                                Color.black,0.5f),
            Color.Lerp( Color.white,                                  Color.black,0.25f),
            Color.Lerp( Color.white,                                  Color.black,0.75f)
        };
    }

    public struct PropLayoutCachedBuildingData
    {
        public uint m_linesUpdateFrame;

        public Vector3 m_buildingPositionWhenGenerated;

        public Vector3 m_cachedPosition;
        public Vector3 m_cachedRotation;
        public Vector3 m_cachedScale;
        public Matrix4x4 m_cachedMatrix;

        public int m_cachedArrayRepeatTimes;
        public Vector3 m_cachedOriginalPosition;
        public Vector3 m_cachedArrayItemPace;

        public bool HasAnyBoard() => true;
        internal void ClearCache()
        {
            m_buildingPositionWhenGenerated = default;
        }
    }

}
