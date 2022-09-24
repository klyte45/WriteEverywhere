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

    }
}
