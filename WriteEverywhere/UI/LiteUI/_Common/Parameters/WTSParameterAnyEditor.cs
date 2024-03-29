﻿using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSParameterAnyEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsImg = new[] { Str.WTS_IMAGESRC_ASSET, Str.WTS_IMAGESRC_LOCAL, Str.WTS_PARAMTYPE_PLAINTEXT, Str.WTS_PARAMTYPE_VARIABLE };
        public bool IsText { get; } = true;
        public int HoverIdx => m_hoverIdx;
        public Func<IIndexedPrefabData> m_prefabIndexGetter;
        public WTSParameterAnyEditor(Func<IIndexedPrefabData> prefabIndexGetter)
        {
            m_prefabIndexGetter = prefabIndexGetter;
        }
        public float DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            bool dirtyInput;
            bool dirtyType;
            using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
            {
                var currIdx = (tab.IsTextVariable ? 2 : 0) + (tab.IsLocal ? 1 : 0);
                var modelType = GUILayout.SelectionGrid(currIdx, v_protocolsImg, 2);
                dirtyType = modelType != currIdx;
                if (dirtyType)
                {
                    tab.IsTextVariable = ((modelType & 2) == 2);
                    tab.SetLocal((modelType & 1) == 1);
                    tab.SetIsVariable((modelType & 1) == 1);
                    tab.ClearSelectedFolder();
                    tab.ClearSelectedValue();
                }
            };
            using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
            {
                var newInput = GUILayout.TextField(tab.SearchText);
                dirtyInput = newInput != tab.SearchText;
                if (dirtyInput)
                {
                    tab.SearchText = newInput;
                }
            };

            if (dirtyInput || dirtyType)
            {
                tab.RestartFilterCoroutine(this);
            }
            return 80;
        }
        private int m_hoverIdx;
        public void DrawLeftPanel(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            if (tab.IsTextVariable) WTSParameterVariableEditor<T>.VariableDrawLeftPanel(renderingClass, tab, areaRect, ref m_hoverIdx, this);
            else WTSParameterImageEditor<T>.ImageDrawLeftPanel(tab, areaRect, this);
        }

        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            if (tab.IsTextVariable) WTSParameterVariableEditor<T>.VariableDrawRightPanel(tab, areaRect);
            else WTSParameterImageEditor<T>.ImageDrawRightPanel(tab, areaRect);
        }

        public string[] OnFilterParam(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab) => (tab.IsTextVariable) ? WTSParameterVariableEditor<T>.VariableOnFilterParam(renderingClass, tab) : WTSParameterImageEditor<T>.ImageOnFilterParam(tab, m_prefabIndexGetter);
        public void OnSelectItem(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, int selectLayout)
        {
            if (tab.IsTextVariable) WTSParameterVariableEditor<T>.VariableOnSelectItem(renderingClass, tab, selectLayout, ref m_hoverIdx, this);
            else WTSParameterImageEditor<T>.ImageOnSelectItem(tab, selectLayout, this);
        }

        public void OnHoverVar(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, int autoSelectVal, BaseCommandLevel commandLevel)
        {
            if (tab.IsTextVariable) WTSParameterVariableEditor<T>.VariableOnHoverVar(renderingClass, tab, autoSelectVal, commandLevel, ref m_hoverIdx);
        }
    }
}
