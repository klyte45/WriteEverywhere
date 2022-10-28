using Kwytto.Interfaces;
using WriteEverywhere.UI;

namespace WriteEverywhere.Overrides
{
    public class AssetEditorActions : IAssetEditorActions
    {
        public void AfterLoad()
        {
            BuildingLiteUI.Instance.ReloadAsset();
            WTSVehicleLiteUI.Instance.ReloadAsset();
        }

        public void AfterSave() { }
    }
}
