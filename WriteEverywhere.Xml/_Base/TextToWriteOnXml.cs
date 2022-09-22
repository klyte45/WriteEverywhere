using ColossalFramework;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    [XmlRoot("textDescriptor")]
    public abstract class BaseTextToWriteOnXml : IParameterizableVariable, ILibable
    {
        #region Line dimensions
        [XmlElement("WriteLineMaxDimensions")]
        public Vector2Xml LineMaxDimensions { get; set; } = new Vector2Xml { X = 2, Y = .5f };
        public float TextLineHeight => LineMaxDimensions.Y;
        public float MaxWidthMeters => LineMaxDimensions.X;
        #endregion


        [XmlAttribute("applyOverflowResizingOnY")]
        public bool m_applyOverflowResizingOnY = false;


        [XmlAttribute("textContentV2")]
        public TextContent textContent = TextContent.None;
        [XmlIgnore]
        private ITextParameterWrapper parameterValue;

        [XmlAttribute("value")]
        public string ValueAsUri
        {
            get => parameterValue?.ToString().TrimToNull();
            set => SetDefaultParameterValueAsString(value);
        }

        public abstract ITextParameterWrapper GenerateParamVal(string uri, TextRenderingClass clazz);
        public string SetDefaultParameterValueAsString(string value, TextRenderingClass renderingClass = TextRenderingClass.Any) => (parameterValue = value.IsNullOrWhiteSpace() ? null : GenerateParamVal(value, renderingClass))?.ToString();


        [XmlElement("ParameterSequenceSteps")]
        public abstract TextParameterSequenceXml SequenceSteps { get; set; }

        [XmlAttribute("overrideFont")] public string m_overrideFont;
        [XmlAttribute("fontClass")] public FontClass m_fontClass = FontClass.Regular;

        [XmlAttribute("allCaps")]
        public bool m_allCaps = false;
        [XmlAttribute("applyAbbreviations")]
        public bool m_applyAbbreviations = false;
        [XmlAttribute("prefix")]
        public string m_prefix = "";
        [XmlAttribute("suffix")]
        public string m_suffix = "";
        [XmlAttribute("verticalLineAlignment")]
        public float m_verticalAlignment = .5f;
        [XmlAttribute("horizontalLineAlignment")]
        public float m_horizontalAlignment = .5f;

        [XmlAttribute("saveName")]
        public string SaveName { get; set; }

        [XmlElement("PlacingSettings")]
        public PlacingSettings PlacingConfig { get; set; } = new PlacingSettings();
        [XmlElement("ColoringSettings")]
        public ColoringSettings ColoringConfig { get; set; } = new ColoringSettings();
        [XmlElement("IlluminationSettings")]
        public IlluminationSettings IlluminationConfig { get; set; } = new IlluminationSettings();
        [XmlElement("MultiItemSettings")]
        public SubItemSettings MultiItemSettings { get; set; } = new SubItemSettings();
        [XmlElement("AnimationSettings")]
        public AnimationSettings AnimationSettings { get; set; } = new AnimationSettings();
        [XmlElement("ParameterDisplayName")]
        public string ParameterDisplayName { get; set; } = "";
        public string GetParameterDisplayName() => ParameterDisplayName ?? SaveName;
        public TextContent GetTextContent() => textContent;
        public abstract string GetValueAsUri();
        public abstract int GetParamIdx();
    }
}
