extern alias CD;

using Bridge_WE2CD;
using CD::CustomData.Overrides;
using UnityEngine;
using WriteEverywhere.Singleton;

namespace K45_WE2CD
{
    public class BridgeCD : IBridge
    {
        public BridgeCD()
        {
            CDFacade.Instance.EventOnBuildingNameGenStrategyChanged += () => WTSCacheSingleton.ClearCacheBuildingName(null);
        }

        public override bool AddressesAvailable => true;

        public override int Priority => 0;

        public override bool IsBridgeEnabled => true;

        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => CDFacade.Instance.GetStreetAndNumber(sidewalk, midPosBuilding, out number, out streetName);

        public override byte GetDirection(ushort segmentId) => CardinalPoint.GetCardinalPoint(CDFacade.Instance.GetStreetDirection(segmentId)).GetCardinalIndex8();

        public override Color GetDistrictColor(byte districtId) => CDFacade.Instance.GetDistrictColor(districtId);

        public override string GetStreetQualifier(ushort segmentId) => CDFacade.Instance.GetStreetQualifier(segmentId);

        public override string GetStreetSuffix(ushort segmentId) => CDFacade.Instance.GetStreetFull(segmentId).Replace(GetStreetQualifier(segmentId), "");
    }
}
