
using ColossalFramework;
using Kwytto.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Font;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Rendering;
using WriteEverywhere.TransportLines;

namespace WriteEverywhere.Singleton
{
    public class WTSCacheSingleton : SingletonLite<WTSCacheSingleton>
    {

        private readonly NonSequentialList<SegmentItemCache> m_cacheSegments = new NonSequentialList<SegmentItemCache>();
        private readonly NonSequentialList<CityTransportLineItemCache> m_cacheTransportLines = new NonSequentialList<CityTransportLineItemCache>();
        private readonly NonSequentialList<IntercityTransportLineItemCache> m_cacheIntercityTransportLines = new NonSequentialList<IntercityTransportLineItemCache>();
        private readonly NonSequentialList<DistrictItemCache> m_cacheDistricts = new NonSequentialList<DistrictItemCache>();
        private readonly NonSequentialList<ParkItemCache> m_cacheParks = new NonSequentialList<ParkItemCache>();
        private readonly NonSequentialList<BuildingItemCache> m_cacheBuildings = new NonSequentialList<BuildingItemCache>();
        private readonly Dictionary<string, FormattableString> m_cacheStringsFormattable = new Dictionary<string, FormattableString>();

        private IEnumerator PurgeCache(CacheErasingFlags cacheFlags, InstanceID instanceID)
        {
            coroutineFlagsToErase |= cacheFlags;
            yield return 0;
            var objectsToIterate = m_cacheSegments.Values.Cast<IItemCache>()
                .Concat(m_cacheTransportLines.Values.Cast<IItemCache>())
                .Concat(m_cacheIntercityTransportLines.Values.Cast<IItemCache>())
                .Concat(m_cacheDistricts.Values.Cast<IItemCache>())
                .Concat(m_cacheParks.Values.Cast<IItemCache>())
                .Concat(m_cacheBuildings.Values.Cast<IItemCache>()).ToList();
            for (int i = 0; i < objectsToIterate.Count; i++)
            {
                objectsToIterate[i].PurgeCache(coroutineFlagsToErase, instanceID);
                if (i % 100 == 99)
                {
                    yield return 0;
                }
            }
            coroutineFlagsToErase = 0;
            instance.lastCoroutine = null;
        }

        private CacheErasingFlags coroutineFlagsToErase;
        private Coroutine lastCoroutine;


        private static void DoClearCacheCoroutineStart(CacheErasingFlags flags)
        {
            if (!(instance.lastCoroutine is null))
            {
                ModInstance.Controller.StopCoroutine(instance.lastCoroutine);
            }
            instance.lastCoroutine = ModInstance.Controller?.StartCoroutine(instance.PurgeCache(flags, default));
        }

        public static void ClearCacheSegmentNameParam() => DoClearCacheCoroutineStart(CacheErasingFlags.SegmentNameParam);

        public static void ClearCacheDistrictName() => DoClearCacheCoroutineStart(CacheErasingFlags.DistrictName | CacheErasingFlags.LineName);

        public static void ClearCacheCityName()
        {
            if (instance.m_cacheDistricts.TryGetValue(0, out DistrictItemCache cache))
            {
                cache.PurgeCache(CacheErasingFlags.DistrictName, default);
            }
        }

        public static void ClearCacheParkName() => DoClearCacheCoroutineStart(CacheErasingFlags.ParkName | CacheErasingFlags.LineName);
        public static void ClearCacheDistrictArea() => DoClearCacheCoroutineStart(CacheErasingFlags.DistrictArea | CacheErasingFlags.ParkArea);
        public static void ClearCacheBuildingName(ushort? buildingId)
        {
            if (buildingId is ushort buildingIdSh)
            {
                ClearCacheBuildingName(buildingIdSh);
            }
            else
            {
                DoClearCacheCoroutineStart(CacheErasingFlags.BuildingName | CacheErasingFlags.LineName);
            }
        }
        public static void ClearCacheBuildingName(ushort buildingId)
        {
            if (instance.m_cacheBuildings.TryGetValue(buildingId, out BuildingItemCache cache))
            {
                cache.PurgeCache(CacheErasingFlags.BuildingName, new InstanceID { Building = buildingId });
            }
        }
        public static void ClearCacheBuildingAll(ushort buildingId)
        {
            if (instance.m_cacheBuildings.TryGetValue(buildingId, out BuildingItemCache cache))
            {
                cache.PurgeCache(CacheErasingFlags.BuildingName | CacheErasingFlags.BuildingPosition, new InstanceID { Building = buildingId });
            }
        }


        public static void ClearCacheSegmentSeed() => DoClearCacheCoroutineStart(CacheErasingFlags.SegmentNameParam | CacheErasingFlags.SegmentSize);

        public static void ClearCacheSegmentSize() => DoClearCacheCoroutineStart(CacheErasingFlags.SegmentSize);

        public static void ClearCacheSegmentSize(ushort segmentId)
        {
            if (instance.m_cacheSegments.TryGetValue(segmentId, out SegmentItemCache cache))
            {
                cache.PurgeCache(CacheErasingFlags.SegmentSize, new InstanceID { NetSegment = segmentId });
            }
        }
        public static void ClearCacheSegmentNameParam(ushort segmentId)
        {
            if (instance.m_cacheSegments.TryGetValue(segmentId, out SegmentItemCache cache))
            {
                cache.PurgeCache(CacheErasingFlags.SegmentNameParam, new InstanceID { NetSegment = segmentId });
            }
        }

        public static void ClearCacheVehicleNumber() => DoClearCacheCoroutineStart(CacheErasingFlags.VehicleParameters);

        public static void ClearCacheLineId() => DoClearCacheCoroutineStart(CacheErasingFlags.VehicleParameters | CacheErasingFlags.LineId | CacheErasingFlags.LineName);
        public static void ClearCacheLineName() => DoClearCacheCoroutineStart(CacheErasingFlags.LineName);
        public static void ClearCacheLineName(WTSLine line)
        {
            if (line.regional)
            {
                if (instance.m_cacheIntercityTransportLines.TryGetValue(line.lineId, out IntercityTransportLineItemCache cache))
                {
                    cache.PurgeCache(CacheErasingFlags.LineName, new InstanceID { NetNode = line.lineId });
                }
            }
            else
            {
                if (instance.m_cacheTransportLines.TryGetValue(line.lineId, out CityTransportLineItemCache cache))
                {
                    cache.PurgeCache(CacheErasingFlags.LineName, new InstanceID { TransportLine = line.lineId });
                }
            }

        }

        private T SafeGetter<T>(NonSequentialList<T> cacheList, long id) where T : IItemCache, new()
        {
            if (!cacheList.TryGetValue(id, out T result))
            {
                result = cacheList[id] = new T() { Id = id };
            }
            return result;
        }

        public SegmentItemCache GetSegment(ushort id) => SafeGetter(m_cacheSegments, id);
        public ITransportLineItemCache GetATransportLine(WTSLine id) => id.regional ? GetIntercityTransportLine(id.lineId) : (ITransportLineItemCache)GetCityTransportLine(id.lineId);
        public CityTransportLineItemCache GetCityTransportLine(ushort id) => SafeGetter(m_cacheTransportLines, id);
        public IntercityTransportLineItemCache GetIntercityTransportLine(ushort id) => SafeGetter(m_cacheIntercityTransportLines, id);
        public DistrictItemCache GetDistrict(ushort id) => SafeGetter(m_cacheDistricts, id);
        public ParkItemCache GetPark(ushort id) => SafeGetter(m_cacheParks, id);
        public BuildingItemCache GetBuilding(ushort id) => SafeGetter(m_cacheBuildings, id);
        public FormattableString AsFormattable(string s)
        {
            if (!m_cacheStringsFormattable.ContainsKey(s))
            {
                m_cacheStringsFormattable[s] = new FormattableString(s);
            }
            return m_cacheStringsFormattable[s];
        }

        public static BasicRenderInformation GetTextData(string text, string prefix, string suffix, DynamicSpriteFont primaryFont, string overrideFont)
        {
            string str = $"{prefix}{text}{suffix}";
            return (FontServer.instance[overrideFont] ?? primaryFont ?? FontServer.instance[WEMainController.DEFAULT_FONT_KEY])?.DrawString(ModInstance.Controller, str, default, FontServer.instance.ScaleEffective);
        }
    }
    public static class StringFormattableExtensions
    {
        public static FormattableString AsFormattable(this string s) => WTSCacheSingleton.instance.AsFormattable(s);
    }
}

