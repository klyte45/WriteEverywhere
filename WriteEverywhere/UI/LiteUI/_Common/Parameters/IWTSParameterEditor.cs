using UnityEngine;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public interface IWTSParameterEditor<T>
    {
        void DrawLeftPanel(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, Vector2 areaRect);
        void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect);
        float DrawTop( WTSBaseParamsTab<T> tab, Vector2 areaRect);
        bool IsText { get; }
        int HoverIdx { get; }

        string[] OnFilterParam(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab);
        void OnSelectItem(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, int selectLayout);
        void OnHoverVar(TextRenderingClass renderingClass, WTSBaseParamsTab<T> wTSBaseParamsTab, int autoSelectVal, BaseCommandLevel commandLevel);
    }
}
