extern alias ADR;

using ADR::Bridge_WE2ADR;
using UnityEngine;
using WriteEverywhere.Singleton;
using static CardinalPoint;

namespace WriteEverywhere.Rendering
{
    public class SegmentItemCache : IItemCache
    {
        public ushort segmentId;
        public long? Id { get => segmentId; set => segmentId = (ushort)(value ?? 0); }
        public FormattableString FullStreetName
        {
            get
            {
                if (fullStreetName is null)
                {
                    fullStreetName = ModInstance.Controller.ConnectorADR.GetStreetFullName(segmentId);
                }
                return fullStreetName.AsFormattable();
            }
        }
        public FormattableString StreetName
        {
            get
            {
                if (streetName is null)
                {
                    streetName = (NetManager.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.CustomName) == 0
                        ? ModInstance.Controller.ConnectorADR.GetStreetSuffix(segmentId)
                        : ModInstance.Controller.ConnectorADR.GetStreetSuffixCustom(segmentId);
                }
                return streetName.AsFormattable();
            }
        }
        public FormattableString StreetQualifier
        {
            get
            {
                if (streetQualifier is null)
                {
                    streetQualifier = (NetManager.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.CustomName) == 0
                        ? ModInstance.Controller.ConnectorADR.GetStreetQualifier(segmentId)
                        : ModInstance.Controller.ConnectorADR.GetStreetQualifierCustom(segmentId);
                }
                return streetQualifier.AsFormattable();
            }
        }

        public byte ParkId
        {
            get
            {
                if (parkId is null)
                {
                    parkId = DistrictManager.instance.GetPark(NetManager.instance.m_segments.m_buffer[segmentId].m_middlePosition);
                }
                return parkId ?? 0;
            }
        }

        public byte DistrictId
        {
            get
            {
                if (districtId is null)
                {
                    districtId = DistrictManager.instance.GetDistrict(NetManager.instance.m_segments.m_buffer[segmentId].m_middlePosition);
                }
                return districtId ?? 0;
            }
        }
        public string Direction
        {
            get
            {
                if (direction8 is null)
                {
                    direction8 = ModInstance.Controller.ConnectorADR.GetDirection(segmentId);
                }
                return ((Cardinal16)((direction8 ?? 0) * 2)).ValueToI18n();
            }
        }

        public int StartMileageMeters
        {
            get
            {
                if (startMileageMeters is null)
                {
                    FillHwParams();
                }
                return startMileageMeters ?? 0;
            }
        }

        public int EndMileageMeters
        {
            get
            {
                if (endMileageMeters is null)
                {
                    FillHwParams();
                }
                return endMileageMeters ?? 0;
            }
        }

        public ushort OutsideConnectionId
        {
            get
            {
                if (outsideConnectionId is null)
                {
                    ref NetSegment currSegment = ref NetManager.instance.m_segments.m_buffer[segmentId];
                    ref NetNode nodeEnd = ref NetManager.instance.m_nodes.m_buffer[currSegment.m_endNode];
                    ref NetNode nodeStart = ref NetManager.instance.m_nodes.m_buffer[currSegment.m_startNode];
                    outsideConnectionId = nodeEnd.m_building > 0 && BuildingManager.instance.m_buildings.m_buffer[nodeEnd.m_building].Info.m_buildingAI is OutsideConnectionAI
                        ? nodeEnd.m_building
                        : nodeStart.m_building > 0 && BuildingManager.instance.m_buildings.m_buffer[nodeStart.m_building].Info.m_buildingAI is OutsideConnectionAI
                            ? nodeStart.m_building
                            : (ushort?)0;
                }
                return outsideConnectionId ?? 0;
            }
        }

        private string fullStreetName;
        private string streetName;
        private string streetQualifier;
        private string postalCode;
        private byte? parkId;
        private byte? districtId;
        private int? startMileageMeters;
        private int? endMileageMeters;
        private ushort? outsideConnectionId;
        private byte? direction8;
        private float? distanceFromCenter;


        private void FillHwParams()
        {
            ModInstance.Controller.ConnectorADR.GetMileageParameters(NetManager.instance.m_segments.m_buffer[segmentId].m_nameSeed, out var src, out var mileageOffset);
            startMileageMeters = SegmentUtils.GetNumberAt(0, segmentId, src, mileageOffset, out _);
            endMileageMeters = SegmentUtils.GetNumberAt(1, segmentId, src, mileageOffset, out _);
        }

        public void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refId)
        {
            if (cacheToPurge.Has(CacheErasingFlags.SegmentNameParam))
            {
                fullStreetName = null;
                streetName = null;
                streetQualifier = null;
                direction8 = null;
            }

            if (cacheToPurge.Has(CacheErasingFlags.PostalCodeParam))
            {
                postalCode = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.BuildingName | CacheErasingFlags.SegmentSize | CacheErasingFlags.OutsideConnections))
            {
                outsideConnectionId = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.DistrictArea))
            {
                districtId = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.ParkArea))
            {
                parkId = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.SegmentSize))
            {
                startMileageMeters = null;
                endMileageMeters = null;
                distanceFromCenter = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.PostalCodeParam))
            {
                distanceFromCenter = null;
            }
        }

        internal float GetMetersAt(float pos) => Mathf.Lerp(StartMileageMeters, EndMileageMeters, pos);
    }

}
