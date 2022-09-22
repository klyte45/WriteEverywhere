extern alias VS;
using Kwytto.Data;
using Kwytto.Utils;
using System.Xml.Serialization;
using VS::Bridge_WE2VS;

namespace WriteEverywhere.Data
{

    [XmlRoot("WTSVehicleData")]
    public class WTSVehicleData : DataExtensionBase<WTSVehicleData>
    {
        public override string SaveId => "K45_WE_WTSVehicleData";

        [XmlElement]
        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> CityDescriptors = new SimpleXmlDictionary<string, LayoutDescriptorVehicleXml>();
        [XmlIgnore]
        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> GlobalDescriptors = new SimpleXmlDictionary<string, LayoutDescriptorVehicleXml>();
        [XmlIgnore]
        public SimpleXmlDictionary<string, LayoutDescriptorVehicleXml> AssetsDescriptors = new SimpleXmlDictionary<string, LayoutDescriptorVehicleXml>();
        [XmlAttribute("defaultFont")]
        public virtual string DefaultFont { get; set; }
        public void CleanCache()
        {

        }
    }

}
