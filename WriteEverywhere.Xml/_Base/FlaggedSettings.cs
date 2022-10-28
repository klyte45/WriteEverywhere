using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Xml
{
    public class FlaggedSettings
    {
        [XmlAttribute("requiredFlags")]
        public int m_requiredFlags;
        [XmlAttribute("forbiddenFlags")]
        public int m_forbiddenFlags;
        [XmlAttribute("requiredFlags2")]
        public int m_requiredFlags2;
        [XmlAttribute("forbiddenFlags2")]
        public int m_forbiddenFlags2;

        public bool TestFlags(int instanceFlags, int instanceFlags2)
            => ((instanceFlags & m_requiredFlags) == m_requiredFlags)
                        && ((instanceFlags & m_forbiddenFlags) == 0)
                        && ((instanceFlags2 & m_requiredFlags2) == m_requiredFlags2)
                        && ((instanceFlags2 & m_forbiddenFlags2) == 0);
    }

}

