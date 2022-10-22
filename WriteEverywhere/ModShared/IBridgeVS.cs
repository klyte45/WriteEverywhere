using Kwytto.Interfaces;
using UnityEngine;
using WriteEverywhere.Layout;

namespace WriteEverywhere.ModShared
{
    public abstract class IBridgeVS : IBridgePrioritizable
    {
        public abstract bool IsAvailable { get; }

        public abstract int Priority { get; }
        public abstract bool IsBridgeEnabled { get; }

        public abstract bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout, ushort buildingId);
        public abstract string[] ListAllSkins(VehicleInfo info);
        public abstract ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName);
        public abstract Material GetSkinMaterial(VehicleInfo info, string skinName);
        public abstract void ApplySkin(VehicleInfo info, string skinName, string contents);
    }
}
