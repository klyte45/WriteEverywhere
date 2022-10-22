using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;

namespace WriteEverywhere.Rendering
{
    public class BuildingItemCache : IItemCache
    {
        public ushort buildingId;
        public long? Id { get => buildingId; set => buildingId = (ushort)(value ?? 0); }

        public FormattableString Name
        {
            get
            {
                if (name is null)
                {
                    name = BuildingManager.instance.GetBuildingName(buildingId, InstanceID.Empty) ?? "";
                }
                return name.AsFormattable();
            }
        }
        public ushort SegmentId
        {
            get
            {
                if (segmentId is null)
                {
                    UpdateAddressFields();
                }
                return segmentId ?? 0;
            }
        }


        public int AddressNumber
        {
            get
            {
                if (addressNumber is null)
                {
                    UpdateAddressFields();
                }
                return addressNumber ?? 0;
            }
        }
        private void UpdateAddressFields()
        {
            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            SegmentUtils.GetNearestSegment(b.CalculateSidewalkPosition(), out _, out float targetLength, out ushort segmentIdFound);
            if (segmentIdFound > 0)
            {
                ModInstance.Controller.ConnectorCD.GetMileageParameters(NetManager.instance.m_segments.m_buffer[segmentIdFound].m_nameSeed, out var mileageSrc, out var mileageOffset);
                addressNumber = SegmentUtils.CalculateBuildingAddressNumber(b.m_position, segmentIdFound, targetLength, b.m_position, mileageSrc, mileageOffset);
                segmentId = segmentIdFound;
            }
            else
            {
                addressNumber = 0;
                segmentId = 0;
            }
        }

        private string name = null;
        private ushort? segmentId;
        private int? addressNumber;
        public void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refID)
        {
            if (cacheToPurge.Has(CacheErasingFlags.BuildingName))
            {
                name = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.SegmentSize) || (cacheToPurge.Has(CacheErasingFlags.BuildingPosition) && refID.Building == buildingId))
            {
                segmentId = null;
                addressNumber = null;
            }
        }
    }

}
