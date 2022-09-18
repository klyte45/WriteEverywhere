using ColossalFramework.Globalization;
using WriteEverywhere.Localization;
using System;
using System.Linq;
using UnityEngine;

namespace WriteEverywhere.UI
{
    internal class WTSParameterImageEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsImg = new[] { Str.WTS_IMAGESRC_ASSET, Str.WTS_IMAGESRC_LOCAL };
        public void DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            bool dirtyInput;
            bool dirtyType;
            using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
            {
                var modelType = GUILayout.SelectionGrid(tab.IsLocal ? 1 : 0, v_protocolsImg, v_protocolsImg.Length);
                dirtyType = tab.IsLocal != (modelType == 1);
                if (dirtyType)
                {
                    tab.SetLocal(modelType == 1);
                    tab.ClearSelectedFolder();
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
                tab.RestartFilterCoroutine();
            }
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
        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            var texture = tab.currentFolderAtlas?.sprites.Where(x => x.name == tab.SelectedValue).FirstOrDefault()?.texture;
            if (texture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture, ScaleMode.ScaleToFit, true);
            }
        }
        public bool IsText { get; } = true;
        public void OnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout)
        {
            if (tab.SelectedFolder is null)
            {
                tab.SetSelectedFolder(tab.m_searchResult.Value[selectLayout] == "<ROOT>" ? "" : tab.m_searchResult.Value[selectLayout]);
                tab.m_searchResult.Value = new string[0];
                tab.RestartFilterCoroutine();
            }
            else
            {
                tab.SetSelectedValue(tab.m_searchResult.Value[selectLayout]);
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
    }
}
