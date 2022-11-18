using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSParameterFolderEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsFld = new[] { Str.WTS_FOLDERSRC_ASSET, Str.WTS_FOLDERSRC_LOCAL };

        public bool IsText { get; } = false;

        public int HoverIdx => 0;

        private Func<IIndexedPrefabData> m_prefabIndexGetter;

        public WTSParameterFolderEditor(Func<IIndexedPrefabData> prefabIndexGetter)
        {
            m_prefabIndexGetter = prefabIndexGetter;
        }

        public float DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            bool dirtyType = false;
            if (ulong.TryParse(tab.SearchPropName?.Split('.')[0] ?? "", out _))
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
                {
                    var modelType = GUILayout.SelectionGrid(tab.IsLocal ? 1 : 0, v_protocolsFld, v_protocolsFld.Length);
                    dirtyType = tab.IsLocal != (modelType == 1);
                    if (dirtyType)
                    {
                        tab.SetLocal(modelType == 1);
                        tab.ClearSelectedFolder();
                    }
                };
            }
            else
            {
                dirtyType = tab.IsLocal;
                tab.SetLocal(true);
            }

            bool dirtyInput = false;
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
            return 50;
        }

        public void DrawLeftPanel(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            var selectLayout = GUILayout.SelectionGrid(Array.IndexOf(tab.m_searchResult.Value, tab.SelectedValue), tab.m_searchResult.Value, 1, new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft
            }, GUILayout.Width((areaRect.x / 2) - 25));
            if (selectLayout >= 0)
            {
                OnSelectItem(renderingClass, tab, selectLayout);
            }

        }
        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 _)
        {
            if (tab.currentFolderAtlas != null)
            {
                GUILayout.Label(string.Join("\n", tab.currentFolderAtlas.OrderBy(x => x.Key).Select(x => $"\u2022 {x.Key}").ToArray()), new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true,
                });
            }
        }

        public void OnSelectItem(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, int selectLayout)
        {
            tab.SetSelectedFolder(tab.m_searchResult.Value[selectLayout] == "<ROOT>" ? "" : tab.m_searchResult.Value[selectLayout]);
            if (tab.IsLocal)
            {
                ModInstance.Controller.AtlasesLibrary.GetSpriteLib(tab.SelectedFolder, out tab.currentFolderAtlas);
            }
            else if (ulong.TryParse(tab.SearchPropName?.Split('.')[0] ?? "", out ulong wId))
            {
                tab.SetSelectedFolder("");
                ModInstance.Controller.AtlasesLibrary.GetSpriteLib(wId.ToString(), out tab.currentFolderAtlas);
            }
        }

        public string[] OnFilterParam(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab)
        {
            switch (tab.CurrentState)
            {
                case WTSBaseParamsTab<T>.State.GettingImage:
                    if (tab.SelectedFolder is null && tab.IsLocal)
                    {
                        goto case WTSBaseParamsTab<T>.State.GettingFolder;
                    }
                    return tab.IsLocal
                        ? ModInstance.Controller.AtlasesLibrary.FindByInLocalSimple(tab.SelectedFolder == "<ROOT>" ? null : tab.SelectedFolder, tab.SearchText, out tab.currentFolderAtlas)
                        : ModInstance.Controller.AtlasesLibrary.FindByInAssetSimple(m_prefabIndexGetter(), tab.SearchText, out tab.currentFolderAtlas);
                case WTSBaseParamsTab<T>.State.GettingFolder:
                    return tab.IsLocal
                        ? ModInstance.Controller.AtlasesLibrary.FindByInLocalFolders(tab.SearchText)
                        : ModInstance.Controller.AtlasesLibrary.HasAssetAtlas(m_prefabIndexGetter()) ? new string[] { "<ROOT>" } : new string[0];
            }
            return null;
        }

        public void OnHoverVar(TextRenderingClass renderingClass, WTSBaseParamsTab<T> wTSBaseParamsTab, int autoSelectVal, BaseCommandLevel commandLevel)
        {
        }
    }
}
