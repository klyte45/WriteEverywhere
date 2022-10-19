extern alias CD;

using Bridge_WE2CD;
using CD::CustomData.Overrides;
using ColossalFramework.Plugins;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.ModShared;
using WriteEverywhere.Singleton;

namespace K45_WE2CD
{
    public class BridgeCD : IBridge
    {
        public BridgeCD()
        {
            if (!PluginManager.instance.GetPluginsInfo().Any(x => x.assemblyCount > 0 && x.isEnabled && x.ContainsAssembly(typeof(CDFacade).Assembly)))
            {
                throw new Exception("The Custom Data bridge isn't available due to the mod not being active. Using fallback!");
            }
            CDFacade.Instance.EventOnBuildingNameGenStrategyChanged += () =>
            {
                WTSCacheSingleton.ClearCacheBuildingName(null);
                WEFacade.BuildingPropsSingleton.ResetLines();
            };
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
