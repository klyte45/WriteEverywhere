extern alias WE;

using Kwytto.Interfaces;
using UnityEngine;

namespace Bridge_WE2VS
{
    public abstract class IBridge : IBridgePrioritizable
    {
        public abstract bool IsAvailable { get; }

        public abstract int Priority { get; }
        public abstract bool IsBridgeEnabled { get; }

        public abstract bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out ILayoutDescriptorVehicleXml layout);
        public abstract string[] ListAllSkins(VehicleInfo info);
        public abstract ILayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName);
        public abstract Material GetSkinMaterial(VehicleInfo info, string skinName);
        public abstract void ApplySkin(VehicleInfo info, string skinName, string contents);
    }
}
