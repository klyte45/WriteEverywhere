extern alias VS;
extern alias WE;
using ColossalFramework.Plugins;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VS::VehicleSkins.ModShared;
using WriteEverywhere.Layout;
using WriteEverywhere.ModShared;

namespace K45_WE2VS
{
    public class BridgeVS : IBridgeVS
    {
        public BridgeVS()
        {
            if (!PluginManager.instance.GetPluginsInfo().Any(x => x.assemblyCount > 0 && x.isEnabled && x.ContainsAssembly(typeof(VSFacade).Assembly)))
            {
                throw new Exception("The Vehicle Skins bridge isn't available due to the mod not being active. Using fallback!");
            }
        }
        public override bool IsAvailable { get; } = true;
        public override bool IsBridgeEnabled { get; } = PluginUtils.VerifyModsEnabled(new Dictionary<ulong, string> { }, new List<string>
        {
          typeof(VSFacade).Assembly.GetName().Name
        }).Count > 0;

        public override int Priority { get; } = 0;

        public override void ApplySkin(VehicleInfo info, string skinName, string contents) => VSFacade.Apply(info, skinName, contents);
        public override ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName) => VSFacade.GetSkin(info, skinName, out ILayoutDescriptorVehicleXml layout) ? layout : new LayoutDescriptorVehicleXml
        {
            VehicleAssetName = info.name
        };
        public override bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout, ushort buildingId) => VSFacade.GetDescriptorForVehicle(info, vehicleId, isParked, out layout, buildingId);
        public override Material GetSkinMaterial(VehicleInfo info, string skinName) => VSFacade.GetSkinMaterial(info, skinName);
        public override string[] ListAllSkins(VehicleInfo info) => VSFacade.GetAvailableSkins(info);
    }
}
