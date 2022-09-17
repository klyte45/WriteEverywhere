//using SpriteFontPlus;
//using SpriteFontPlus.Utility;
//using System.Collections.Generic;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
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
        public TextParameterWrapper ToWrapper(Rendering.TextRenderingClass renderClass) => new TextParameterWrapper(IsEmpty ? null : Value, renderClass);
    }
}
