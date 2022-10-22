using UnityEngine;
using WriteEverywhere.Layout;

namespace WriteEverywhere.ModShared
{
    public class BridgeVSFallback : IBridgeVS
    {
        public override int Priority { get; } = 1000;
        public override bool IsBridgeEnabled { get; } = true;
        public override bool IsAvailable { get; } = false;

        public override void ApplySkin(VehicleInfo info, string skinName, string contents) { }
        public override ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName) => null;

        public override bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout, ushort buildingId)
        {
            layout = null;
            return false;
        }

        public override Material GetSkinMaterial(VehicleInfo info, string skinName) => null;
        public override string[] ListAllSkins(VehicleInfo info) => new[] { "" };
    }
}