extern alias ADR;

using Kwytto.Interfaces;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace WriteEverywhere.Xml
{
    [XmlRoot("textDescriptor2D")]
    public class ImageLayerTextDescriptorXml : ILibable
    {
        [XmlAttribute("textScale")]
        public float m_textScale = 1f;
        [XmlAttribute("spacingFactor")]
        public float m_charSpacingFactor = 0.85f;
        [XmlAttribute("maxWidth")]
        public int m_maxWidthPixels = 0;
        [XmlAttribute("fixedHeight")]
        public int m_fixedHeightPixels = 0;
        [XmlAttribute("applyOverflowResizingOnY")]
        public bool m_applyOverflowResizingOnY = false;

        [XmlElement("offsetUV")]
        public Vector2 OffsetUV { get; set; } = Vector2.one / 2;
        [XmlElement("pivotUV")]
        public Vector2 PivotUV { get; set; } = Vector2.one / 2;

        [XmlAttribute("textContent")]
        public TextContent m_textType = TextContent.ParameterizedText;

        [XmlIgnore]
        public TextParameterWrapper m_paramValue;
        [XmlAttribute("spriteName")]
        public string SpriteParam
        {
            get => m_paramValue?.ToString();
            set => m_paramValue = new TextParameterWrapper(value, TextRenderingClass.None);
        }

        [XmlAttribute("overrideFont")] public string m_overrideFont;
        [XmlAttribute("fontClass")] public FontClass m_fontClass = FontClass.Regular;


        [XmlAttribute("saveName")]
        public string SaveName { get; set; }

        [XmlElement("ColoringSettings")]
        public ColoringSettings ColoringConfig { get; set; } = new ColoringSettings { m_cachedColor = Color.white };

        public bool IsSpriteText()
        {
            switch (m_textType)
            {
                case TextContent.ParameterizedSpriteSingle:
                    return true;
            }
            return false;
        }

        public Vector2 GetScale(Vector2 textSize)
        {
            if (IsSpriteText())
            {
                return Vector2.one * m_textScale;
            }
            if (m_fixedHeightPixels > 0)
            {
                var heightMultiplier = m_fixedHeightPixels / textSize.y;
                var widthMultiplier = Mathf.Min(heightMultiplier, m_maxWidthPixels > 0 ? m_maxWidthPixels / textSize.x : heightMultiplier);

                return new Vector2(widthMultiplier, heightMultiplier);
            }
            else
            {
                var widthMultiplier = m_maxWidthPixels > 0 ? Mathf.Min(m_maxWidthPixels / textSize.x, m_textScale) : m_textScale;
                var heightMultiplier = m_applyOverflowResizingOnY ? widthMultiplier : m_textScale;

                return new Vector2(widthMultiplier, heightMultiplier);
            }
        }


        public Vector4 GetAreaSize(float shieldWidth, float shieldHeight, float textureWidth, float textureHeight, bool invertY = false)
        {
            var multiplier = GetScale(new Vector2(textureWidth, textureHeight));
            float textTargetHeight = textureHeight * multiplier.y;
            float textTargetWidth = textureWidth * multiplier.x;

            return new Vector4(
                   Mathf.Lerp(0, shieldWidth, OffsetUV.x) - Mathf.Lerp(0, textTargetWidth, PivotUV.x),
                   Mathf.Lerp(0, shieldHeight, invertY ? 1 - OffsetUV.y : OffsetUV.y) - Mathf.Lerp(0, textTargetHeight, invertY ? 1 - PivotUV.y : PivotUV.y),
                   textTargetWidth,
                   textTargetHeight);
        }
    }

}

