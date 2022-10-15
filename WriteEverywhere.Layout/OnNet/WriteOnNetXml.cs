
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
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
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
            }
        }
        [XmlAttribute("segmentPosition")]
        public float SegmentPosition
        {
            get => m_segmentPosition; set
            {
                m_segmentPosition = value;
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
            }
        }
        [XmlAttribute("segmentPositionStart")]
        public float SegmentPositionStart
        {
            get => m_segmentPositionStart; set
            {
                m_segmentPositionStart = value;
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
            }
        }
        [XmlAttribute("segmentPositionEnd")]
        public float SegmentPositionEnd
        {
            get => m_segmentPositionEnd; set
            {
                m_segmentPositionEnd = value;
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
            }
        }
        [XmlAttribute("segmentPositionsRepeatCount")]
        public ushort SegmentPositionRepeatCount
        {
            get => m_segmentPositionRepeat; set
            {
                m_segmentPositionRepeat = value;
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
            }
        }
        [XmlAttribute("segmentPositionsRepeating")]
        public bool SegmentPositionRepeating
        {
            get => m_segmentRepeatItem; set
            {
                m_segmentRepeatItem = value;
                BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
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
        public TextToWriteOnXml[] TextDescriptors { get => textDescriptors; set => textDescriptors = value ?? new TextToWriteOnXml[0]; }
        private TextToWriteOnXml[] textDescriptors = new TextToWriteOnXml[0];
        [XmlIgnore]
        public ref TextToWriteOnXml[] RefTextDescriptors => ref textDescriptors;

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
            set
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
    }
}
