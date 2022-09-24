extern alias VS;

using ColossalFramework;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;

namespace WriteEverywhere.Xml
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
        public BoardTextDescriptorGeneralXml[] TextDescriptors { get => textDescriptors; set => textDescriptors = value ?? new BoardTextDescriptorGeneralXml[0]; }
        private BoardTextDescriptorGeneralXml[] textDescriptors = new BoardTextDescriptorGeneralXml[0];
        [XmlIgnore]
        public ref BoardTextDescriptorGeneralXml[] RefTextDescriptors => ref textDescriptors;

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
                    OnChangeMatrixData();
                }
                m_arrayRepeat = value;
            }
        }
        [XmlIgnore]
        private int m_arrayRepeatTimes = 0;
        [XmlAttribute("arrayRepeatTimes")]
        public int ArrayRepeatTimes
        {
            get => m_arrayRepeatTimes;
            set
            {
                if (value != m_arrayRepeatTimes)
                {
                    OnChangeMatrixData();
                }
                m_arrayRepeatTimes = value;
            }
        }

        [XmlAttribute("coloringMode")]
        public ColoringMode ColorModeProp { get; set; } = ColoringMode.Fixed;

        [XmlAttribute("useFixedIfMultiline")]
        public bool UseFixedIfMultiline { get; set; } = true;

        [XmlAttribute("subBuildingIdxPivotReference")]
        public int SubBuildingPivotReference { get; set; } = -1;
       
        private Vector3Xml m_arrayRepeat = new Vector3Xml();

     

        [XmlElement("TextParametersV4")]
        public SimpleNonSequentialList<TextParameterXmlContainer> TextParameters
        {
            get
            {
                var res = new SimpleNonSequentialList<TextParameterXmlContainer>();
                foreach (var k in m_textParameters?.Keys)
                {
                    if (m_textParameters[k] != null)
                    {
                        res[k] = TextParameterXmlContainer.FromWrapper(m_textParameters[k]);
                    }
                }
                return res;
            }
            set
            {
                m_textParameters?.Clear();
                foreach (var k in value?.Keys)
                {
                    SetTextParameter((int)k, value[k]?.Value);
                }
            }
        }

        public TextParameterWrapper SetTextParameter(int idx, string val)
        {
            if (m_textParameters == null)
            {
                m_textParameters = new SimpleNonSequentialList<TextParameterWrapper>();
            }
            var result = new TextParameterWrapper(val, RenderingClass);
            if (result.IsParameter)
            {
                return m_textParameters[idx] = new TextParameterWrapper("<CANNOT SET PARAMETER AS PARAMETER!>", RenderingClass);
            }
            else
            {
                return m_textParameters[idx] = result;
            }
        }
        public void DeleteTextParameter(int idx)
        {
            if (m_textParameters == null)
            {
                m_textParameters = new SimpleNonSequentialList<TextParameterWrapper>();
            }
            m_textParameters[idx] = null;
        }

        public Dictionary<int, List<Tuple<IParameterizableVariable, string>>> GetAllParametersUsedWithData() =>
            TextDescriptors
            .Where(x =>
                x.Value.IsParameter
                || (x.ParameterSequence?.Any(y => y.Value?.IsParameter ?? false) ?? false)
            )
            .SelectMany(x =>
            x.textContent != TextContent.TextParameterSequence
                    ? new[] { Tuple.New(x as IParameterizableVariable, x.SaveName, x.Value) }
                    : x.ParameterSequence.Where(y => y.Value?.IsParameter ?? false).Select(y => Tuple.New(y as IParameterizableVariable, x.SaveName, y.Value))
            )
            .GroupBy(x => x.First.GetParamIdx()).ToDictionary(x => x.Key, x => x.Select(y => Tuple.New(y.First, y.Second)).ToList());

        [XmlIgnore]
        public SimpleNonSequentialList<TextParameterWrapper> m_textParameters = new SimpleNonSequentialList<TextParameterWrapper>();

        public TextParameterWrapper GetParameter(int idx) => m_textParameters.TryGetValue(idx, out var val) ? val : null;
    }
}
