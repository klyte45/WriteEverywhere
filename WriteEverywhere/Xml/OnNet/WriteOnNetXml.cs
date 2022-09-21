using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{


    [XmlRoot("onNetDescriptor")]
    public class WriteOnNetXml : BaseWriteOnXml, ILibable
    {
        public const int TEXT_PARAMETERS_COUNT = 10;

        [XmlAttribute("pivot")]
        public PivotPosition PivotPosition
        {
            get => m_pivotPosition; set
            {
                m_pivotPosition = value;
                OnChangeMatrixData();
            }
        }
        [XmlAttribute("segmentPosition")]
        public float SegmentPosition
        {
            get => m_segmentPosition; set
            {
                m_segmentPosition = value;
                OnChangeMatrixData();
            }
        }
        [XmlAttribute("segmentPositionStart")]
        public float SegmentPositionStart
        {
            get => m_segmentPositionStart; set
            {
                m_segmentPositionStart = value;
                OnChangeMatrixData();
            }
        }
        [XmlAttribute("segmentPositionEnd")]
        public float SegmentPositionEnd
        {
            get => m_segmentPositionEnd; set
            {
                m_segmentPositionEnd = value;
                OnChangeMatrixData();
            }
        }
        [XmlAttribute("segmentPositionsRepeatCount")]
        public ushort SegmentPositionRepeatCount
        {
            get => m_segmentPositionRepeat; set
            {
                m_segmentPositionRepeat = value;
                OnChangeMatrixData();
            }
        }
        [XmlAttribute("segmentPositionsRepeating")]
        public bool SegmentPositionRepeating
        {
            get => m_segmentRepeatItem; set
            {
                m_segmentRepeatItem = value;
                OnChangeMatrixData();
            }
        }

        private float m_segmentPosition = 0.5f;
        private float m_segmentPositionStart = 0f;
        private float m_segmentPositionEnd = 1f;
        private ushort m_segmentPositionRepeat = 1;
        private bool m_segmentRepeatItem = false;
        private PivotPosition m_pivotPosition = PivotPosition.Left;

        [XmlIgnore]
        public Color? FixedColor { get => m_cachedColor; set => m_cachedColor = value; }
        [XmlIgnore]
        private Color? m_cachedColor;
        [XmlAttribute("fixedColor")]
        public string FixedColorStr { get => m_cachedColor == null ? null : ColorExtensions.ToRGB(FixedColor ?? Color.clear); set => FixedColor = ColorExtensions.FromRGBSafe(value); }

        [XmlAttribute("fontName")]
        public string FontName { get => fontName; set => fontName = value; }
        [XmlIgnore]
        public ref string RefFontName => ref fontName;

        [XmlElement("textDescriptor")]
        public BoardTextDescriptorGeneralXml[] TextDescriptors { get => textDescriptors; set => textDescriptors = value ?? new BoardTextDescriptorGeneralXml[0]; }
        private BoardTextDescriptorGeneralXml[] textDescriptors = new BoardTextDescriptorGeneralXml[0];
        [XmlIgnore]
        internal ref BoardTextDescriptorGeneralXml[] RefTextDescriptors => ref textDescriptors;

        [XmlAttribute("simplePropName")]
        public string m_simplePropName;
        [XmlIgnore]
        public PropInfo SimpleProp
        {
            get
            {
                if (m_simplePropName != null && m_simpleProp?.name != m_simplePropName)
                {
                    m_simpleProp = PrefabCollection<PropInfo>.FindLoaded(m_simplePropName);
                    if (m_simpleProp == null)
                    {
                        m_simplePropName = null;
                    }
                }
                return m_simpleProp;
            }
            internal set
            {
                m_simplePropName = value?.name;
                m_simpleProp = value;
            }
        }
        [XmlIgnore]
        protected PropInfo m_simpleProp;
        private string fontName;

        [XmlAttribute("saveName")]
        public string SaveName { get; set; }

        public override PrefabInfo TargetAssetParameter => SimpleProp;

        public override TextRenderingClass RenderingClass => TextRenderingClass.PlaceOnNet;

        public override string DescriptorOverrideFont => FontName;

        public override TextParameterWrapper GetParameter(int idx) => null;
    }
}
