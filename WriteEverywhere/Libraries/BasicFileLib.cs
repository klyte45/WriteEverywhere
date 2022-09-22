extern alias VS;

using Kwytto.Interfaces;
using Kwytto.Libraries;
using Kwytto.Utils;
using WriteEverywhere.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using VS::Bridge_WE2VS;

namespace WriteEverywhere.Libraries
{


    //[XmlRoot("LibPropSettings")] public class WTSLibPropSettings : LibBaseFile<WTSLibPropSettings, BoardDescriptorGeneralXml> { protected override string XmlName => "LibPropSettings"; }
    //[XmlRoot("LibPropTextItem")] public class WTSLibPropTextItem : LibBaseFile<WTSLibPropTextItem, BoardTextDescriptorGeneralXml> { protected override string XmlName => "LibPropTextItem"; }
    //[XmlRoot("LibRoadCornerRule")] public class WTSLibRoadCornerRule : LibBaseFile<WTSLibRoadCornerRule, BoardInstanceRoadNodeXml> { protected override string XmlName => "LibRoadCornerRule"; }
    //[XmlRoot("LibRoadCornerRuleList")] public class WTSLibRoadCornerRuleList : LibBaseFile<WTSLibRoadCornerRuleList, ILibableAsContainer<BoardInstanceRoadNodeXml>> { protected override string XmlName => "LibRoadCornerRuleList"; }
    //[XmlRoot("LibBuildingPropLayoutList")] public class WTSLibBuildingPropLayoutList : LibBaseFile<WTSLibBuildingPropLayoutList, ExportableBoardInstanceBuildingListXml> { protected override string XmlName => "LibBuildingPropLayoutList"; }
    //[XmlRoot("LibBuildingPropLayout")] public class WTSLibBuildingPropLayout : LibBaseFile<WTSLibBuildingPropLayout, BoardInstanceBuildingXml> { protected override string XmlName => "LibBuildingPropLayout"; }
    [XmlRoot("LibVehicleLayout")] public class WTSLibVehicleLayout : LibBaseFile<WTSLibVehicleLayout, LayoutDescriptorVehicleXml> { protected override string XmlName => "LibVehicleLayout"; }
    [XmlRoot("LibTextList")] internal class WTSLibTextList : LibBaseFile<WTSLibTextList, ILibableAsContainer<BoardTextDescriptorGeneralXml>> { protected override string XmlName => "WTSLibTextList"; }
    [XmlRoot("LibTextItem")] internal class WTSLibTextItem : LibBaseFile<WTSLibTextItem, BoardTextDescriptorGeneralXml> { protected override string XmlName => "WTSLibTextItem"; }
    [XmlRoot("LibOnNetPropLayout")] public class WTSLibOnNetPropLayout : LibBaseFile<WTSLibOnNetPropLayout, WriteOnNetXml> { protected override string XmlName => "LibOnNetPropLayout"; }
    [XmlRoot("LibOnNetPropLayoutList")] public class WTSLibOnNetPropLayoutList : LibBaseFile<WTSLibOnNetPropLayoutList, ExportableBoardInstanceOnNetListXml> { protected override string XmlName => "LibOnNetPropLayoutList"; }
    //[XmlRoot("LibHighwayShieldLayout")] public class WTSLibHighwayShieldLayout : LibBaseFile<WTSLibHighwayShieldLayout, HighwayShieldDescriptor> { protected override string XmlName => "LibHighwayShieldLayout"; }
    //[XmlRoot("LibHighwayShieldTextLayer")] public class WTSLibHighwayShieldTextLayer : LibBaseFile<WTSLibHighwayShieldTextLayer, ImageLayerTextDescriptorXml> { protected override string XmlName => "LibHighwayShieldTextLayer"; }

    //#region Mileage Marker
    //[XmlRoot("LibMileageMarkerProp")] public class WTSLibMileageMarkerGroup : BasicLib<WTSLibMileageMarkerGroup, BoardDescriptorMileageMarkerXml> { protected override string XmlName => "LibMileageMarkerProp"; }
    //[XmlRoot("LibMileageMarkerText")] public class WTSLibTextMeshMileageMarker : BasicLib<WTSLibTextMeshMileageMarker, BoardTextDescriptorMileageMarkerXml> { protected override string XmlName => "LibMileageMarkerText"; }
    //#endregion

    //#region In Segment props
    //[XmlRoot("LibSegmentPropGroup")] public class WTSLibPropGroupHigwaySigns : BasicLib<WTSLibPropGroupHigwaySigns, BoardBunchContainerHighwaySignXml> { protected override string XmlName => "LibSegmentPropGroup"; }
    //[XmlRoot("LibSegmentProp")] public class WTSLibPropSingleHighwaySigns : BasicLib<WTSLibPropSingleHighwaySigns, BoardDescriptorHigwaySignXml> { protected override string XmlName => "LibSegmentProp"; }
    //[XmlRoot("LibSegmentText")] public class WTSLibTextMeshHighwaySigns : BasicLib<WTSLibTextMeshHighwaySigns, BoardTextDescriptorHighwaySignsXml> { protected override string XmlName => "LibSegmentText"; }
    //#endregion

}