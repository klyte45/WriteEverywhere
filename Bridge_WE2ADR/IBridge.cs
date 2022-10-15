﻿using Kwytto.Interfaces;
using System.Collections;
using UnityEngine;
using static Bridge_WE2ADR.SegmentUtils;

namespace Bridge_WE2ADR
{
    public abstract class IBridge : IBridgePrioritizable
    {
        public abstract bool AddressesAvailable { get; }
        public abstract int Priority { get; }
        public abstract bool IsBridgeEnabled { get; }

        public abstract Color GetDistrictColor(ushort districtId);
        public abstract Vector2 GetStartPoint();
        public virtual string GetStreetFullName(ushort segmentId) => NetManager.instance.GetSegmentName(segmentId);
        public abstract string GetStreetSuffix(ushort segmentId);
        public abstract string GetStreetQualifier(ushort segmentId);
        public abstract string GetStreetPostalCode(Vector3 position, ushort segmentId);
        public abstract bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName);
        public virtual string GetStreetSuffixCustom(ushort idx)
        {
            string result = GetStreetFullName(idx);
            //if (result.Contains(" "))
            //{
            //    switch (WTSRoadNodesData.Instance.RoadQualifierExtraction)
            //    {
            //        case RoadQualifierExtractionMode.START:
            //            result = result.Substring(result.IndexOf(' ') + 1);
            //            break;
            //        case RoadQualifierExtractionMode.END:
            //            result = result.Substring(0, result.LastIndexOf(' '));
            //            break;
            //    }
            //}
            return result;
        }
        public virtual string GetStreetQualifierCustom(ushort idx)
        {
            string result = GetStreetFullName(idx);
            //if (result.Contains(" "))
            //{
            //    switch (WTSRoadNodesData.Instance.RoadQualifierExtraction)
            //    {
            //        case RoadQualifierExtractionMode.START:
            //            result = result.Substring(0, result.IndexOf(' '));
            //            break;
            //        case RoadQualifierExtractionMode.END:
            //            result = result.Substring(result.LastIndexOf(' ') + 1);
            //            break;
            //    }
            //    return result;
            //}
            return "";
        }

        public abstract AdrHighwayParameters GetHighwayData(ushort seedId);
        public abstract AdrHighwayParameters GetHighwayTypeData(string typeName);
        public abstract IEnumerator ListAllAvailableHighwayTypes(string filterText, out string[] result);

        public class AdrHighwayParameters
        {
            public string layoutName;
            public string detachedStr;
            public string hwIdentifier;
            public string shortCode;
            public string longCode;
            public Color hwColor;
            public int mileageOffset;
            public MileageStartSource mileageSrc;
            public MileageStartSource axis;
        }

        public abstract byte GetDirection(ushort segmentId);
        public abstract float GetDistanceFromCenter(ushort segmentId);
    }
}
