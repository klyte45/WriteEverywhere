extern alias TLM;
using Kwytto.Utils;
using System.Xml.Serialization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Data
{
    public class BuildingParametersData
    {
        [XmlAttribute("assetName")]
        public string assetName;

        [XmlElement("TextParameters")]
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
            var result = new TextParameterWrapper(val, TextRenderingClass.Buildings);
            if (result.IsParameter)
            {
                return m_textParameters[idx] = new TextParameterWrapper("<CANNOT SET PARAMETER AS PARAMETER!>", TextRenderingClass.Buildings);
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


        [XmlIgnore]
        public SimpleNonSequentialList<TextParameterWrapper> m_textParameters = new SimpleNonSequentialList<TextParameterWrapper>();

        public TextParameterWrapper GetParameter(int idx) => m_textParameters.TryGetValue(idx, out var val) ? val : null;

    }

}
