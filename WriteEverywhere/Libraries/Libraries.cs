using Kwytto.Interfaces;
using Kwytto.LiteUI;
using System.IO;
using WriteEverywhere.Layout;

namespace WriteEverywhere.Libraries
{
    public class GUITextEntryLib : GUIXmlFolderLib<TextToWriteOnXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "TextEntry"); }
    public class GUITextEntryListLib : GUIXmlFolderLib<ILibableAsContainer<TextToWriteOnXml>> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "TextEntryList"); }

    public class GUIBuildingPropListLib : GUIXmlFolderLib<ListWriteOnBuildingPropXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "BuildingPropList"); }
    public class GUIBuildingPropLib : GUIXmlFolderLib<WriteOnBuildingPropXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "BuildingProp"); }

    public class GUIOnNetPropLib : GUIXmlFolderLib<WriteOnNetXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "OnNetProp"); }
    public class GUIOnNetPropListLib : GUIXmlFolderLib<ExportableBoardInstanceOnNetListXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "ListOnNetProp"); }

    public class GUIVehicleLayoutLib : GUIXmlFolderLib<LayoutDescriptorVehicleXml> { public override string LibFolderPath { get; } = Path.Combine(WEMainController.LIBRARYFOLDERS_ROOT, "Vehicles"); }
}
