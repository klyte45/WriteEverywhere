﻿using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorGeneralTab : IGUITab<BoardTextDescriptorGeneralXml>
    {
        public GeneralWritingEditorGeneralTab()
        {
            var viewAtlas = UIView.GetAView().defaultAtlas;
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

        private readonly GUIXmlLib<WTSLibTextItem, BoardTextDescriptorGeneralXml> m_textItemLib = new GUIXmlLib<WTSLibTextItem, BoardTextDescriptorGeneralXml>()
        {
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,
            DeleteQuestionI18n = Str.WTS_SEGMENT_REMOVEITEM,
        };

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Settings);

        public bool DrawArea(Vector2 tabAreaRect, ref BoardTextDescriptorGeneralXml currentItem, int currentItemIdx)
        {
            GUILayout.Label($"<i>{Str.WTS_GENERAL_SETTINGS}</i>");
            var canEdit = true;
            var item = currentItem;
            var hasChanges = false;
            var wrapper = new Wrapper<BoardTextDescriptorGeneralXml>(item);

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

        private void ChangeName(string x, BoardTextDescriptorGeneralXml item) => item.SaveName = x;

        public void Reset() => m_textItemLib.ResetStatus();

        #region Action buttons
        private void ExportLayout() => m_textItemLib.GoToExport();
        private void ImportLayout() => m_textItemLib.GoToImport();
        private void PasteFromClipboard(Wrapper<BoardTextDescriptorGeneralXml> wrapper)
        {
            var item = XmlUtils.DefaultXmlDeserialize<BoardTextDescriptorGeneralXml>(m_clipboard);
            item.SaveName = wrapper.Value.SaveName;
            wrapper.Value = item;
        }

        private void CopyToClipboard(BoardTextDescriptorGeneralXml item) => m_clipboard = XmlUtils.DefaultXmlSerialize(item);

        #endregion
    }
}