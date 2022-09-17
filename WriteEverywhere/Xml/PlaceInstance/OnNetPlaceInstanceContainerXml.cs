using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    [XmlRoot("onNetDescriptor")]
    public class OnNetInstanceCacheContainerXml : BoardInstanceOnNetXml
    {
        [XmlElement("targetSegments")]
        public SimpleNonSequentialList<ushort> m_targets = new SimpleNonSequentialList<ushort>();
        [XmlIgnore]
        public List<Vector3Xml> m_cachedPositions;
        [XmlIgnore]
        public List<Vector3Xml> m_cachedRotations;
        [XmlIgnore]
        public PropInfo m_simpleCachedProp => m_simpleProp;

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
                for (int i = 0; i < m_textParameters.Length; i++)
                {
                    if (m_textParameters[i] != null)
                    {
                        res[i] = TextParameterXmlContainer.FromWrapper(m_textParameters[i]);
                    }
                }
                return res;
            }
            set
            {
                foreach (var k in value?.Keys)
                {
                    if (k < m_textParameters.Length)
                    {
                        m_textParameters[k] = value[k]?.ToWrapper(RenderingClass);
                    }
                }
            }
        }

        public TextParameterWrapper SetTextParameter(int idx, string val)
        {
            if (m_textParameters == null)
            {
                m_textParameters = new TextParameterWrapper[TEXT_PARAMETERS_COUNT];
            }
            return m_textParameters[idx] = new TextParameterWrapper(val, RenderingClass);
        }
        public void DeleteTextParameter(int idx)
        {
            if (m_textParameters == null)
            {
                m_textParameters = new TextParameterWrapper[TEXT_PARAMETERS_COUNT];
            }
            m_textParameters[idx] = null;
        }

        public Dictionary<int, List<BoardTextDescriptorGeneralXml>> GetAllParametersUsedWithData() => Descriptor?.TextDescriptors.Where(x => x.IsParameter()).GroupBy(x => x.m_parameterIdx).ToDictionary(x => x.Key, x => x.ToList());

        [XmlIgnore]
        public TextParameterWrapper[] m_textParameters = new TextParameterWrapper[TEXT_PARAMETERS_COUNT];

        public override TextParameterWrapper GetParameter(int idx) => m_textParameters?[idx];
    }
}
