using UnityEngine;
using WriteEverywhere.Plugins;

namespace WriteEverywhere.UI
{
    public interface IWTSParameterEditor<T>
    {
        void DrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        float DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        bool IsText { get; }
        int HoverIdx { get; }

        string[] OnFilterParam(WTSBaseParamsTab<T> tab);
        void OnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout);
        void OnHoverVar(WTSBaseParamsTab<T> wTSBaseParamsTab, int autoSelectVal, BaseCommandLevel commandLevel);
    }
}
