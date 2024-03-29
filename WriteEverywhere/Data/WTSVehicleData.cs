﻿
using Kwytto.Data;
using Kwytto.Utils;
using System.Xml.Serialization;
using WriteEverywhere.Layout;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Data
{

    [XmlRoot("VehicleData")]
    public class WTSVehicleData : DataExtensionBase<WTSVehicleData>
    {
        public override string SaveId => "K45_WE_VehicleData";

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
