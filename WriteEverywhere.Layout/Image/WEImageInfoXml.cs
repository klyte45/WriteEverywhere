using System.Xml.Serialization;
using UnityEngine;

namespace WriteEverywhere.Xml
{
    [XmlRoot("ImageInformation")]
    public class WEImageInfoXml
    {
        [XmlElement("Borders")]
        public BorderOffsets borders;

        [XmlAttribute("pixelsPerMeters")]
        public float pixelsPerMeters = 100;

        public class BorderOffsets
        {
            [XmlAttribute("pxLeft")]
            public int left;
            [XmlAttribute("pxRight")]
            public int right;
            [XmlAttribute("pxTop")]
            public int top;
            [XmlAttribute("pxBottom")]
            public int bottom;

            public Vector4 ToWEBorder(float width, float height) => new Vector4(left / width, right / width, top / height, bottom / height);
        }
    }
}
