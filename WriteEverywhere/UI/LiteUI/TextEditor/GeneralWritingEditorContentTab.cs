﻿using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorContentTab : WTSBaseParamsTab<TextToWriteOnXml>
    {
        protected override TextRenderingClass RenderingClass => m_targetRenderingClass;
        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_DiskDrive);

        private static readonly TextContent[] m_contents = new[] { TextContent.ParameterizedText, TextContent.ParameterizedSpriteSingle, TextContent.ParameterizedSpriteFolder, TextContent.TextParameterSequence };
        private static readonly string[] m_optionsContent = m_contents.Select(x => x.ValueToI18n()).ToArray();
        private static readonly TextContent[] m_contentsExibition = new[] { TextContent.None, TextContent.ParameterizedText, TextContent.ParameterizedSpriteSingle };
        private static readonly string[] m_optionsContentExibition = new[] {
            Str.we_generalTextEditor_previewTypeVariablePath,
            Str.we_generalTextEditor_previewTypeSetText,
            Str.we_generalTextEditor_previewTypeImageFrame
        };

        private readonly GUIRootWindowBase m_root;
        private readonly Func<PrefabInfo> infoGetter;
        private readonly TextRenderingClass m_targetRenderingClass;
        private readonly Texture2D m_reload = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Reload);

        public GeneralWritingEditorContentTab(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, TextRenderingClass targetRenderingClass) : base()
        {
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            this.infoGetter = infoGetter;
            this.m_targetRenderingClass = targetRenderingClass;
        }
        private Vector2 scrollParamsPos;
        protected override void DrawListing(Vector2 tabAreaSize, TextToWriteOnXml currentItem, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_TEXT_CONTENTVALUE_TAB}</i>");
            var item = currentItem;
            GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_TEXT_CONTENT, ref item.textContent, m_optionsContent, m_contents, m_root, isEditable);
            switch (item.textContent)
            {

                case TextContent.ParameterizedSpriteFolder:
                case TextContent.ParameterizedSpriteSingle:
                case TextContent.ParameterizedText:
                    var param = item.Value;
                    GUIKwyttoCommons.AddButtonSelector(tabAreaSize.x, Str.WTS_CONTENT_TEXTVALUE, param is null ? GUIKwyttoCommons.v_null : param.IsEmpty ? GUIKwyttoCommons.v_empty : param.ToString(), () => OnGoToPicker(currentItem, -1), isEditable);
                    if (param?.IsParameter ?? false)
                    {
                        GUIKwyttoCommons.TextWithLabel(tabAreaSize.x, Str.we_generalTextEditor_labelForParamListing, item.ParameterDisplayName, (x) => item.ParameterDisplayName = x, isEditable);
                    }
                    if (SceneUtils.IsAssetEditor && item.textContent == TextContent.ParameterizedText)
                    {
                        GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_assetEditorExibitionText, ref item.assetEditorPreviewContentType, m_optionsContentExibition, m_contentsExibition, m_root, isEditable);
                        if (item.assetEditorPreviewContentType == TextContent.ParameterizedText)
                        {
                            GUIKwyttoCommons.TextWithLabel(tabAreaSize.x, Str.we_generalTextEditor_assetEditorPreviewSetText, item.AssetEditorPreviewText ?? "", (x) => item.AssetEditorPreviewText = x);
                        }
                    }
                    if (item.textContent != TextContent.ParameterizedText && SceneUtils.IsAssetEditor && infoGetter().m_isCustomContent)
                    {
                        GUILayout.Space(4);
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(Str.we_generalTextEditor_goToAssetSpritesFolder, GUILayout.Height(30)))
                            {
                                var path = Path.Combine(KFileUtils.GetRootFolderForK45(infoGetter()), WEMainController.EXTRA_SPRITES_FILES_FOLDER_ASSETS);
                                KFileUtils.EnsureFolderCreation(path);
                                ColossalFramework.Utils.OpenInFileBrowser(path);
                            }
                            GUIKwyttoCommons.SquareTextureButton2(m_reload, Str.we_assetEditor_reloadAssetImages, () => ModInstance.Controller.AtlasesLibrary.ReloadAssetImages());
                        }
                    }
                    break;
                case TextContent.TextParameterSequence:
                    if (item.ParameterSequence is null)
                    {
                        item.ParameterSequence = new TextParameterSequence(new[] { new TextParameterSequenceItem("", m_targetRenderingClass) }, m_targetRenderingClass);
                    }
                    var paramSeq = item.ParameterSequence;
                    if (paramSeq.Any(x => x.IsParameter))
                    {
                        GUIKwyttoCommons.TextWithLabel(tabAreaSize.x, Str.we_generalTextEditor_labelForParamListing, item.ParameterDisplayName, (x) => item.ParameterDisplayName = x, isEditable);
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("#", GUILayout.Width(25));
                        GUILayout.Label(Str.WTS_CONTENT_TEXTVALUE);
                        GUILayout.Label(Str.WTS_PARAMSEQ_STEPLENGTH, new GUIStyle(GUI.skin.label)
                        {
                            alignment = TextAnchor.MiddleRight,
                            wordWrap = false,
                        }, GUILayout.Width(100));
                        GUILayout.Label(Str.WTS_PARAMSEQ_ACTIONS, GUILayout.Width(60));
                        GUILayout.Space(30);
                        var rect = GUILayoutUtility.GetLastRect();
                        rect.height = 18;
                        if (isEditable && GUI.Button(rect, "+"))
                        {
                            AddItem(paramSeq);
                        }
                    }
                    using (var scrollPanel = new GUILayout.ScrollViewScope(scrollParamsPos))
                    {
                        var line = 0;
                        foreach (var seqItem in paramSeq)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label((line + 1).ToString(), GUILayout.Width(25));
                                GUIKwyttoCommons.AddButtonSelector(seqItem?.Value?.ToString(), () => OnGoToPicker(currentItem, line), isEditable, new GUIStyle(GUI.skin.button)
                                {
                                    alignment = TextAnchor.MiddleLeft,
                                    wordWrap = false,
                                });
                                if (isEditable)
                                {
                                    int newLenght;
                                    if ((newLenght = GUIIntField.IntField($"{Str.WTS_PARAMSEQ_STEPLENGTH} " + line, (int)seqItem.m_length, 0).Value) != seqItem.m_length)
                                    {
                                        paramSeq.SetLengthAt(line, newLenght);
                                    }
                                }
                                else
                                {
                                    GUILayout.Label(seqItem.m_length.ToString("#,##0") + "Fr.", GUILayout.Width(25));
                                }
                                GUILayout.Space(30);
                                var rect = GUILayoutUtility.GetLastRect();
                                rect.height = 18;
                                if (isEditable && line > 0 && GUI.Button(rect, "↑"))
                                {
                                    MoveUp(paramSeq, line);
                                }
                                GUILayout.Space(30);
                                rect = GUILayoutUtility.GetLastRect();
                                rect.height = 18;
                                if (isEditable && line < paramSeq.TotalItems - 1 && GUI.Button(rect, "↓"))
                                {
                                    MoveDown(paramSeq, line);
                                }
                                GUILayout.Space(30);
                                rect = GUILayoutUtility.GetLastRect();
                                rect.height = 18;
                                if (isEditable && line > 0 && GUI.Button(rect, "X"))
                                {
                                    RemoveAt(paramSeq, line);
                                }
                            }
                            line++;
                        }
                        scrollParamsPos = scrollPanel.scrollPosition;
                    }
                    break;

            }
        }

        private void MoveUp(TextParameterSequence paramSeq, int line) => paramSeq.MoveUp(line);
        private void MoveDown(TextParameterSequence paramSeq, int line) => paramSeq.MoveDown(line);
        private void RemoveAt(TextParameterSequence paramSeq, int line) => paramSeq.RemoveAt(line);
        private void AddItem(TextParameterSequence paramSeq) => paramSeq.Add(new TextParameterWrapper(), 250);
        private void OnGoToPicker(TextToWriteOnXml currentItem, int key)
        {
            TextParameterWrapper value;
            if (key == -1)
            {
                value = currentItem.Value ?? new TextParameterWrapper();
            }
            else if (currentItem.ParameterSequence is TextParameterSequence tps && key >= 0 && key < tps.TotalItems)
            {
                value = currentItem.ParameterSequence.ElementAt(key).Value;
            }
            else
            {
                return;
            }
            GoToPicker(key, currentItem.textContent, value, currentItem);
        }

        protected override string GetAssetName(TextToWriteOnXml item) => infoGetter()?.name;
        protected override void SetTextParameter(TextToWriteOnXml item, int currentEditingParam, string paramValue)
        {
            if (currentEditingParam == -1)
            {
                item.SetDefaultParameterValueAsString(paramValue, m_targetRenderingClass);
            }
            else if (item.ParameterSequence is TextParameterSequence tps && currentEditingParam >= 0 && currentEditingParam < tps.TotalItems)
            {
                tps.SetTextAt(currentEditingParam, paramValue, m_targetRenderingClass);
            }
        }
        protected override PrefabInfo GetCurrentInfo() => infoGetter();
    }
}
