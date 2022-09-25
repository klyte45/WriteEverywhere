using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorGeneralTab : IGUITab<TextToWriteOnXml>
    {
        private readonly Func<TextToWriteOnXml[]> listGetter;
        public GeneralWritingEditorGeneralTab(Func<TextToWriteOnXml[]> listGetter)
        {
            this.listGetter = listGetter;
            m_deleteItem = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
            m_copy = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Copy);
            m_paste = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Paste);
            m_importLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Import);
            m_exportLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Export);
        }

        private readonly Texture2D m_deleteItem;
        private readonly Texture2D m_copy;
        private readonly Texture2D m_paste;
        private readonly Texture2D m_importLib;
        private readonly Texture2D m_exportLib;
        private string m_clipboard;
        private FooterBarStatus CurrentLibState => m_textItemLib.Status;

        private readonly GUIXmlLib<WTSLibTextItem, TextToWriteOnXml> m_textItemLib = new GUIXmlLib<WTSLibTextItem, TextToWriteOnXml>()
        {
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,
            DeleteQuestionI18n = Str.WTS_SEGMENT_REMOVEITEM,
        };

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Settings);

        public bool DrawArea(Vector2 tabAreaRect, ref TextToWriteOnXml currentItem, int currentItemIdx, bool canEdit)
        {
            GUILayout.Label($"<i>{Str.WTS_GENERAL_SETTINGS}</i>");
            var item = currentItem;
            var hasChanges = false;
            var wrapper = new Wrapper<TextToWriteOnXml>(item);

            if (CurrentLibState == FooterBarStatus.AskingToImport)
            {
                m_textItemLib.DrawImportView((x, _) => wrapper.Value = x);
            }
            else
            {
                hasChanges |= GUIKwyttoCommons.TextWithLabel(tabAreaRect.x, Str.WTS_TEXT_TAB_TITLE, item.SaveName, (x) => ChangeName(x, item), canEdit);

                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (CurrentLibState == FooterBarStatus.Normal)
                        {
                            GUI.tooltip = "";
                            GUILayout.FlexibleSpace();
                            GUIKwyttoCommons.SquareTextureButton(m_deleteItem, Str.WTS_DELETETEXTITEM, () => wrapper.Value = null, canEdit);
                            GUILayout.FlexibleSpace();
                            GUIKwyttoCommons.SquareTextureButton(m_copy, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCLIPBOARD, () => CopyToClipboard(item));
                            GUIKwyttoCommons.SquareTextureButton(m_paste, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_PASTEFROMCLIPBOARD, () => PasteFromClipboard(wrapper), canEdit && !(m_clipboard is null));
                            GUILayout.FlexibleSpace();
                            GUIKwyttoCommons.SquareTextureButton(m_importLib, Str.WTS_IMPORTLAYOUT_LIB, ImportLayout, canEdit);
                            GUIKwyttoCommons.SquareTextureButton(m_exportLib, Str.WTS_EXPORTLAYOUT_LIB, ExportLayout);
                        }
                        else
                        {
                            m_textItemLib.Draw(null, null, () => item);
                        }
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"<i>{GUI.tooltip}</i>", new GUIStyle(GUI.skin.label)
                        {
                            richText = true,
                            alignment = TextAnchor.MiddleRight
                        }, GUILayout.Height(40));
                        GUI.tooltip = "";
                    }
                    bool changed = GUIKwyttoCommons.AddVector3Field(tabAreaRect.x, ref WTSUtils.axisRotationTG, "ROT VEC TG", "ROTATIOON", true, -1, 1);
                    changed |= GUIKwyttoCommons.AddFloatField(tabAreaRect.x, "ROT DEG TG", ref WTSUtils.degRotationTG, true, -180, 180);
                    changed |= GUIKwyttoCommons.AddVector3Field(tabAreaRect.x, ref WTSUtils.axisRotationN, "ROT VEC NM", "ROTATIOON2", true, -1, 1);
                    changed |= GUIKwyttoCommons.AddFloatField(tabAreaRect.x, "ROT DEG NM", ref WTSUtils.degRotationN, true, -180, 180);
                    changed |= GUIKwyttoCommons.AddVector4Field(tabAreaRect.x, WTSUtils.tangent, "ROT VEC NM", "ROTATIOON2", out WTSUtils.tangent, true, -1, 1);
                    if (changed)
                    {
                        foreach (var board in listGetter())
                        {
                            board.BackgroundMeshSettings.FrameMeshSettings.GlassSpecularLevel = board.BackgroundMeshSettings.FrameMeshSettings.GlassSpecularLevel;
                        }
                    }
                }
            }
            if (wrapper.Value != item)
            {
                currentItem = wrapper.Value;
                return true;
            }
            else
            {
                return hasChanges;
            }
        }

        private void ChangeName(string x, TextToWriteOnXml item) => item.SaveName = x;

        public void Reset() => m_textItemLib.ResetStatus();

        #region Action buttons
        private void ExportLayout() => m_textItemLib.GoToExport();
        private void ImportLayout() => m_textItemLib.GoToImport();
        private void PasteFromClipboard(Wrapper<TextToWriteOnXml> wrapper)
        {
            var item = XmlUtils.DefaultXmlDeserialize<TextToWriteOnXml>(m_clipboard);
            item.SaveName = wrapper.Value.SaveName;
            wrapper.Value = item;
        }

        private void CopyToClipboard(TextToWriteOnXml item) => m_clipboard = XmlUtils.DefaultXmlSerialize(item);

        #endregion
    }
}
