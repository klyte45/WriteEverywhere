using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using Kwytto.Interfaces;
using Kwytto.LiteUI;
using Kwytto.UI;
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;
using static Kwytto.Utils.XmlUtils;

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
        private readonly GUIBasicListingTabsContainer<TextToWriteOnXml> m_tabsContainer;


        private string[] m_cachedItemList;
        private readonly GUIXmlLib<WTSLibTextList, ILibableAsContainer<TextToWriteOnXml>> m_textGroupLib = new GUIXmlLib<WTSLibTextList, ILibableAsContainer<TextToWriteOnXml>>()
        {
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,
            DeleteButtonI18n = Str.WTS_SEGMENT_CLEARDATA,
            ExportI18n = Str.we_generalLib_exportFullList,
            ImportI18n = Str.we_generalLib_importFullList,
            DeleteQuestionI18n = Str.WTS_SEGMENT_CLEARDATA_AYS,
            ImportAdditiveI18n = Str.we_generalLib_importAdditive,
        };

        private FooterBarStatus CurrentLibState => m_textGroupLib.Status;
        private State CurrentLocalState { get; set; } = State.Normal;

        public virtual void Reset()
        {
            m_textGroupLib.ResetStatus();
        }

        private readonly RefGetter<TextToWriteOnXml[]> GetDescriptorArray;
        private readonly RefGetter<string> GetFont;
        public int SelectedTextItem => m_tabsContainer.ListSel;
        public bool IsOnTextDimensionsView => m_tabsContainer.CurrentTabIdx == m_sizeEditorTabIdx;
        private readonly int m_sizeEditorTabIdx;
        public readonly TextRenderingClass m_targetRenderingClass;

        public GeneralWritingGUI(GUIColorPicker colorPicker, TextRenderingClass targetRenderingClass, Func<PrefabInfo> infoGetter, RefGetter<TextToWriteOnXml[]> getDescriptorArray, RefGetter<string> getFont)
        {
            var viewAtlas = UIView.GetAView().defaultAtlas;

            m_targetRenderingClass = targetRenderingClass;
            m_colorPicker = colorPicker;
            this.getInfo = infoGetter;
            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, OnSelectFont, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);
            var uicomp = WTSOnNetLiteUI.Instance.GetComponent<UIComponent>();
            GeneralWritingEditorPositionsSizesTab positionTab;
            var tabs = new IGUITab<TextToWriteOnXml>[]{
                    new GeneralWritingEditorGeneralTab(()=>getDescriptorArray()),
                     positionTab = new GeneralWritingEditorPositionsSizesTab(colorPicker.GetComponentInParent<GUIRootWindowBase>()),
                    new GeneralWritingEditorForegroundTab(m_colorPicker),
                    new GeneralWritingEditorBgMeshSettingsTab(m_colorPicker,infoGetter),
                    new GeneralWritingEditorFrameSettingsTab(m_colorPicker,infoGetter),
                    new GeneralWritingEditorIlluminationTab(m_colorPicker),
                    new GeneralWritingEditorContentTab(m_colorPicker,infoGetter,targetRenderingClass)
                    };
            m_tabsContainer = new GUIBasicListingTabsContainer<TextToWriteOnXml>(
               tabs,
                OnAddItem,
                GetList,
                GetCurrentItem, SetCurrentItem);

            m_sizeEditorTabIdx = Array.IndexOf(tabs, positionTab);
            GetDescriptorArray = getDescriptorArray;
            GetFont = getFont;
        }

        private TextToWriteOnXml GetCurrentItem(int arg) => GetDescriptorArray()[arg];
        private string[] GetList() => m_cachedItemList;
        public void ReloadList() => m_cachedItemList = GetDescriptorArray().Select(x => x.SaveName).ToArray();
        private void OnAddItem()
        {
            GetDescriptorArray() = GetDescriptorArray().Concat(new[] { new TextToWriteOnXml() { SaveName = "NEW", } }).ToArray();
            ReloadList();
        }

        private void SetCurrentItem(int arg, TextToWriteOnXml val)
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
            ReloadList();
        }

        public void DoDraw(Rect area, bool allowEdit)
        {
            if (getInfo() is null)
            {
                GUILayout.Label(Str.we_textEditor_needPropSelectedWarning);
                return;
            }
            switch (CurrentLibState)
            {
                case FooterBarStatus.AskingToImport:
                case FooterBarStatus.AskingToImportAdditive:
                    using (new GUILayout.AreaScope(new Rect(5 * GUIWindow.ResolutionMultiplier, 30 * GUIWindow.ResolutionMultiplier, area.width - 10 * GUIWindow.ResolutionMultiplier, area.height - 30 * GUIWindow.ResolutionMultiplier)))
                    {
                        m_textGroupLib.DrawImportView((x, y) =>
                        {
                            GetDescriptorArray() = (y ? GetDescriptorArray() : new TextToWriteOnXml[0]).AddRangeToArray(x.m_dataArray);
                            ReloadList();
                        });
                    }
                    break;
                default:
                    switch (CurrentLocalState)
                    {
                        case State.Normal:
                            RegularDraw(area.size, allowEdit);
                            break;
                        case State.GeneralFontPicker:
                            m_fontFilter.DrawSelectorView(area.height);
                            break;
                    }
                    break;
            }

        }

        private void RegularDraw(Vector2 size, bool allowEdit)
        {
            m_tabsContainer.DrawListTabs(new Rect(0, 20 * GUIWindow.ResolutionMultiplier, size.x, size.y - 20 * GUIWindow.ResolutionMultiplier), allowEdit, true);
            using (new GUILayout.AreaScope(new Rect(0, 0, size.x, 20 * GUIWindow.ResolutionMultiplier)))
            {
                m_textGroupLib.Draw(WEUIUtils.RedButton, () =>
                {
                    GetDescriptorArray() = new TextToWriteOnXml[0];
                    ReloadList();
                }, () => new ILibableAsContainer<TextToWriteOnXml> { Data = new ListWrapper<TextToWriteOnXml>() { listVal = GetDescriptorArray().ToList() } }, m_textGroupLib.FooterDraw);
            }
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
