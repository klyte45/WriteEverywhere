using ColossalFramework;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{
    public class BackgroundMesh
    {
        [XmlElement("size")]
        public Vector2Xml Size
        {
            get => m_size; set
            {
                FrameMeshSettings.cachedFrameArray = null;
                m_size = value;
            }
        }

        [XmlIgnore]
        private Vector2Xml m_size = new Vector2Xml();
        [XmlIgnore]
        public Color m_bgMainColor;
        [XmlAttribute("color")]
        public string BgColorStr { get => ColorExtensions.ToRGB(m_bgMainColor); set => m_bgMainColor = ColorExtensions.FromRGBSafe(value) ?? default; }

        [XmlIgnore]
        public Color m_cachedBackColor = Color.white;
        [XmlAttribute("backColor")]
        public string BackColorStr { get => ColorExtensions.ToRGB(m_cachedBackColor); set => m_cachedBackColor = ((Color?)ColorExtensions.FromRGBSafe(value)) ?? Color.black; }

        [XmlAttribute("useFrame")]
        public bool m_useFrame = false;
        [XmlAttribute("textVerticalAlignment")]
        public float m_verticalAlignment = 0.5f;
        [XmlAttribute("textHorizontalAlignment")]
        public float m_horizontalAlignment = 0.5f;
        [XmlElement("frame")]
        public FrameMesh FrameMeshSettings { get; set; } = new FrameMesh();


        [XmlIgnore]
        public TextParameterWrapper BgImage => bgImage;
        [XmlIgnore]
        private TextParameterWrapper bgImage;

        [XmlAttribute("bgImage")]
        public string BgImageAsUri
        {
            get => bgImage?.ToString().TrimToNull();
            set => SetBgImage(value);
        }
        public string SetBgImage(string value) => (bgImage = value.IsNullOrWhiteSpace() ? null : new TextParameterWrapper(value, TextRenderingClass.BgMesh) is TextParameterWrapper tpw && tpw.ParamType == TextParameterWrapper.ParameterType.IMAGE ? tpw : null)?.ToString();
    }
}

