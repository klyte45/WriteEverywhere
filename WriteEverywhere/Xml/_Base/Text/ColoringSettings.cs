using ColossalFramework;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WriteEverywhere.Xml
{
    public class ColoringSettings
    {
        [XmlAttribute("useContrastColor")]
        public bool UseContrastColor { get => ColorSource == ColoringSource.Contrast; set => ColorSource = value ? ColoringSource.Contrast : ColoringSource.Fixed; }
        [XmlIgnore]
        public Color m_cachedColor = Color.white;
        [XmlAttribute("color")]
        public string FixedColor { get => m_cachedColor == Color.clear ? null : ColorExtensions.ToRGB(m_cachedColor); set => m_cachedColor = value.IsNullOrWhiteSpace() ? Color.clear : ((Color?)ColorExtensions.FromRGBSafe(value)) ?? Color.clear; }
        [XmlIgnore]
        public Color m_cachedBackColor = Color.clear;

        [XmlAttribute("backColorIsColor")]
        public bool UseFrontColorAsBackColor { get => m_cachedBackColor == Color.clear; set => m_cachedBackColor = value ? Color.clear : Color.black; }
        [XmlAttribute("backColor")]
        public string BackColor { get => m_cachedBackColor == Color.clear ? null : ColorExtensions.ToRGB(m_cachedBackColor); set => m_cachedBackColor = value.IsNullOrWhiteSpace() ? Color.clear : ((Color?)ColorExtensions.FromRGBSafe(value)) ?? Color.clear; }
        [XmlAttribute("colorSource")]
        public ColoringSource ColorSource { get; set; } = ColoringSource.Fixed;

        public enum ColoringSource
        {
            Fixed,
            Contrast,
            Parent
        }
    }

}

