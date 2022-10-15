using ColossalFramework;
using Kwytto.LiteUI;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.UI
{
    internal class WTSParameterVariableEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsTxt = new[] { Str.WTS_PARAMTYPE_PLAINTEXT, Str.WTS_PARAMTYPE_VARIABLE };
        private int m_hoverIdx;

        public bool IsText { get; } = true;
        public int HoverIdx => m_hoverIdx;

        public float DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {

            using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
            {
                var varType = GUILayout.SelectionGrid(tab.IsVariable ? 1 : 0, v_protocolsTxt, v_protocolsTxt.Length);
                bool dirtyType = tab.IsVariable != (varType == 1);
                if (dirtyType)
                {
                    tab.SetIsVariable(varType == 1);
                    tab.ClearSelectedValue();
                    tab.SearchText = "";
                    tab.SetVariableDescription("");
                    tab.m_searchResult.Value = new string[0];
                    if (tab.IsVariable)
                    {
                        tab.RestartFilterCoroutine(this);
                    }
                }
            };


            bool dirtyInput = false;
            using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
            {
                var newInput = GUILayout.TextField(tab.SearchText);
                dirtyInput = newInput != tab.SearchText;
                if (dirtyInput)
                {
                    tab.SearchText = newInput;
                }
            }

            if (tab.IsVariable && dirtyInput)
            {
                tab.RestartFilterCoroutine(this);
            }
            return 50;
        }
        public static void VariableDrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect, ref int hoverIdx, IWTSParameterEditor<T> paramEditor)
        {
            if (tab.IsVariable)
            {
                var selectOpt = GUILayout.SelectionGrid(-1, tab.m_searchResult.Value, 1, new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft
                }, GUILayout.Width((areaRect.x / 2) - 25 * GUIWindow.ResolutionMultiplier));
                if (selectOpt >= 0)
                {
                    VariableOnSelectItem(tab, selectOpt, ref hoverIdx, paramEditor);
                }
            }

        }

        public static void VariableDrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 _) => GUILayout.Label(tab.VariableDescription, new GUIStyle(GUI.skin.label) { richText = true });


        public static void VariableOnSelectItem(WTSBaseParamsTab<T> tab, int selectOpt, ref int hoverIdx, IWTSParameterEditor<T> paramEditor)
        {
            var cl = CommandLevelSingleton.Instance.OnFilterParamByText(tab.GetCurrentParamString(), out _);
            if (selectOpt > 0 || (cl.level == 0 && selectOpt == 0))
            {
                if (cl.defaultValue is null || hoverIdx == selectOpt)
                {
                    var value = tab.m_searchResult.Value[selectOpt];
                    var paramPath = CommandLevelSingleton.GetParameterPath(tab.SelectedValue ?? "");
                    tab.SetSelectedValue(CommandLevel.FromParameterPath(paramPath.Take(cl.level).Concat(new[] { value == GUIKwyttoCommons.v_empty ? "" : value })));
                    tab.m_searchResult.Value = new string[0];
                    tab.SearchText = "";
                    hoverIdx = -1;
                    tab.RestartFilterCoroutine(paramEditor);
                }
                else
                {
                    VariableOnHoverVar(tab, selectOpt, cl, ref hoverIdx);
                }
            }
            else if (selectOpt == 0 && cl.level > 0)
            {
                var paramPath = CommandLevelSingleton.GetParameterPath(tab.SelectedValue ?? "").ToArray();
                tab.SetSelectedValue(CommandLevel.FromParameterPath(paramPath.Take(cl.level - 1)));
                tab.m_searchResult.Value = new string[0];
                hoverIdx = -1;
                if (CommandLevelSingleton.Instance.OnFilterParamByText(tab.GetCurrentParamString(), out _).defaultValue is null)
                {
                    tab.SearchText = paramPath[cl.level];
                    tab.RestartFilterCoroutine(paramEditor);
                }
                else
                {
                    tab.SearchText = "";
                    tab.RestartFilterCoroutine(paramEditor, paramPath[cl.level]);
                }
            }
        }

        public static void VariableOnHoverVar(WTSBaseParamsTab<T> tab, int selectOpt, BaseCommandLevel cl, ref int hoverIdx)
        {
            hoverIdx = selectOpt;
            var str = tab.m_searchResult.Value[selectOpt];
            var key = cl.NextLevelsKeys.Where(z => CommandLevelSingleton.GetEnumKeyValue(z, cl.level) == str).FirstOrDefault();
            tab.SetVariableDescription(key is null ? "" : $"<color=#00FF00>{key}</color>\n\n" + key.ValueToI18n());
        }

        public static string[] VariableOnFilterParam(WTSBaseParamsTab<T> tab)
        {
            var cmdResult = CommandLevelSingleton.Instance.OnFilterParamByText(tab.GetCurrentParamString(), out string currentDescription);
            if (cmdResult is null)
            {
                return null;
            }
            else
            {
                tab.SetVariableDescription(
                    (cmdResult.regexValidValues.IsNullOrWhiteSpace() ? "" : $"Regex: <color=#FFFF00>{cmdResult.regexValidValues}</color>\n")
                    + currentDescription);
                return cmdResult.regexValidValues != null
                    ? Regex.IsMatch(tab.SearchText, cmdResult.regexValidValues) ? (new[] { Regex.Replace(tab.SearchText, @"([^\\])/|^/", "$1\\/") }) : (new string[0])
                    : cmdResult.NextLevelsKeys?.Select(x => CommandLevelSingleton.GetEnumKeyValue(x, cmdResult.level)).Where(x => x.ToLower().Contains(tab.SearchText)).OrderBy(x => x).ToArray();
            }
        }

        public void DrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect) => VariableDrawLeftPanel(tab, areaRect, ref m_hoverIdx, this);
        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect) => VariableDrawRightPanel(tab, areaRect);
        public string[] OnFilterParam(WTSBaseParamsTab<T> tab) => VariableOnFilterParam(tab);
        public void OnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout) => VariableOnSelectItem(tab, selectLayout, ref m_hoverIdx, this);
        public void OnHoverVar(WTSBaseParamsTab<T> tab, int autoSelectVal, BaseCommandLevel commandLevel) => VariableOnHoverVar(tab, autoSelectVal, commandLevel, ref m_hoverIdx);
    }
}
