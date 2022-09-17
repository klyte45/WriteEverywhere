using ColossalFramework;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WriteEverywhere.Xml
{
    public class FrameMesh
    {

        [XmlIgnore]
        public Color OutsideColor { get => m_cachedOutsideColor; set => m_cachedOutsideColor = value; }
        [XmlIgnore]
        public Color m_cachedOutsideColor = Color.gray;
        [XmlAttribute("color")]
        public string OutsideColorStr { get => m_cachedOutsideColor == null ? null : ColorExtensions.ToRGB(OutsideColor); set => OutsideColor = value.IsNullOrWhiteSpace() ? default : ColorExtensions.FromRGB(value); }

        [XmlIgnore]
        public Color GlassColor
        {
            get => m_cachedGlassColor;
            set => m_cachedGlassColor = value;
        }
        [XmlIgnore]
        public Color m_cachedGlassColor = Color.black;
        [XmlAttribute("glassColor")]
        public string GlassColorStr { get => m_cachedGlassColor == null ? null : ColorExtensions.ToRGB(GlassColor); set => GlassColor = value.IsNullOrWhiteSpace() ? default : ColorExtensions.FromRGB(value); }

        [XmlAttribute("inheritColor")]
        public bool m_inheritColor = false;

        [XmlElement("backSize")]
        public Vector2Xml BackSize
        {
            get => m_backSize; set
            {
                ClearCacheArray(); m_backSize = value;
            }
        }
        [XmlElement("backOffset")]
        public Vector2Xml BackOffset
        {
            get => m_backOffset; set
            {
                ClearCacheArray(); m_backOffset = value;
            }
        }
        [XmlAttribute("frontDepth")]
        public float FrontDepth
        {
            get => m_frontDepth; set
            {
                ClearCacheArray(); m_frontDepth = value;
            }
        }
        [XmlAttribute("glassTransparency")]
        public float GlassTransparency
        {
            get => m_glassTransparency; set
            {
                ClearCacheArray(); m_glassTransparency = value;
            }
        }
        [XmlAttribute("glassSpecularLevel")]
        public float GlassSpecularLevel
        {
            get => m_glassSpecularLevel; set
            {
                ClearCacheArray(); m_glassSpecularLevel = value;
            }
        }
        [XmlAttribute("backDepth")]
        public float BackDepth
        {
            get => m_backDepth; set
            {
                ClearCacheArray(); m_backDepth = value;
            }
        }
        [XmlAttribute("frontBorderThickness")]
        public float FrontBorderThickness
        {
            get => m_frontBorderThickness; set
            {
                ClearCacheArray(); m_frontBorderThickness = value;
            }
        }

        public void ClearCacheArray() => cachedFrameArray = null;
        [XmlIgnore]
        public Vector3[] cachedFrameArray;
        [XmlIgnore]
        public Mesh meshOuterContainer;
        //   [XmlIgnore]
        //   public Mesh meshInnerContainer;
        [XmlIgnore]
        public Mesh meshGlass;
        [XmlIgnore]
        public Texture2D cachedGlassMain;
        [XmlIgnore]
        public Texture2D cachedGlassXYS;
        [XmlIgnore]
        private Vector2Xml m_backSize = new Vector2Xml();
        [XmlIgnore]
        private Vector2Xml m_backOffset = new Vector2Xml();
        [XmlIgnore]
        private float m_frontDepth = .01f;
        [XmlIgnore]
        private float m_backDepth = .5f;
        [XmlIgnore]
        private float m_frontBorderThickness = .01f;
        [XmlIgnore]
        private float m_glassTransparency = 0.62f;
        [XmlIgnore]
        private float m_glassSpecularLevel = 0.26f;

        ~FrameMesh()
        {
            UnityEngine.Object.Destroy(cachedGlassMain);
            UnityEngine.Object.Destroy(cachedGlassXYS);
        }
    }

}

