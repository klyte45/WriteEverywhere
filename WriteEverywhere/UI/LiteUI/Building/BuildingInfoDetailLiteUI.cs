extern alias VS;

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using HarmonyLib;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class BuildingInfoDetailLiteUI
    {
        private enum State
        {
            Normal,
            GeneralFontPicker
        }
        public ConfigurationSource CurrentSource => m_currentSource;
        internal WriteOnBuildingXml CurrentEditingLayout { get; set; }
        private int m_currentSubBuilding;
        private ConfigurationSource m_currentSource;

        private readonly Texture2D m_goToFile;
        private readonly Texture2D m_reloadFiles;
        private readonly Texture2D m_createNew;
        private readonly Texture2D m_deleteFromCity;
        private readonly Texture2D m_cloneToCity;
        private readonly Texture2D m_exportGlobal;
        private readonly Texture2D m_exportAsset;
        private readonly Texture2D m_copy;
        private readonly Texture2D m_paste;
        private readonly Texture2D m_importLib;
        private readonly Texture2D m_exportLib;
        private readonly GUIColorPicker m_colorPicker;
        private readonly GUIRootWindowBase m_root;

        private readonly GUIBasicListingTabsContainer<WriteOnBuildingPropXml> m_tabsContainer;
        private BuildingUITextTab m_textEditorTab;
        private int m_textEditorTabIdx;

        private string m_clipboard;
        private int[] m_cachedItemListIdx;
        private string[] m_cachedItemListLabels;
        private float m_offsetYContent;

        private readonly GUIXmlLib<WTSLibOnBuildingPropLayoutList, ExportableBoardInstanceOnBuildingListXml> xmlLibList = new GUIXmlLib<WTSLibOnBuildingPropLayoutList, ExportableBoardInstanceOnBuildingListXml>()
        {
            DeleteQuestionI18n = Str.WTS_SEGMENT_CLEARDATA_AYS,
            ImportI18n = Str.WTS_SEGMENT_IMPORTDATA,
            ExportI18n = Str.WTS_SEGMENT_EXPORTDATA,
            DeleteButtonI18n = Str.WTS_SEGMENT_REMOVEITEM,
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,

        };

        private FooterBarStatus CurrentLibState => xmlLibList.Status;
        private State CurrentLocalState { get; set; } = State.Normal;

        public int PropSel => m_tabsContainer.ListSel >= 0 ? m_cachedItemListIdx[m_tabsContainer.ListSel] : -1;
        public int TextSel => m_textEditorTab.SelectedTextItem;
        public ushort CurrentGrabbedId { get; set; }
        public BuildingInfo CurrentEditingInfo { get; private set; }
        public bool IsOnTextDimensionsView => m_textEditorTab.IsOnTextDimensionsView;
        public bool IsOnTextEditor => m_textEditorTabIdx == m_tabsContainer.CurrentTabIdx;

        public BuildingInfoDetailLiteUI(GUIColorPicker colorPicker)
        {
            var viewAtlas = UIView.GetAView().defaultAtlas;

            m_goToFile = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Load);
            m_reloadFiles = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Reload);
            m_createNew = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_New);
            m_deleteFromCity = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
            m_cloneToCity = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Download);
            m_exportGlobal = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_DiskDrive);
            m_exportAsset = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Steam);
            m_copy = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Copy);
            m_paste = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Paste);
            m_importLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Import);
            m_exportLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Export);

            m_colorPicker = colorPicker;
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();

            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, OnSelectFont, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);


            var root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            var tabs = new IGUITab<WriteOnBuildingPropXml>[] {
                new BuildingUIBasicTab(OnImportSingle, OnDelete, root),
                m_textEditorTab =  new BuildingUITextTab(m_colorPicker, () => CurrentEditingLayout.PropInstances[PropSel].SimpleProp, () =>
                {
                    if(PropSel>=0 && CurrentEditingLayout.PropInstances[PropSel] != null)
                    {
                        return ref CurrentEditingLayout.PropInstances[PropSel].RefTextDescriptors;
                    }
                    throw new System.Exception("Invalid call!!!");
                }, () =>
                {
                    if(PropSel>=0 && CurrentEditingLayout?.PropInstances[PropSel] != null)
                    {
                        return ref CurrentEditingLayout.PropInstances[PropSel].RefFontName;
                    }
                    throw new System.Exception("Invalid call!!!");
                })
            };
            m_tabsContainer = new GUIBasicListingTabsContainer<WriteOnBuildingPropXml>(tabs, OnAdd, GetSideList, GetSelectedItem, OnSetCurrentItem, AddExtraButtonsList);
            m_textEditorTabIdx = Array.IndexOf(tabs, m_textEditorTab);
        }

        public void DoDraw(Rect area, int subbuildingIdx, BuildingInfo parentBuilding)
        {
            if (CurrentEditingInfo != parentBuilding || subbuildingIdx != m_currentSubBuilding)
            {
                OnChangeInfo(parentBuilding, subbuildingIdx);
            }
            if (CurrentEditingInfo is null)
            {
                return;
            }
            switch (CurrentLibState)
            {
                case FooterBarStatus.Normal:
                case FooterBarStatus.AskingToExport:
                case FooterBarStatus.AskingToExportOverwrite:
                case FooterBarStatus.AskingToRemove:
                    switch (CurrentLocalState)
                    {
                        case State.Normal:
                            RegularDraw(area.size);
                            break;
                        case State.GeneralFontPicker:
                            m_fontFilter.DrawSelectorView(area.height - 20);
                            break;
                    }
                    break;
                case FooterBarStatus.AskingToImport:
                case FooterBarStatus.AskingToImportAdditive:
                    xmlLibList.DrawImportView(
                        (x, isAdd) =>
                    {
                        var newItems = x.Instances.Select(y =>
                        {
                            y.SubBuildingPivotReference = subbuildingIdx;
                            return y;
                        }).ToArray();
                        if (isAdd)
                        {
                            CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.AddRangeToArray(newItems);
                        }
                        else
                        {
                            CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.Where(y => y.SubBuildingPivotReference != subbuildingIdx).Concat(newItems).ToArray();
                        }
                        SaveAndReload();
                    }
                    );
                    break;
            }

        }

        private void RegularDraw(Vector2 size)
        {
            using (new GUILayout.VerticalScope())
            {
                var isEditable = m_currentSource == ConfigurationSource.CITY || m_currentSource == ConfigurationSource.SKIN;
                using (new GUILayout.HorizontalScope(GUILayout.Height(70)))
                {
                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        var skinNoWrap = new GUIStyle(GUI.skin.label) { wordWrap = false };
                        GUILayout.Label($"<color=#FFFF00>{Str.WTS_CURRENTSELECTION}</color>", skinNoWrap, GUILayout.MaxWidth(140));
                        GUILayout.Label(CurrentEditingInfo.GetUncheckedLocalizedTitle(), skinNoWrap);
                        GUILayout.Label($"<color=#FFFF00>{Str.WTS_CURRENTLY_USING}</color>", skinNoWrap);
                        GUILayout.Label(m_currentSource.ValueToI18n(), skinNoWrap, GUILayout.MaxWidth(140));
                        GUILayout.FlexibleSpace();
                    }
                    using (new GUILayout.VerticalScope(GUILayout.MaxWidth(300)))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            if (CurrentLibState == FooterBarStatus.Normal)
                            {
                                GUI.tooltip = "";
                                GUILayout.FlexibleSpace();
                                GUIKwyttoCommons.SquareTextureButton(m_goToFile, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_OPENGLOBALSFOLDER, GoToGlobalFolder);
                                GUIKwyttoCommons.SquareTextureButton(m_reloadFiles, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_RELOADDESCRIPTORS, ReloadFiles);
                                GUILayout.FlexibleSpace();
                                GUIKwyttoCommons.SquareTextureButton(m_createNew, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_NEWINCITY, CreateNew, !isEditable);
                                GUIKwyttoCommons.SquareTextureButton(m_deleteFromCity, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_DELETEFROMCITY, DeleteFromCity, m_currentSource == ConfigurationSource.CITY);
                                GUIKwyttoCommons.SquareTextureButton(m_cloneToCity, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCITY, CloneToCity, m_currentSource == ConfigurationSource.ASSET || m_currentSource == ConfigurationSource.GLOBAL);
                                GUILayout.FlexibleSpace();
                                GUIKwyttoCommons.SquareTextureButton(m_exportGlobal, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTASGLOBAL, ExportGlobal, m_currentSource == ConfigurationSource.CITY);
                                GUIKwyttoCommons.SquareTextureButton(m_exportAsset, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTTOASSETFOLDER, ExportAsset, m_currentSource == ConfigurationSource.CITY && CurrentEditingInfo.name.EndsWith("_Data"));
                                GUILayout.FlexibleSpace();
                                GUIKwyttoCommons.SquareTextureButton(m_copy, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCLIPBOARD, CopyToClipboard, m_currentSource != ConfigurationSource.NONE);
                                GUIKwyttoCommons.SquareTextureButton(m_paste, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_PASTEFROMCLIPBOARD, PasteFromClipboard, isEditable && !(m_clipboard is null));
                                if (isEditable)
                                {
                                    GUILayout.FlexibleSpace();
                                    GUIKwyttoCommons.SquareTextureButton(m_importLib, Str.WTS_IMPORTLAYOUT_LIB, ImportLayout);
                                    GUIKwyttoCommons.SquareTextureButton(m_exportLib, Str.WTS_EXPORTLAYOUT_LIB, ExportLayout);
                                }
                                else if (m_currentSource != ConfigurationSource.NONE)
                                {
                                    GUILayout.FlexibleSpace();
                                    GUIKwyttoCommons.SquareTextureButton(m_exportLib, Str.WTS_EXPORTLAYOUT_LIB, ExportLayout);
                                }
                            }
                            else
                            {
                                xmlLibList.Draw(WEUIUtils.RedButton, OnClearList, OnGetExportableList);
                            }
                        }
                        GUILayout.FlexibleSpace();
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
                if (isEditable)
                {
                    m_fontFilter.DrawButton(size.x, CurrentEditingLayout?.FontName);
                }
                else if (m_currentSource != ConfigurationSource.NONE)
                {
                    m_fontFilter.DrawButtonDisabled(size.x, CurrentEditingLayout?.FontName);
                }
                if (m_currentSource != ConfigurationSource.NONE)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        var lastRect = GUILayoutUtility.GetLastRect();
                        m_offsetYContent = lastRect.yMax + 8;
                    }
                    m_tabsContainer.DrawListTabs(new Rect(0, m_offsetYContent, size.x, size.y - m_offsetYContent), isEditable);
                }
            }
        }

        private void OnChangeInfo(BuildingInfo parentBuilding, int subBuilding)
        {
            CurrentEditingInfo = parentBuilding;
            m_currentSubBuilding = subBuilding;
            ReloadDescriptor();
        }

        private void ReloadDescriptor()
        {
            WTSBuildingPropsSingleton.GetTargetDescriptor(CurrentEditingInfo.name, out m_currentSource, out var currentLayout);
            CurrentEditingLayout = currentLayout;
            var cachedItemList = currentLayout?.PropInstances.Select((x, i) => Tuple.New(i, x)).Where(x => x.Second.SubBuildingPivotReference == m_currentSubBuilding).Select((x) => new KeyValuePair<int, string>(x.First, x.Second.SaveName)).ToList();
            m_cachedItemListIdx = cachedItemList.Select(x => x.Key).ToArray();
            m_cachedItemListLabels = cachedItemList.Select(x => x.Value).ToArray();
            OnTabChanged(-1);
            m_tabsContainer.Reset();
            xmlLibList.ResetStatus();
        }

        private void OnTabChanged(int tabIdx)
        {

        }
        private void GoTo(State newState) => CurrentLocalState = newState;

        #region Extra buttons
        private void AddExtraButtonsList(bool canEdit)
        {
            if (canEdit && GUILayout.Button(Str.we_roadEditor_importAdding))
            {
                xmlLibList.GoToImportAdditive();
            }
            if (canEdit && GUILayout.Button(Str.we_roadEditor_importReplacing))
            {
                xmlLibList.GoToImport();
            }
            if (GUILayout.Button(Str.WTS_SEGMENT_EXPORTDATA))
            {
                xmlLibList.GoToExport();
            }
            if (canEdit && GUILayout.Button(Str.WTS_SEGMENT_CLEARDATA))
            {
                xmlLibList.GoToRemove();
            }
        }
        private void ExportLayout() => xmlLibList.GoToExport();
        private void ImportLayout() => xmlLibList.GoToImport();
        private void ExportAsset() => ExportTo(Path.Combine(Path.GetDirectoryName(PackageManager.FindAssetByName(CurrentEditingInfo.name)?.package?.packagePath), $"{MainController.m_defaultFileNameBuildingsXml}.xml"));

        private void ExportGlobal() => ExportTo(Path.Combine(MainController.DefaultBuildingsConfigurationFolder, $"{MainController.m_defaultFileNameBuildingsXml}_{CurrentEditingInfo.name}.xml"));

        private void ExportTo(string output)
        {
            if (!(CurrentEditingInfo is null))
            {
                var assetId = CurrentEditingInfo.name.Split('.')[0] + ".";
                WTSBuildingPropsSingleton.GetTargetDescriptor(CurrentEditingInfo.name, out _, out var target);
                if (target is WriteOnBuildingXml layout)
                {
                    layout.BuildingInfoName = CurrentEditingInfo.name;
                    File.WriteAllText(output, XmlUtils.DefaultXmlSerialize(layout));

                    KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                    {
                        title = Str.WTS_VEHICLE_EXPORTLAYOUT,
                        message = Str.WTS_VEHICLE_EXPORTLAYOUT_SUCCESSSAVEDATA,
                        scrollText = $"<color=#FFFF00>{output}</color>",
                        buttons = new[]
                        {
                            KwyttoDialog.SpaceBtn,
                            new KwyttoDialog.ButtonDefinition
                            {
                                title = KStr.comm_goToFileLocation,
                                onClick = ()=>{
                                    ColossalFramework.Utils.OpenInFileBrowser(output);
                                    return false;
                                }
                            },
                            new KwyttoDialog.ButtonDefinition
                            {
                                title = Locale.Get("EXCEPTION_OK"),
                                onClick = ()=>true,
                                style= KwyttoDialog.ButtonStyle.White
                            }
                        }
                    });

                    ModInstance.Controller?.BuildingPropsSingleton?.LoadAllBuildingConfigurations();
                }
            }
        }
        private void PasteFromClipboard()
        {
            WTSBuildingPropsSingleton.SetCityDescriptor(CurrentEditingInfo, XmlUtils.DefaultXmlDeserialize<WriteOnBuildingXml>(m_clipboard));

            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }

        private void CopyToClipboard() => m_clipboard = XmlUtils.DefaultXmlSerialize(CurrentEditingLayout);
        private void CloneToCity()
        {
            WTSBuildingPropsSingleton.SetCityDescriptor(CurrentEditingInfo, XmlUtils.CloneViaXml(CurrentEditingLayout));
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }
        private void CreateNew()
        {
            WTSBuildingPropsSingleton.SetCityDescriptor(CurrentEditingInfo, new WriteOnBuildingXml());
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }
        private void DeleteFromCity()
        {
            WTSBuildingPropsSingleton.SetCityDescriptor(CurrentEditingInfo, null);
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }

        private void ReloadFiles()
        {
            ModInstance.Controller?.BuildingPropsSingleton?.LoadAllBuildingConfigurations();
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }
        private void GoToGlobalFolder() => ColossalFramework.Utils.OpenInFileBrowser(MainController.DefaultBuildingsConfigurationFolder);
        #endregion

        #region Search font

        private readonly GUIFilterItemsScreen<State> m_fontFilter;
        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() || LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
            yield return 0;
        }
        private void OnSelectFont(int _, string fontName) => CurrentEditingLayout.FontName = fontName;
        #endregion


        #region Tab Actions
        private WriteOnBuildingPropXml GetSelectedItem(int listSel) => CurrentEditingLayout.PropInstances[m_cachedItemListIdx[listSel]];
        private string[] GetSideList() => m_cachedItemListLabels;
        private void OnAdd()
        {
            CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.Concat(new[] { new WriteOnBuildingPropXml() { SaveName = "NEW", SubBuildingPivotReference = m_currentSubBuilding } }).ToArray();
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }
        private void OnClearList()
        {
            CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.Where(x => x.SubBuildingPivotReference != m_currentSubBuilding).ToArray();
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }

        private void OnSetCurrentItem(int listSel, WriteOnBuildingPropXml newVal)
        {
            var effectiveIdx = m_cachedItemListIdx[listSel];
            if (newVal is null)
            {
                CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.Where((k, i) => i != effectiveIdx).ToArray();
                m_tabsContainer.Reset();
            }
            else
            {
                CurrentEditingLayout.PropInstances[effectiveIdx] = newVal;
            }
        }
        private ExportableBoardInstanceOnBuildingListXml OnGetExportableList() => new ExportableBoardInstanceOnBuildingListXml
        {
            Instances = CurrentEditingLayout.PropInstances.Where(x => x.SubBuildingPivotReference == m_currentSubBuilding).Select((x) => XmlUtils.CloneViaXml(x)).ToArray(),
        };

        private void OnDelete()
        {
            CurrentEditingLayout.PropInstances = CurrentEditingLayout.PropInstances.Where((k, i) => i != PropSel).ToArray();
            m_tabsContainer.Reset();
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
        }

        private void OnImportSingle(WriteOnBuildingPropXml data, bool _)
        {
            data.SaveName = CurrentEditingLayout.PropInstances[PropSel].SaveName;
            data.SubBuildingPivotReference = m_currentSubBuilding;
            CurrentEditingLayout.PropInstances[PropSel] = data;
            SaveAndReload();
        }

        private void SaveAndReload()
        {
            WTSBuildingPropsSingleton.SetCityDescriptor(CurrentEditingInfo, CurrentEditingLayout);
            var oldListSel = m_tabsContainer.ListSel;
            OnChangeInfo(CurrentEditingInfo, m_currentSubBuilding);
            m_tabsContainer.ListSel = oldListSel;
        }

        #endregion
    }

}
