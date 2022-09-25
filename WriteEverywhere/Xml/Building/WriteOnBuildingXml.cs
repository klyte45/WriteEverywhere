using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
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


        public Dictionary<int, List<Tuple<IParameterizableVariable, string>>> GetAllParametersUsedWithData() =>
           PropInstances.SelectMany(p => p.TextDescriptors
            .Where(x =>
                (x.Value?.IsParameter ?? false)
                || (x.ParameterSequence?.Any(y => y.Value?.IsParameter ?? false) ?? false)
            )
            .SelectMany(x =>
            x.textContent != TextContent.TextParameterSequence
                    ? new[] { Tuple.New(x as IParameterizableVariable, $"L: <color=yellow>{p.SaveName}</color>; T: <color=cyan>{x.SaveName}</color>", x.Value) }
                    : x.ParameterSequence.Where(y => y.Value?.IsParameter ?? false).Select(y => Tuple.New(y as IParameterizableVariable, $"L: {p.SaveName}; T: {x.SaveName}", y.Value))
            ))
            .GroupBy(x => x.First.GetParamIdx()).ToDictionary(x => x.Key, x => x.Select(y => Tuple.New(y.First, y.Second)).ToList());

    }

    public class ExportableBoardInstanceOnBuildingListXml : ILibable
    {
        public WriteOnBuildingPropXml[] Instances { get; set; }
        [XmlAttribute("saveName")]
        public string SaveName { get; set; }
    }
}