using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    public class BoardTextDescriptorGeneralXml : BaseTextToWriteOnXml
    {
        [XmlIgnore]
        public TextParameterSequence ParameterSequence { get; set; }

        [XmlElement("ParameterSequenceSteps")]
        public override TextParameterSequenceXml SequenceSteps
        {
            get => ParameterSequence?.ToXml();
            set => ParameterSequence = TextParameterSequence.FromXml(value);
        }

        public override ITextParameterWrapper GenerateParamVal(string uri, TextRenderingClass clazz) => new TextParameterWrapper(uri, clazz);

        public override string GetValueAsUri() => Value.ToString();

        public override int GetParamIdx() => Value?.GetParamIdx ?? -1;

        [XmlElement("BackgroundMeshSettings")]
        public BackgroundMesh BackgroundMeshSettings { get; set; } = new BackgroundMesh();

        [XmlIgnore]
        public TextParameterWrapper Value => parameterValue;
        [XmlIgnore]
        private TextParameterWrapper parameterValue;
    }
}
