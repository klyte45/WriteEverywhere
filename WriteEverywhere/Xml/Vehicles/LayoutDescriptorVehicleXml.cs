extern alias VS;
using Kwytto.Interfaces;

using Kwytto.Utils;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using VS::Bridge_WE2VS;

namespace WriteEverywhere.Xml
{
    [XmlRoot("vehicleDescriptor")]
    public class LayoutDescriptorVehicleXml : BaseWriteOnXml, ILibable, ILayoutDescriptorVehicleXml
    {
        [XmlIgnore]
        public string SaveName { get; set; }

        [XmlAttribute("vehicleAssetName")]
        public string VehicleAssetName { get => SaveName; set => SaveName = value; }

        [XmlElement("textDescriptor")]
        public TextToWriteOnXml[] TextDescriptors { get => textDescriptors; set => textDescriptors = value ?? new TextToWriteOnXml[0]; }

        [XmlAttribute("defaultFont")]
        public string FontName { get; set; }

        private VehicleInfo m_cachedInfo;
        private TextToWriteOnXml[] textDescriptors = new TextToWriteOnXml[0];

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

        BaseTextToWriteOnXml[] ILayoutDescriptorVehicleXml.TextDescriptors => textDescriptors;

        public bool IsValid()
        {
            if (TextDescriptors is null)
            {
                TextDescriptors = new TextToWriteOnXml[0];
            }
            return !(CachedInfo is null || VehicleAssetName is null);
        }
    }
}
