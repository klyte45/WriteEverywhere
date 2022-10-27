using ColossalFramework;
using System.Xml.Serialization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{
    public class TextToWriteOnXml : BaseTextToWriteOnXml
    {
        [XmlIgnore]
        public TextParameterSequence ParameterSequence { get; set; }

        [XmlElement("ParameterSequenceSteps")]
        public override TextParameterSequenceXml SequenceSteps
        {
            get => ParameterSequence?.ToXml();
            set => ParameterSequence = TextParameterSequence.FromXml(value);
        }

        public override string GetValueAsUri() => Value.ToString();

        public override int GetParamIdx() => Value?.GetParamIdx ?? -1;

        public override void SetDefaultParameterValueAsString(string value, TextRenderingClass renderingClass = TextRenderingClass.Any)
        {
            parameterValue = value.IsNullOrWhiteSpace() ? null : new TextParameterWrapper(value, renderingClass);
        }

        [XmlElement("BackgroundMeshSettings")]
        public BackgroundMesh BackgroundMeshSettings { get; set; } = new BackgroundMesh();

        [XmlIgnore]
        public TextParameterWrapper Value => parameterValue;

        protected override ITextParameterWrapper ParameterValue => parameterValue;

        [XmlIgnore]
        private TextParameterWrapper parameterValue;

        [XmlAttribute("AssetEditorPreviewContentType")]
        public TextContent assetEditorPreviewContentType = TextContent.None;

        [XmlAttribute("AssetEditorPreviewText")]
        public string AssetEditorPreviewText { get; set; }

    }
}
