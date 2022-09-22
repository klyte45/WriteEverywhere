using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSParameterFolderEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsFld = new[] { Str.WTS_FOLDERSRC_ASSET, Str.WTS_FOLDERSRC_LOCAL };

        public bool IsText { get; } = false;

        public int HoverIdx => 0;

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

        public void DrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            var selectLayout = GUILayout.SelectionGrid(Array.IndexOf(tab.m_searchResult.Value, tab.SelectedValue), tab.m_searchResult.Value, 1, new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft
            }, GUILayout.Width((areaRect.x / 2) - 25));
            if (selectLayout >= 0)
            {
                OnSelectItem(tab, selectLayout);
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

        public void OnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout)
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

        public string[] OnFilterParam(WTSBaseParamsTab<T> tab)
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
                        : ModInstance.Controller.AtlasesLibrary.FindByInAssetSimple(ulong.TryParse(tab.SearchPropName.Split('.')[0] ?? "", out ulong wId) ? wId : 0u, tab.SearchText, out tab.currentFolderAtlas);
                case WTSBaseParamsTab<T>.State.GettingFolder:
                    return tab.IsLocal
                        ? ModInstance.Controller.AtlasesLibrary.FindByInLocalFolders(tab.SearchText)
                        : ModInstance.Controller.AtlasesLibrary.HasAtlas(ulong.TryParse(tab.SearchPropName?.Split('.')[0] ?? "", out ulong wId2) ? wId2 : 0) ? new string[] { "<ROOT>" } : new string[0];
            }
            return null;
        }

        public void OnHoverVar(WTSBaseParamsTab<T> wTSBaseParamsTab, int autoSelectVal, CommandLevel commandLevel)
        {
        }
    }
}
