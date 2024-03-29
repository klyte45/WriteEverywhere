﻿using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    public class IlluminationSettings : FlaggedSettings
    {
        [XmlAttribute("type")]
        public MaterialType IlluminationType { get; set; } = MaterialType.OPAQUE;
        [XmlAttribute("strength")]
        public float m_illuminationStrength = 1;
        [XmlAttribute("depth")]
        public float m_illuminationDepth = 0;
        [XmlAttribute("useInBg")]
        public bool m_useForBg = false;
        [XmlAttribute("blinkType")]
        public BlinkType BlinkType { get; set; } = BlinkType.None;
        [XmlElement("customBlinkParams")]
        public Vector4Xml CustomBlink { get; set; } = new Vector4Xml();
    }

}

