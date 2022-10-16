using Kwytto.Interfaces;
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
        public virtual void GetMileageParameters(ushort segmentSeedId, out MileageStartSource src, out int offset)
        {
            src = MileageStartSource.DEFAULT;
            offset = 0;
        }
        public virtual string GetStreetFullName(ushort segmentId) => NetManager.instance.GetSegmentName(segmentId);
        public abstract string GetStreetSuffix(ushort segmentId);
        public abstract string GetStreetQualifier(ushort segmentId);
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
        public abstract byte GetDirection(ushort segmentId);
    }
}
