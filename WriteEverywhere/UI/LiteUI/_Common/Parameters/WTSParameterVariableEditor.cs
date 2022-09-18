using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using WriteEverywhere.Xml;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    internal class WTSParameterVariableEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsTxt = new[] { Str.WTS_PARAMTYPE_PLAINTEXT, Str.WTS_PARAMTYPE_VARIABLE };

        public bool IsText { get; } = true;
        public int HoverIdx { get; private set; }

        public void DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
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
                        tab.RestartFilterCoroutine();
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
                tab.RestartFilterCoroutine();
            }
        }

        public void DrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            if (tab.IsVariable)
            {
                var selectOpt = GUILayout.SelectionGrid(-1, tab.m_searchResult.Value, 1, new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft
                }, GUILayout.Width((areaRect.x / 2) - 25));
                if (selectOpt >= 0)
                {
                    OnSelectItem(tab, selectOpt);
                }
            }

        }

        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 _) => GUILayout.Label(tab.VariableDescription, new GUIStyle(GUI.skin.label) { richText = true });


        public void OnSelectItem(WTSBaseParamsTab<T> tab, int selectOpt)
        {
            var cl = CommandLevel.OnFilterParamByText(tab.GetCurrentParamString(), out _);
            if (selectOpt > 0)
            {
                if (cl.defaultValue is null || HoverIdx == selectOpt)
                {
                    var value = tab.m_searchResult.Value[selectOpt];
                    var paramPath = CommandLevel.GetParameterPath(tab.SelectedValue ?? "");
                    tab.SetSelectedValue(CommandLevel.FromParameterPath(paramPath.Take(cl.level).Concat(new[] { value == GUIKwyttoCommons.v_empty ? "" : value })));
                    tab.m_searchResult.Value = new string[0];
                    tab.SearchText = "";
                    HoverIdx = -1;
                    tab.RestartFilterCoroutine();
                }
                else
                {
                    OnHoverVar(tab, selectOpt, cl);
                }
            }
            else if (selectOpt == 0 && cl.level > 0)
            {
                var paramPath = CommandLevel.GetParameterPath(tab.SelectedValue ?? "");
                tab.SetSelectedValue(CommandLevel.FromParameterPath(paramPath.Take(cl.level - 1)));
                tab.m_searchResult.Value = new string[0];
                HoverIdx = -1;
                if (CommandLevel.OnFilterParamByText(tab.GetCurrentParamString(), out _).defaultValue is null)
                {
                    tab.SearchText = paramPath[cl.level - 1];
                    tab.RestartFilterCoroutine();
                }
                else
                {
                    tab.SearchText = "";
                    tab.RestartFilterCoroutine(paramPath[cl.level - 1]);
                }
            }
        }

        public void OnHoverVar(WTSBaseParamsTab<T> tab, int selectOpt, CommandLevel cl)
        {
            HoverIdx = selectOpt;
            var str = tab.m_searchResult.Value[HoverIdx];
            var key = cl.nextLevelOptions.Where(z => z.Key.ToString() == str).FirstOrDefault().Key;
            tab.SetVariableDescription(key is null ? "" : $"<color=#00FF00>{key}</color>\n\n" + key.ValueToI18n());
        }

        public string[] OnFilterParam(WTSBaseParamsTab<T> tab)
        {
            var cmdResult = CommandLevel.OnFilterParamByText(tab.GetCurrentParamString(), out string currentDescription);
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
                    : cmdResult.nextLevelOptions?.Select(x => x.Key.ToString()).Where(x => x.ToLower().Contains(tab.SearchText)).OrderBy(x => x).ToArray();
            }
        }
    }
}
