using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    public class PlacingSettings
    {
        [XmlAttribute("cloneInvertHorizontalAlign")]
        public bool m_invertYCloneHorizontalAlign;
        [XmlAttribute("clone180DegY")]
        public bool m_create180degYClone;
        [XmlAttribute("mirrored")]
        public bool m_mirrored = true;

        [XmlElement("position")]
        public Vector3Xml Position { get; set; } = new Vector3Xml();
        [XmlElement("rotation")]
        public Vector3Xml Rotation { get; set; } = new Vector3Xml();
    }
}



