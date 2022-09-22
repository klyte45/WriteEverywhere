using System.Xml.Serialization;

public class TextParameterSequenceSlideXml
{
    [XmlAttribute("value")]
    public string Value { get; set; }
    [XmlAttribute("frames")]
    public long Frames { get; set; }
}