﻿using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    [XmlRoot("onNetDescriptor")]
    public class OnNetInstanceCacheContainerXml : WriteOnNetXml
    {
        [XmlElement("targetSegments")]
        public SimpleNonSequentialList<ushort> m_targets = new SimpleNonSequentialList<ushort>();
        [XmlIgnore]
        public List<Vector3Xml> m_cachedPositions;
        [XmlIgnore]
        public List<Vector3Xml> m_cachedRotations;
        [XmlIgnore]
        public PropInfo SimpleCachedProp => m_simpleProp;

        public override void OnChangeMatrixData()
        {
            base.OnChangeMatrixData();
            m_cachedPositions = null;
            m_cachedRotations = null;
        }

        public ushort GetTargetSegment(int id) => m_targets.TryGetValue(id, out ushort value) ? value : (ushort)0;
        public void SetTargetSegment(int id, ushort value) => m_targets[id] = value;


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

        public Dictionary<int, List<BoardTextDescriptorGeneralXml>> GetAllParametersUsedWithData() => TextDescriptors.Where(x => x.Value.IsParameter).GroupBy(x => x.Value.GetParamIdx).ToDictionary(x => x.Key, x => x.ToList());

        [XmlIgnore]
        public SimpleNonSequentialList<TextParameterWrapper> m_textParameters = new SimpleNonSequentialList<TextParameterWrapper>();

        public override TextParameterWrapper GetParameter(int idx) => m_textParameters.TryGetValue(idx, out var val) ? val : null;
    }
}
