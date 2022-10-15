﻿extern alias VS;
using UnityEngine;
using VS::Bridge_WE2VS;
using WriteEverywhere.Xml;

namespace WriteEverywhere.ModShared
{
    public class BridgeVSFallback : IBridge
    {
        public override int Priority { get; } = 1000;
        public override bool IsBridgeEnabled { get; } = true;
        public override bool IsAvailable { get; } = false;

        public override void ApplySkin(VehicleInfo info, string skinName, string contents) { }
        public override ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName) => null;

        public override bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout)
        {
            layout = null;
            return false;
        }

        public override Material GetSkinMaterial(VehicleInfo info, string skinName) => null;
        public override string[] ListAllSkins(VehicleInfo info) => new[] { "" };
    }
}