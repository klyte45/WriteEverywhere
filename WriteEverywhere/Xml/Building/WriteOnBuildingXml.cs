using Kwytto.Interfaces;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    [XmlRoot("buildingConfig")]
    public class WriteOnBuildingXml : ILibable
    {
        [XmlAttribute("buildingInfoName")]
        public string BuildingInfoName { get; set; }
        [XmlElement("boardDescriptor")]
        public WriteOnBuildingPropXml[] PropInstances
        {
            get => m_propInstances;
            set
            {
                m_propInstances = value;
            }
        }
        [XmlAttribute("saveName")]
        public string SaveName { get; set; }
        [XmlAttribute("overrideFont")]
        public string FontName { get; set; }

        [XmlAttribute("stopMappingThresold")]
        public float StopMappingThresold { get; set; } = 1f;

        [XmlAttribute("versionWELastEdit")]
        public string VersionWELastEdit { get => ModInstance.FullVersion; set { } }

        [XmlAttribute("versionWECreation")]
        public string VersionWECreation { get; set; } = ModInstance.FullVersion;

        [XmlIgnore]
        protected WriteOnBuildingPropXml[] m_propInstances = new WriteOnBuildingPropXml[0];

    }

    public class ExportableBoardInstanceOnBuildingListXml : ILibable
    {
        public WriteOnBuildingPropXml[] Instances { get; set; }
        [XmlAttribute("saveName")]
        public string SaveName { get; set; }
    }
}