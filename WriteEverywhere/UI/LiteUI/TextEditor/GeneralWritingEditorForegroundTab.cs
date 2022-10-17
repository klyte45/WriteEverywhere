
using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorForegroundTab : IGUITab<TextToWriteOnXml>
    {
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_FontIcon);

        private readonly GUIColorPicker m_picker;
        private readonly string[] m_fontClasses = EnumI18nExtensions.GetAllValuesI18n<FontClass>();
        private readonly GUIRootWindowBase m_root;

        private readonly string[] contrastOptions;
        private readonly ColoringSource[] contrastValues;
        private readonly TextRenderingClass srcClass;

        public GeneralWritingEditorForegroundTab(GUIColorPicker colorPicker, TextRenderingClass srcClass)
        {
            m_picker = colorPicker;
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, null, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);
            contrastValues = ColoringSourceExtensions.AvailableAtClass(srcClass);
            contrastOptions = contrastValues.Select(x => x.ValueToI18n()).ToArray();
            this.srcClass = srcClass;

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

            bool isPlatformRelative = srcClass != TextRenderingClass.Vehicle && (item.ColoringConfig.m_colorSource == ColoringSource.PlatformLine || item.ColoringConfig.m_colorSource == ColoringSource.ContrastPlatformLine);
            GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useFixedIfMultiline, ref item.ColoringConfig.m_useFixedIfMultiline, isEditable, isPlatformRelative);

            if (item.ColoringConfig.m_colorSource == ColoringSource.Fixed || (item.ColoringConfig.m_useFixedIfMultiline && isPlatformRelative))
            {
                GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_COLOR, m_picker, item.ColoringConfig.m_cachedColor, (x) => item.ColoringConfig.m_cachedColor = x ?? Color.white, isEditable);
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
                GUIKwyttoCommons.AddColorPicker(Str.we_generalTextEditor_fontBackfaceColor, m_picker, item.ColoringConfig.m_cachedBackColor, (x) => item.ColoringConfig.m_cachedBackColor = x.Value, isEditable);
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
