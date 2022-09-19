
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorForegroundTab : IGUITab<BoardTextDescriptorGeneralXml>
    {
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_FontIcon);

        private readonly GUIColorPicker m_picker;
        private readonly string[] m_alignmentOptions = EnumI18nExtensions.GetAllValuesI18n<UIHorizontalAlignment>();
        private readonly string[] m_fontClasses = EnumI18nExtensions.GetAllValuesI18n<FontClass>();
        private readonly GUIRootWindowBase m_root;

        public GeneralWritingEditorForegroundTab(GUIColorPicker colorPicker)
        {
            m_picker = colorPicker;
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, null, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);

        }

        public bool DrawArea(Vector2 tabAreaSize, ref BoardTextDescriptorGeneralXml currentItem, int currentItemIdx)
        {
            var item = currentItem;
            bool isEditable = true;
            switch (CurrentLocalState)
            {
                case State.Normal:
                    NormalDraw(tabAreaSize, item, isEditable);
                    break;
                case State.GeneralFontPicker:
                    m_fontFilter.OnSelect = (_, x) => OnSelectFont(item, x);
                    m_fontFilter.DrawSelectorView(tabAreaSize.y);
                    break;
            }

            return false;
        }

        private void NormalDraw(Vector2 tabAreaSize, BoardTextDescriptorGeneralXml item, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_FONTFACE_SETTINGS}</i>");
            bool useContrast = item.ColoringConfig.UseContrastColor;
            if (GUIKwyttoCommons.AddToggle(Str.WTS_USE_CONTRAST_COLOR, ref useContrast, isEditable))
            {
                item.ColoringConfig.UseContrastColor = useContrast;
            }
            if (!useContrast)
            {
                GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_COLOR, m_picker, ref item.ColoringConfig.m_cachedColor, isEditable);
            }
            else
            {
                GUILayout.Space(12);
            }
            GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_DEPTH, ref item.IlluminationConfig.m_illuminationDepth, -1, 1, isEditable);

            if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_TEXT_ALIGN_HOR, (int)item.m_textAlign, m_alignmentOptions, out var newVal, m_root, isEditable))
            {
                item.m_textAlign = (UIHorizontalAlignment)newVal;
            }
            if (isEditable)
            {
                m_fontFilter.DrawButton(tabAreaSize.x, item.m_overrideFont);
            }
            else if (isEditable)
            {
                m_fontFilter.DrawButtonDisabled(tabAreaSize.x, item.m_overrideFont);
            }
            if (item.m_overrideFont is null)
            {
                if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_CLASS_FONT, (int)item.m_fontClass, m_fontClasses, out var newVal1, m_root, isEditable))
                {
                    item.m_fontClass = (FontClass)newVal1;
                }
            }
        }

        public void Reset() { }
        private enum State
        {
            Normal,
            GeneralFontPicker
        }
        private State CurrentLocalState { get; set; } = State.Normal;

        private void GoTo(State newState) => CurrentLocalState = newState;

        #region Search font

        private readonly GUIFilterItemsScreen<State> m_fontFilter;
        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
            yield return 0;
        }
        private void OnSelectFont(BoardTextDescriptorGeneralXml item, string fontName) => item.m_overrideFont = fontName;
        #endregion
    }
}
