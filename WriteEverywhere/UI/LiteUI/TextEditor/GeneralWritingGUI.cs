using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.UI;
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
    internal delegate ref T RefGetter<T>();
    internal class GeneralWritingGUI
    {
        private enum State
        {
            Normal,
            GeneralFontPicker
        }


        private readonly GUIColorPicker m_colorPicker;
        private readonly Func<PrefabInfo> getInfo;
        private readonly GUIBasicListingTabsContainer<BoardTextDescriptorGeneralXml> m_tabsContainer;


        private string m_clipboard;
        private string[] m_cachedItemList;
        private float m_offsetYContent;
        //private readonly GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml> m_vehicleLib = new GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml>()
        //{
        //    DeleteQuestionI18n = "K45_WTS_PROPEDIT_CONFIGDELETE_MESSAGE",
        //    NameAskingI18n = "K45_WTS_EXPORTDATA_NAMEASKING",
        //    NameAskingOverwriteI18n = "K45_WTS_EXPORTDATA_NAMEASKING_OVERWRITE"
        //};

        private FooterBarStatus CurrentLibState => FooterBarStatus.Normal;
        private State CurrentLocalState { get; set; } = State.Normal;

        private readonly RefGetter<BoardTextDescriptorGeneralXml[]> GetDescriptorArray;
        private readonly RefGetter<string> GetFont;

        public GeneralWritingGUI(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, RefGetter<BoardTextDescriptorGeneralXml[]> getDescriptorArray, RefGetter<string> getFont)
        {
            var viewAtlas = UIView.GetAView().defaultAtlas;


            m_colorPicker = colorPicker;
            this.getInfo = infoGetter;
            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, OnSelectFont, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);
            var uicomp = WTSOnNetLiteUI.Instance.GetComponent<UIComponent>();
            m_tabsContainer = new GUIBasicListingTabsContainer<BoardTextDescriptorGeneralXml>(
                new IGUITab<BoardTextDescriptorGeneralXml>[]{
                    new GeneralWritingEditorGeneralTab(),
                    new GeneralWritingEditorPositionsSizesTab(),
                    new GeneralWritingEditorForegroundTab(m_colorPicker),
                    new GeneralWritingEditorBoxSettingsTab(m_colorPicker),
                    new GeneralWritingEditorIlluminationTab(m_colorPicker),
                    new GeneralWritingEditorContentTab(m_colorPicker,infoGetter)
                    },
                OnAddItem,
                GetList,
                GetCurrentItem, SetCurrentItem);
            GetDescriptorArray = getDescriptorArray;
            GetFont = getFont;
            // m_tabsContainer.EventListItemChanged += OnTabChanged;
        }

        private BoardTextDescriptorGeneralXml GetCurrentItem(int arg) => GetDescriptorArray()[arg];
        private string[] GetList() => m_cachedItemList;
        private void OnAddItem()
        {
            GetDescriptorArray() = GetDescriptorArray().Concat(new[] { new BoardTextDescriptorGeneralXml() { SaveName = "NEW" } }).ToArray();
            m_cachedItemList = GetDescriptorArray().Select(x => x.SaveName).ToArray();
        }

        private void SetCurrentItem(int arg, BoardTextDescriptorGeneralXml val)
        {
            if (val is null)
            {
                GetDescriptorArray() = GetDescriptorArray().Where((x, i) => i != arg).ToArray();
                m_tabsContainer.Reset();
            }
            else
            {
                GetDescriptorArray()[arg] = val;
            }
            m_cachedItemList = GetDescriptorArray().Select(x => x.SaveName).ToArray();
        }

        public void DoDraw(Rect area)
        {
            if (getInfo() is null)
            {
                return;
            }
            switch (CurrentLibState)
            {
                case FooterBarStatus.Normal:
                case FooterBarStatus.AskingToExport:
                case FooterBarStatus.AskingToExportOverwrite:
                    switch (CurrentLocalState)
                    {
                        case State.Normal:
                            RegularDraw(area.size);
                            break;
                        case State.GeneralFontPicker:
                            m_fontFilter.DrawSelectorView(area.height);
                            break;
                    }
                    break;
                case FooterBarStatus.AskingToImport:
                    //m_vehicleLib.DrawImportView((x) => GeneralWritingEditorTextsSingleton.SetCityDescriptor(m_currentInfo, x));
                    break;
            }

        }

        private void RegularDraw(Vector2 size)
        {
            m_tabsContainer.DrawListTabs(new Rect(0, 0, size.x, size.y), true);
        }

        private void GoTo(State newState) => CurrentLocalState = newState;


        #region Search font

        private readonly GUIFilterItemsScreen<State> m_fontFilter;
        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() || LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
            yield return 0;
        }
        private void OnSelectFont(int _, string fontName) => GetFont() = fontName;
        #endregion
    }
}
