
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{


    [XmlRoot("onBuildingPropDescriptor")]
    public class WriteOnBuildingPropXml : BaseWriteOnXml, ILibable
    {
        public const int TEXT_PARAMETERS_COUNT = 10;

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

        public override TextRenderingClass RenderingClass => TextRenderingClass.Buildings;

        public override string DescriptorOverrideFont => FontName;

        [XmlArray("platformOrder")]
        [XmlArrayItem("p")]
        public int[] m_platforms = new int[0];

        [XmlAttribute("showIfNoLine")]
        public bool m_showIfNoLine = true;

        [XmlElement("arrayRepeatOffset")]
        public Vector3Xml ArrayRepeat
        {
            get => m_arrayRepeat; set
            {
                if (value != m_arrayRepeat)
                {
                    BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
                }
                m_arrayRepeat = value;
            }
        }
        [XmlIgnore]
        private int m_arrayRepeatTimes = 1;
        [XmlAttribute("arrayRepeatTimes")]
        public int ArrayRepeatTimes
        {
            get => m_arrayRepeatTimes;
            set
            {
                if (value != m_arrayRepeatTimes)
                {
                    BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData());
                }
                m_arrayRepeatTimes = Mathf.Clamp(value, 1, 9999);
            }
        }

        [XmlAttribute("coloringMode")]
        public ColoringMode ColorModeProp { get; set; } = ColoringMode.Fixed;

        [XmlAttribute("useFixedIfMultiline")]
        public bool UseFixedIfMultiline { get; set; } = true;

        [XmlAttribute("subBuildingIdxPivotReference")]
        public int SubBuildingPivotReference { get; set; } = 0;

        private Vector3Xml m_arrayRepeat = new Vector3Xml();

    }
}
