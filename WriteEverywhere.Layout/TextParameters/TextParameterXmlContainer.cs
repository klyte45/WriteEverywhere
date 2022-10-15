using System.Xml.Serialization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{
    public class TextParameterXmlContainer
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
        [XmlAttribute("isEmpty")]
        public bool IsEmpty { get; set; }

        public static TextParameterXmlContainer FromWrapper(TextParameterWrapper input) => new TextParameterXmlContainer
        {
            IsEmpty = input.IsEmpty,
            Value = input.ToString()
        };
        public TextParameterWrapper ToWrapper(TextRenderingClass renderClass) => new TextParameterWrapper(IsEmpty ? null : Value, renderClass);
    }
}
