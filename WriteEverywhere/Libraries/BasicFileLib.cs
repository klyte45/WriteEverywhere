extern alias VS;
using Kwytto.Interfaces;
using Kwytto.Libraries;
using System.Xml.Serialization;
using WriteEverywhere.Layout;

namespace WriteEverywhere.Libraries
{
    [XmlRoot("LibTextList")] public class WTSLibTextList : LibBaseFile<WTSLibTextList, ILibableAsContainer<TextToWriteOnXml>> { protected override string XmlName => "WTSLibTextList"; }
    [XmlRoot("LibTextItem")] public class WTSLibTextItem : LibBaseFile<WTSLibTextItem, TextToWriteOnXml> { protected override string XmlName => "WTSLibTextItem"; }

    [XmlRoot("LibOnBuildingPropLayoutList")] public class WTSLibOnBuildingPropLayoutList : LibBaseFile<WTSLibOnBuildingPropLayoutList, ExportableBoardInstanceOnBuildingListXml> { protected override string XmlName => "LibOnBuildingPropLayoutList"; }
    [XmlRoot("LibOnBuildingPropLayout")] public class WTSLibOnBuildingPropLayout : LibBaseFile<WTSLibOnBuildingPropLayout, WriteOnBuildingPropXml> { protected override string XmlName => "LibOnBuildingPropLayout"; }

    [XmlRoot("LibVehicleLayout")] public class WTSLibVehicleLayout : LibBaseFile<WTSLibVehicleLayout, LayoutDescriptorVehicleXml> { protected override string XmlName => "LibVehicleLayout"; }

    [XmlRoot("LibOnNetPropLayout")] public class WTSLibOnNetPropLayout : LibBaseFile<WTSLibOnNetPropLayout, WriteOnNetXml> { protected override string XmlName => "LibOnNetPropLayout"; }
    [XmlRoot("LibOnNetPropLayoutList")] public class WTSLibOnNetPropLayoutList : LibBaseFile<WTSLibOnNetPropLayoutList, ExportableBoardInstanceOnNetListXml> { protected override string XmlName => "LibOnNetPropLayoutList"; }

}