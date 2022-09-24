extern alias TLM;
using System.Collections.Generic;
using System.Linq;
using TLM::Bridge_WE2TLM;
using UnityEngine;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Data
{
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

}
