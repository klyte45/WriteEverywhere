using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace WriteEverywhere.Layout
{

    public class WriteOnNetGroupXml : IWriteGroup
    {
        [XmlIgnore]
        public OnNetInstanceCacheContainerXml[] BoardsData { get; set; } = new OnNetInstanceCacheContainerXml[0];
        [XmlIgnore]
        public bool cached = false;
        [XmlElement("BoardsData")]
        public SimpleXmlList<OnNetInstanceCacheContainerXml> BoardsDataExportable
        {
            get => new SimpleXmlList<OnNetInstanceCacheContainerXml>(BoardsData);
            set => BoardsData = value.ToArray();
        }
        public bool HasAnyBoard() => (BoardsData?.Where(y => y != null)?.Count() ?? 0) > 0;

    }
    public class ExportableBoardInstanceOnNetListXml : ILibable
    {
        public WriteOnNetXml[] Instances { get; set; }
        [XmlAttribute("saveName")]
        public string SaveName { get; set; }
    }
}

