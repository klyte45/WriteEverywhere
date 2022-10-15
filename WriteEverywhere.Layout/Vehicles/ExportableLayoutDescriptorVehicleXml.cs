

using Kwytto.Interfaces;
using System.Xml;
using System.Xml.Serialization;

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
            get => BasicIUserMod.FullVersion;
            set => lastLayoutVersion = value;
        }
    }
}
