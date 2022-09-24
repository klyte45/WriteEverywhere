using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class BuildingParamsTab : WTSBaseParamsTab<BuildingParametersData>
    {
        private uint lastTickDraw;
        private Dictionary<int, List<Tuple<IParameterizableVariable, string>>> m_cachedParamsUsed;

        private Func<ushort> m_buildingIdxGetter;
        private Action<ushort> m_buildingIdxSetter;
        private Func<WriteOnBuildingXml> m_buildingLayoutGetter;
        private Func<BuildingInfo> m_buildingInfoGetter;
        private GUIFilterItemsScreen<StateInstance> m_instanceFilter;

        private enum StateInstance
        {
            Normal,
            GettingInstance
        }

        public BuildingParamsTab(Func<ushort> buildingIdxGetter, Action<ushort> buildingIdxSetter, Func<WriteOnBuildingXml> buildingLayoutGetter, Func<BuildingInfo> buildingInfoGetter)
        {
            m_buildingIdxGetter = buildingIdxGetter;
            m_buildingLayoutGetter = buildingLayoutGetter;
            m_buildingIdxSetter = buildingIdxSetter;
            m_buildingInfoGetter = buildingInfoGetter;
            m_instanceFilter = new GUIFilterItemsScreen<StateInstance>(Str.we_buildingEditor_selectBuilding, ModInstance.Controller, OnFilterParam, OnBuildingSet, GoTo, StateInstance.Normal, StateInstance.GettingInstance, extraButtonsSearch: ExtraButtonsSearch);

        }
        private float ExtraButtonsSearch(out bool hasChanged)
        {
            hasChanged = false;
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if (GUILayout.Button(Str.we_buildingEditor_pickerBtn, currentTool is BuildingEditorTool currentToolVeh ? WEUIUtils.GreenButton : GUI.skin.button, GUILayout.Width(100)))
            {
                var vehTool = ToolsModifierControl.toolController.GetComponent<BuildingEditorTool>();
                vehTool.GetHoverColor = (x) => BuildingManager.instance.m_buildings.m_buffer[x].Info == m_buildingInfoGetter() ? BuildingEditorTool.m_hoverColor : BuildingEditorTool.m_removeColor;
                vehTool.OnBuildingSelect += (x) =>
                {
                    if (m_buildingInfoGetter() == BuildingManager.instance.m_buildings.m_buffer[x].Info)
                    {
                        m_currentState = StateInstance.Normal;
                        m_buildingIdxSetter(x);
                    }
                };
                ToolsModifierControl.SetTool<BuildingEditorTool>();
            }
            return 0;
        }

        private ushort autoselect;
        private readonly Wrapper<ushort[]> m_searchResultIdxBuilding = new Wrapper<ushort[]>();
        private readonly Wrapper<string[]> m_searchResultIdxBuildingName = new Wrapper<string[]>();

        private IEnumerator OnFilterParam(string input, Action<string[]> _)
        {
            yield return 0;
            var buffer = BuildingManager.instance.m_buildings.m_buffer;
            var results = new List<Tuple<string, ushort>>();
            for (int i = 1; i < buffer.Length; i++)
            {

                if ((buffer[i].m_flags & Building.Flags.Created) != 0 && buffer[i].Info == m_buildingInfoGetter() && BuildingManager.instance.GetBuildingName((ushort)i, default) is string name && name.Contains(input))
                {
                    results.Add(Tuple.New(name, (ushort)i));
                }


                if (i % 750 == 0)
                {
                    yield return 0;
                }
            }
            yield return m_searchResultIdxBuilding.Value = results.Select(x => x.Second).ToArray();
            yield return m_searchResultIdxBuildingName.Value = results.Select(x => x.First).ToArray();
            if (autoselect != 0)
            {
                var autoSelectVal = Array.IndexOf(m_searchResult.Value, autoselect);
                if (autoSelectVal > 0)
                {
                    m_buildingIdxSetter(autoselect);
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
            }
        }
        private StateInstance m_currentState = StateInstance.Normal;
        private void GoTo(StateInstance newState) => m_currentState = newState;
        private void OnBuildingSet(int selectLayout, string _ = null)
        {
            if (autoselect > 0)
            {
                m_buildingIdxSetter(autoselect);
                ToolsModifierControl.SetTool<DefaultTool>();
                m_currentState = StateInstance.Normal;
            }
            autoselect = m_searchResultIdxBuilding.Value[selectLayout];

        }

        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AbsoluteMode);
        protected override string GetAssetName(BuildingParametersData item) => item.assetName;
        protected override void SetTextParameter(BuildingParametersData item, int currentEditingParam, string paramValue) => item.SetTextParameter(currentEditingParam, paramValue);
        protected override void DrawListing(Vector2 size, BuildingParametersData item, bool isEditable)
        {
            if (m_currentState == StateInstance.Normal)
            {
                if (SimulationManager.instance.m_currentTickIndex - lastTickDraw > 5)
                {
                    var target = m_buildingLayoutGetter();
                    if (target is null)
                    {
                        return;
                    }
                    m_cachedParamsUsed = target.GetAllParametersUsedWithData();
                }
                lastTickDraw = SimulationManager.instance.m_currentTickIndex;
                if ((m_cachedParamsUsed?.Count ?? 0) > 0)
                {
                    foreach (var kv in m_cachedParamsUsed.OrderBy(x => x.Key))
                    {
                        var contentTypes = kv.Value.GroupBy(x => x.First.GetTextContent()).Select(x => x.Key);
                        if (contentTypes.Count() > 1)
                        {
                            GUILayout.Label(string.Format(Str.WTS_ONNETEDITOR_TEXTPARAM, kv.Key));
                            GUILayout.Label(Str.WTS_ONNETEDITOR_INVALIDPARAMSETTINGS_DIFFERENTKINDSAMEPARAM, new GUIStyle(GUI.skin.label)
                            {
                                alignment = TextAnchor.MiddleCenter
                            });
                            GUILayout.Space(4);
                            continue;
                        }
                        var targetContentType = contentTypes.First();
                        string target = "FFFFFF";
                        switch (targetContentType)
                        {
                            case TextContent.ParameterizedSpriteSingle:
                                target = Image;
                                break;
                            case TextContent.ParameterizedSpriteFolder:
                                target = Folder;
                                break;
                            case TextContent.ParameterizedText:
                                target = Text;
                                break;
                            case TextContent.Any:
                                target = Any;
                                break;
                        }
                        var usedByText = string.Join("\n", kv.Value.Select(x => $"\u2022 {(x.First.GetParameterDisplayName().TrimToNull() ?? x.Second)}").ToArray());
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(string.Format(Str.WTS_ONNETEDITOR_TEXTPARAM, kv.Key) + $"\n<color=#{target}>{targetContentType.ValueToI18n()}</color>\n\n{usedByText}");
                            var param = item.GetParameter(kv.Key);
                            if (GUILayout.Button(param is null ? GUIKwyttoCommons.v_null : param.IsEmpty ? GUIKwyttoCommons.v_empty : param.ToString(), GUILayout.ExpandHeight(true)))
                            {
                                if (param is null)
                                {
                                    item.SetTextParameter(kv.Key, null);
                                    param = item.GetParameter(kv.Key);
                                }
                                GoToPicker(kv.Key, targetContentType, param, item);
                            }
                        };
                    }
                }
                else
                {
                    GUILayout.Label(Str.we_buildingEditor_noParametersUsedThisInstance);
                }
            }
            else
            {
                m_instanceFilter.DrawSelectorView(size.y);
            }
        }


    }
}
