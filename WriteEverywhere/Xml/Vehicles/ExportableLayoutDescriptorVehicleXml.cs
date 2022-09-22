extern alias VS;
using System.Xml;
using System.Xml.Serialization;
using VS::Bridge_WE2VS;

namespace WriteEverywhere.Xml
{
    [XmlRoot("vehicleDescriptors")]
    public class ExportableLayoutDescriptorVehicleXml
    {
        [XmlElement("vehicleDescriptor")]
        public LayoutDescriptorVehicleXml[] Descriptors { get; set; }
        [XmlIgnore]
        internal string lastLayoutVersion = null;
        [XmlAttribute("WE_layoutVersion")]
        public string LayoutVersion
        {
            get => ModInstance.FullVersion;
            set => lastLayoutVersion = value;
        }
    }
}
