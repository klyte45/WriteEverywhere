extern alias WE;

using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using WE::WriteEverywhere.Xml;

namespace Bridge_WE2VS
{
    public abstract class IBridge : MonoBehaviour
    {
        public abstract bool IsAvailable { get; }
        public abstract bool GetSkinLayout(VehicleInfo info, ushort vehicleId, bool isParked, out LayoutDescriptorVehicleXml layout);
        public abstract string[] ListAllSkins(VehicleInfo info);
        public abstract LayoutDescriptorVehicleXml GetSkin(VehicleInfo info, string skinName);
        public abstract Material GetSkinMaterial(VehicleInfo info, string skinName);
        public abstract void ApplySkin(VehicleInfo info, string skinName, string contents);
    }


    public class LayoutDescriptorVehicleXml : BaseWriteOnXml, ILibable
    {
        [XmlIgnore]
        public string SaveName { get; set; }

        [XmlAttribute("vehicleAssetName")]
        public string VehicleAssetName { get => SaveName; set => SaveName = value; }

        [XmlElement("textDescriptor")]
        public BaseTextToWriteOnXml[] TextDescriptors { get => textDescriptors; set => textDescriptors = value ?? new BaseTextToWriteOnXml[0]; }

        [XmlAttribute("defaultFont")]
        public string FontName { get; set; }

        private VehicleInfo m_cachedInfo;
        private BaseTextToWriteOnXml[] textDescriptors = new BaseTextToWriteOnXml[0];

        [XmlIgnore]
        internal VehicleInfo CachedInfo
        {
            get
            {
                if (m_cachedInfo is null && VehicleAssetName != null && (m_cachedInfo = VehiclesIndexes.instance.PrefabsData.Values.Where(x => x.PrefabName == VehicleAssetName).FirstOrDefault().Info as VehicleInfo) is null)
                {
                    VehicleAssetName = null;
                }
                return m_cachedInfo;
            }
        }

        public override PrefabInfo TargetAssetParameter => CachedInfo;

        public override TextRenderingClass RenderingClass => TextRenderingClass.Vehicle;

        public override string DescriptorOverrideFont => FontName;

        public bool IsValid()
        {
            if (TextDescriptors is null)
            {
                TextDescriptors = new BaseTextToWriteOnXml[0];
            }
            return !(CachedInfo is null || VehicleAssetName is null);
        }
    }
}
