﻿
using ColossalFramework;
using ColossalFramework.Globalization;
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
    internal class GeneralWritingEditorForegroundTab : IGUITab<TextToWriteOnXml>
    {
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_FontIcon);

        private readonly GUIColorPicker m_picker;
        private readonly string[] m_fontClasses = EnumI18nExtensions.GetAllValuesI18n<FontClass>();
        private readonly GUIRootWindowBase m_root;

        private static readonly string[] contrastOptions = EnumI18nExtensions.GetAllValuesI18n<ColoringSource>();
        private static readonly ColoringSource[] contrastValues = Enum.GetValues(typeof(ColoringSource)).Cast<ColoringSource>().ToArray();

        public GeneralWritingEditorForegroundTab(GUIColorPicker colorPicker)
        {
            m_picker = colorPicker;
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, null, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);

        }

        public bool DrawArea(Vector2 tabAreaSize, ref TextToWriteOnXml currentItem, int currentItemIdx, bool isEditable)
        {
            var item = currentItem;
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

        private void NormalDraw(Vector2 tabAreaSize, TextToWriteOnXml item, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_FONTFACE_SETTINGS}</i>");
            GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_colorSource, ref item.ColoringConfig.m_colorSource, contrastOptions, contrastValues, m_root, isEditable);
            GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useFixedIfMultiline, ref item.ColoringConfig.m_useFixedIfMultiline, isEditable, item.ColoringConfig.m_colorSource == ColoringSource.PlatformLine || item.ColoringConfig.m_colorSource == ColoringSource.ContrastPlatformLine);

            if (item.ColoringConfig.m_colorSource == ColoringSource.Fixed || (item.ColoringConfig.m_useFixedIfMultiline && (item.ColoringConfig.m_colorSource == ColoringSource.PlatformLine || item.ColoringConfig.m_colorSource == ColoringSource.ContrastPlatformLine)))
            {
                GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_COLOR, m_picker, ref item.ColoringConfig.m_cachedColor, isEditable);
            }
            else
            {
                GUILayout.Space(12);
            }

            bool backFaceIsFrontFace = item.ColoringConfig.UseFrontColorAsBackColor;
            if (GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_backfaceFontColorIsSameAsFrontFace, ref backFaceIsFrontFace, isEditable))
            {
                item.ColoringConfig.UseFrontColorAsBackColor = backFaceIsFrontFace;
            }
            if (!backFaceIsFrontFace)
            {
                GUIKwyttoCommons.AddColorPicker(Str.we_generalTextEditor_fontBackfaceColor, m_picker, ref item.ColoringConfig.m_cachedBackColor, isEditable);
            }
            else
            {
                GUILayout.Space(12);
            }

            GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_DEPTH, ref item.IlluminationConfig.m_illuminationDepth, -1000, 1000, isEditable);

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
        private void OnSelectFont(TextToWriteOnXml item, string fontName) => item.m_overrideFont = fontName;
        #endregion
    }
}
