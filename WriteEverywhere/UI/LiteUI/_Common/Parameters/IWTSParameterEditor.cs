using UnityEngine;

namespace WriteEverywhere.UI
{
    internal interface IWTSParameterEditor<T>
    {
        void DrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        void DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        bool IsText { get; }
        string[] OnFilterParam(WTSBaseParamsTab<T> tab);
        void OnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout);
    }
}
