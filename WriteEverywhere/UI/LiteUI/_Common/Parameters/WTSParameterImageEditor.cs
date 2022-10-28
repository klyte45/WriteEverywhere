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
    internal class WTSParameterImageEditor<T> : IWTSParameterEditor<T>
    {
        private readonly string[] v_protocolsImg = new[] { Str.WTS_IMAGESRC_ASSET, Str.WTS_IMAGESRC_LOCAL };

        public int HoverIdx => 0;
        private Func<IIndexedPrefabData> m_prefabIndexGetter;

        public WTSParameterImageEditor(Func<IIndexedPrefabData> prefabIndexGetter)
        {
            m_prefabIndexGetter = prefabIndexGetter;
        }
        public float DrawTop(WTSBaseParamsTab<T> tab, Vector2 areaRect)
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
                tab.RestartFilterCoroutine(this);
            }
            return 50;
        }
        public static void ImageDrawLeftPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect, IWTSParameterEditor<T> paramEditor)
        {
            var selectLayout = GUILayout.SelectionGrid(Array.IndexOf(tab.m_searchResult.Value, tab.SelectedValue), tab.m_searchResult.Value, 1, new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft
            }, GUILayout.Width((areaRect.x / 2) - 25 * GUIWindow.ResolutionMultiplier));
            if (selectLayout >= 0)
            {
                ImageOnSelectItem(tab, selectLayout, paramEditor);
            }

        }
        public static void ImageDrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect)
        {
            using (new GUILayout.AreaScope(new Rect(default, areaRect)))
            {
                var selectedImageInfo = tab.currentFolderAtlas?.Where(x => x.Key == tab.SelectedValue).FirstOrDefault().Value;
                if (selectedImageInfo != null)
                {
                    var texture = selectedImageInfo.Texture;
                    using (new GUILayout.VerticalScope())
                    {
                        if (GUIKwyttoCommons.AddVector4Field(areaRect.x, selectedImageInfo.Borders, Str.we_generalTextEditor_imageBordersLBRTlabel, "imageBordersLBRTlabel", out Vector4 result, !(selectedImageInfo.xmlPath is null), 0, 1))
                        {
                            selectedImageInfo.Borders = result;
                        }
                        if (GUIKwyttoCommons.AddFloatField(areaRect.x, Str.we_generalTextEditor_imageDensityPixelsPerMeters, selectedImageInfo.PixelsPerMeter, out var newVal, !(selectedImageInfo.xmlPath is null), 0))
                        {
                            selectedImageInfo.PixelsPerMeter = newVal;
                        }
                        if (!(selectedImageInfo.xmlPath is null))
                        {
                            if (GUILayout.Button(Str.we_generalTextEditor_imageSaveImageInfoBtn))
                            {
                                selectedImageInfo.Save();
                            }
                        }
                        GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_imageShowBorders, ref showBorders);
                    }
                    using (var scroll = new GUILayout.ScrollViewScope(scrollImageRightPanel))
                    {
                        GUI.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture, ScaleMode.ScaleToFit, true);
                        if (showBorders)
                        {
                            var lastRect = new Rect(0, 0, texture.width, texture.height);
                            if (borderOverlayTexture is null)
                            {
                                borderOverlayTexture = new Texture2D(1, 1);
                                borderOverlayTexture.SetPixels(new[] { new Color(1, 0, 1, .5f) });
                                borderOverlayTexture.Apply();
                            }
                            var offsets = selectedImageInfo.OffsetBorders;
                            GUI.DrawTexture(new Rect(lastRect.xMin, lastRect.yMin, offsets.left, lastRect.height), borderOverlayTexture, ScaleMode.StretchToFill, true);
                            GUI.DrawTexture(new Rect(lastRect.width - offsets.right, lastRect.yMin, offsets.right, lastRect.height), borderOverlayTexture, ScaleMode.StretchToFill, true);

                            GUI.DrawTexture(new Rect(lastRect.xMin, lastRect.yMin, lastRect.width, offsets.top), borderOverlayTexture, ScaleMode.StretchToFill, true);
                            GUI.DrawTexture(new Rect(lastRect.xMin, lastRect.height - offsets.bottom, lastRect.width, offsets.bottom), borderOverlayTexture, ScaleMode.StretchToFill, true);

                        }
                        scrollImageRightPanel = scroll.scrollPosition;
                    }
                }
            }
        }
        static Vector2 scrollImageRightPanel;
        private static bool showBorders = true;
        private static Texture2D borderOverlayTexture;
        public bool IsText { get; } = true;
        public static void ImageOnSelectItem(WTSBaseParamsTab<T> tab, int selectLayout, IWTSParameterEditor<T> paramEditor)
        {
            if (tab.SelectedFolder is null)
            {
                tab.SetSelectedFolder(tab.m_searchResult.Value[selectLayout] == "<ROOT>" ? "" : tab.m_searchResult.Value[selectLayout]);
                tab.m_searchResult.Value = new string[0];
                tab.RestartFilterCoroutine(paramEditor);
            }
            else
            {
                if (selectLayout == 0)
                {
                    tab.ClearSelectedValue();
                    tab.ClearSelectedFolder();
                    tab.m_searchResult.Value = new string[0];
                    tab.RestartFilterCoroutine(paramEditor);
                }
                else
                {
                    tab.SetSelectedValue(tab.m_searchResult.Value[selectLayout]);
                }
            }
        }

        public static string[] ImageOnFilterParam(WTSBaseParamsTab<T> tab, Func<IIndexedPrefabData> prefabIndexGetter)
        {
            switch (tab.CurrentState)
            {
                case WTSBaseParamsTab<T>.State.GettingImage:
                case WTSBaseParamsTab<T>.State.GettingAny:
                    if (tab.SelectedFolder is null && tab.IsLocal)
                    {
                        goto case WTSBaseParamsTab<T>.State.GettingFolder;
                    }
                    return tab.IsLocal
                        ? ModInstance.Controller.AtlasesLibrary.FindByInLocalSimple(tab.SelectedFolder == "<ROOT>" ? null : tab.SelectedFolder, tab.SearchText, out tab.currentFolderAtlas)
                        : ModInstance.Controller.AtlasesLibrary.FindByInAssetSimple(prefabIndexGetter(), tab.SearchText, out tab.currentFolderAtlas);
                case WTSBaseParamsTab<T>.State.GettingFolder:
                    return tab.IsLocal
                        ? ModInstance.Controller.AtlasesLibrary.FindByInLocalFolders(tab.SearchText)
                        : ModInstance.Controller.AtlasesLibrary.HasAssetAtlas(prefabIndexGetter()) ? new string[] { "<ROOT>" } : new string[0];
            }
            return null;
        }

        public void DrawLeftPanel(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, Vector2 areaRect) => ImageDrawLeftPanel(tab, areaRect, this);
        public void DrawRightPanel(WTSBaseParamsTab<T> tab, Vector2 areaRect) => ImageDrawRightPanel(tab, areaRect);
        public string[] OnFilterParam(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab) => ImageOnFilterParam(tab, m_prefabIndexGetter);
        public void OnSelectItem(TextRenderingClass renderingClass, WTSBaseParamsTab<T> tab, int selectLayout) => ImageOnSelectItem(tab, selectLayout, this);

        public void OnHoverVar(TextRenderingClass renderingClass, WTSBaseParamsTab<T> wTSBaseParamsTab, int autoSelectVal, BaseCommandLevel commandLevel)
        {
        }
    }
}
