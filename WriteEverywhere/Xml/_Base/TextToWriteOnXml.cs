using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{
    [XmlRoot("textDescriptor")]
    public class BoardTextDescriptorGeneralXml : ILibable
    {
        #region Line dimensions
        [XmlElement("WriteLineMaxDimensions")]
        public Vector2Xml LineMaxDimensions { get; set; } = new Vector2Xml();
        public float TextLineHeight => LineMaxDimensions.Y;
        public float MaxWidthMeters => LineMaxDimensions.X;
        #endregion


        [XmlAttribute("applyOverflowResizingOnY")]
        public bool m_applyOverflowResizingOnY = false;


        [XmlAttribute("textAlign")]
        public UIHorizontalAlignment m_textAlign = UIHorizontalAlignment.Center;

        [XmlAttribute("textContentV2")]
        public TextContent textContent = TextContent.None;
        [XmlAttribute("destinationReference")]
        public DestinationReference m_destinationRelative = DestinationReference.Self;
        [XmlAttribute("parameterIdx")]
        public int m_parameterIdx = 0;
        [XmlIgnore]
        private TextParameterWrapper parameterValue;
        [XmlAttribute("spriteNameV2")]
        public string OLD__SpriteName
        {
            get => null;
            set => DefaultParameterValueAsString = value;
        }

        [XmlAttribute("defaultParameterValue")]
        public string DefaultParameterValueAsString
        {
            get => parameterValue?.ToString().TrimToNull();
            set => SetDefaultParameterValueAsString(value);
        }

        public string SetDefaultParameterValueAsString(string value, TextRenderingClass renderingClass = TextRenderingClass.Any) => (parameterValue = value.IsNullOrWhiteSpace() ? null : new TextParameterWrapper(value, renderingClass))?.ToString();

        [XmlAttribute("parameterDisplayName")]
        public string ParameterDisplayName { get; set; }

        [XmlIgnore]
        public TextParameterWrapper DefaultParameterValue => parameterValue;
        [XmlIgnore]
        public TextParameterSequence ParameterSequence { get; set; }

        [XmlElement("ParameterSequenceSteps")]
        public TextParameterSequenceXml SequenceSteps
        {
            get => ParameterSequence?.ToXml();
            set => ParameterSequence = TextParameterSequence.FromXml(value);
        }


        [XmlAttribute("fixedText")]
        public string m_fixedText = "Text";
        [XmlIgnore]
        public TextParameterWrapper m_spriteParam;

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
        [XmlElement("BackgroundMeshSettings")]
        public BackgroundMesh BackgroundMeshSettings { get; set; } = new BackgroundMesh();
        [XmlElement("AnimationSettings")]
        public AnimationSettings AnimationSettings { get; set; } = new AnimationSettings();
        public bool IsParameter() => ParameterizedTextContents.Contains(textContent);
        private static readonly TextContent[] ParameterizedTextContents = new[]
        {
        TextContent.ParameterizedText,
        TextContent.ParameterizedSpriteFolder,
        TextContent.ParameterizedSpriteSingle,
        };
    }

}
