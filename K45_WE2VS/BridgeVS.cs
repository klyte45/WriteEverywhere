extern alias VS;
extern alias WE;
using Bridge_WE2VS;
using UnityEngine;
using VS::VehicleSkins.ModShared;
using WriteEverywhere.Xml;

namespace K45_WE2VS
{
    public class BridgeVS : IBridge
    {
        public override bool IsAvailable { get; } = true;

        public override int Priority { get; } = 0;

        public override void ApplySkin(VehicleInfo info, string skinName, string contents) => VSFacade.Apply(info, skinName, contents);
        public override ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName) => VSFacade.GetSkin(info, skinName, out ILayoutDescriptorVehicleXml layout) ? layout : new LayoutDescriptorVehicleXml
        {
            VehicleAssetName = info.name
        };
        public override bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout) => VSFacade.GetDescriptorForVehicle(info, vehicleId, isParked, out layout);
        public override Material GetSkinMaterial(VehicleInfo info, string skinName) => VSFacade.GetSkinMaterial(info, skinName);
        public override string[] ListAllSkins(VehicleInfo info) => VSFacade.GetAvailableSkins(info);
    }
}
