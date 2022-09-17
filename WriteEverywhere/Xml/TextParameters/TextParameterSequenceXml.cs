﻿using WriteEverywhere.Rendering;
using System.Xml.Serialization;

public class TextParameterSequenceXml
{
    [XmlArray("Slides")]
    [XmlArrayItem("Slide")]
    public TextParameterSequenceSlideXml[] Slides { get; set; }
    [XmlAttribute("renderClass")]
    public TextRenderingClass RenderingClass { get; set; }
}
