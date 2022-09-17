//using WriteEverywhere.Xml;
//using UnityEngine;

//namespace WriteEverywhere.ModShared
//{
//    public class BridgeVSFallback : IBridgeVS
//    {
//        public override bool IsAvailable { get; } = false;

//        public override void ApplySkin(VehicleInfo info, string skinName, string contents) { }
//        public override LayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName) => null;

//        public override bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out LayoutDescriptorVehicleXml layout)
//        {
//            layout = null;
//            return false;
//        }

//        public override Material GetSkinMaterial(VehicleInfo info, string skinName) => null;
//        public override string[] ListAllSkins(VehicleInfo info) => new[] { "" };
//    }
//}