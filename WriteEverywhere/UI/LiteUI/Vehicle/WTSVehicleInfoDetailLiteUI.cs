//using ColossalFramework;
//using ColossalFramework.Globalization;
//using ColossalFramework.Packaging;
//using ColossalFramework.UI;
//using Kwytto.LiteUI;
//using Kwytto.UI;
//using Kwytto.Utils;
//using SpriteFontPlus;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using UnityEngine;
//using WriteEverywhere.Libraries;
//using WriteEverywhere.Xml;

//namespace WriteEverywhere.UI
//{
//    internal class WTSVehicleInfoDetailLiteUI
//    {
//        private enum State
//        {
//            Normal,
//            GeneralFontPicker
//        }

//        private VehicleInfo m_currentInfo;
//        private VehicleInfo m_currentParentInfo;
//        private string m_currentSkin;
//        private string[] m_availableSkins;
//        private string[] m_availableSkinsOptions;
//        private ConfigurationSource m_currentSource;
//        private LayoutDescriptorVehicleXml m_currentLayout;

//        private readonly Texture2D m_goToFile;
//        private readonly Texture2D m_reloadFiles;
//        private readonly Texture2D m_createNew;
//        private readonly Texture2D m_deleteFromCity;
//        private readonly Texture2D m_cloneToCity;
//        private readonly Texture2D m_exportGlobal;
//        private readonly Texture2D m_exportAsset;
//        private readonly Texture2D m_copy;
//        private readonly Texture2D m_paste;
//        private readonly Texture2D m_importLib;
//        private readonly Texture2D m_save;
//        private readonly Texture2D m_exportLib;
//        private readonly WTSVehicleLayoutEditorPreview m_preview;
//        private readonly GUIColorPicker m_colorPicker;
//        private readonly GUIRootWindowBase m_root;


//        private readonly GUIBasicListingTabsContainer<BoardTextDescriptorGeneralXml> m_tabsContainer;


//        private string m_clipboard;
//        private string[] m_cachedItemList;
//        private float m_offsetYContent;
//        private readonly GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml> m_vehicleLib = new GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml>()
//        {
//            DeleteQuestionI18n = "K45_WTS_PROPEDIT_CONFIGDELETE_MESSAGE",
//            NameAskingI18n = "K45_WTS_EXPORTDATA_NAMEASKING",
//            NameAskingOverwriteI18n = "K45_WTS_EXPORTDATA_NAMEASKING_OVERWRITE"
//        };

//        private FooterBarStatus CurrentLibState => m_vehicleLib.Status;
//        private State CurrentLocalState { get; set; } = State.Normal;

//        public WTSVehicleInfoDetailLiteUI(GUIColorPicker colorPicker)
//        {
//            var viewAtlas = UIView.GetAView().defaultAtlas;

//            m_goToFile = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Load);
//            m_reloadFiles = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Reload);
//            m_createNew = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_New);
//            m_deleteFromCity = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
//            m_cloneToCity = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Download);
//            m_exportGlobal = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_DiskDrive);
//            m_exportAsset = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Steam);
//            m_copy = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Copy);
//            m_paste = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Paste);
//            m_importLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Import);
//            m_exportLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Export);
//            m_save = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Save);

//            m_colorPicker = colorPicker;
//            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();

//            m_fontFilter = new GUIFilterItemsScreen<State>(Locale.Get("K45_WTS_OVERRIDE_FONT"), ModInstance.Controller, OnFilterParam, OnSelectFont, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);


//            var uicomp = WTSVehicleLiteUI.Instance.GetComponent<UIComponent>();
//            GameObjectUtils.CreateUIElement(out UIPanel previewContainer, WTSVehicleLiteUI.Instance.transform, "previewContainer", new UnityEngine.Vector4(uicomp.width, 0, 500, 200));
//            m_preview = previewContainer.gameObject.AddComponent<WTSVehicleLayoutEditorPreview>();
//            uicomp.eventSizeChanged += (x, y) => m_preview.component.area = new UnityEngine.Vector4(y.x, 0, 500, 200);

//            m_preview.component.isVisible = false;
//            var root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
//            m_tabsContainer = new GUIBasicListingTabsContainer<BoardTextDescriptorGeneralXml>(
//                new IGUITab<BoardTextDescriptorGeneralXml>[]{
//                    new GeneralWritingEditorGeneralTab(),
//                    new GeneralWritingEditorPositionsSizesTab(root),
//                    new GeneralWritingEditorForegroundTab(m_colorPicker),
//                    new GeneralWritingEditorBoxSettingsTab(m_colorPicker, ()=> m_currentInfo),
//                    new GeneralWritingEditorIlluminationTab(m_colorPicker),
//                    new GeneralWritingEditorContentTab(m_colorPicker, ()=> m_currentInfo, Rendering.TextRenderingClass.Vehicle)
//                    },
//                OnAddItem,
//                GetList,
//                GetCurrentItem, SetCurrentItem);
//            m_tabsContainer.EventListItemChanged += OnTabChanged;
//        }

//        private BoardTextDescriptorGeneralXml GetCurrentItem(int arg) => m_currentLayout.TextDescriptors[arg];
//        private string[] GetList() => m_cachedItemList;
//        private void OnAddItem()
//        {
//            m_currentLayout.TextDescriptors = m_currentLayout.TextDescriptors.Concat(new[] { new BoardTextDescriptorGeneralXml() { SaveName = "NEW" } }).ToArray();
//            m_cachedItemList = m_currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
//        }

//        private void SetCurrentItem(int arg, BoardTextDescriptorGeneralXml val)
//        {
//            if (val is null)
//            {
//                m_currentLayout.TextDescriptors = m_currentLayout.TextDescriptors.Where((x, i) => i != arg).ToArray();
//                m_tabsContainer.Reset();
//            }
//            else
//            {
//                m_currentLayout.TextDescriptors[arg] = val;
//            }
//            m_cachedItemList = m_currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
//        }

//        public void DoDraw(Rect area, VehicleInfo vehicleInfo, VehicleInfo parentVehicle)
//        {
//            if (m_currentInfo != vehicleInfo)
//            {
//                OnChangeInfo(vehicleInfo, parentVehicle);
//            }
//            if (m_currentInfo is null)
//            {
//                return;
//            }
//            if (GUIKwyttoCommons.AddComboBox(area.width, "K45_WTS_LAYOUTSKIN", ref m_currentSkin, m_availableSkinsOptions, m_availableSkins, m_root))
//            {
//                ReloadSkin();
//            }
//            switch (CurrentLibState)
//            {
//                case FooterBarStatus.Normal:
//                case FooterBarStatus.AskingToExport:
//                case FooterBarStatus.AskingToExportOverwrite:
//                    switch (CurrentLocalState)
//                    {
//                        case State.Normal:
//                            RegularDraw(area.size);
//                            break;
//                        case State.GeneralFontPicker:
//                            m_fontFilter.DrawSelectorView(area.height);
//                            break;
//                    }
//                    break;
//                case FooterBarStatus.AskingToImport:
//                    m_vehicleLib.DrawImportView((x, _) => WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, x));
//                    break;
//            }

//        }

//        private void RegularDraw(Vector2 size)
//        {
//            using (new GUILayout.VerticalScope())
//            {
//                var isEditable = m_currentSource == ConfigurationSource.CITY || m_currentSource == ConfigurationSource.SKIN;
//                using (new GUILayout.HorizontalScope(GUILayout.Height(70)))
//                {
//                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
//                    {
//                        GUILayout.FlexibleSpace();
//                        GUILayout.Label($"<color=#FFFF00>{Locale.Get("K45_WTS_CURRENTSELECTION")}</color>\n{m_currentInfo.name}");
//                        GUILayout.Label($"<color=#FFFF00>{Locale.Get("K45_WTS_CURRENTLY_USING")}</color>\n{Locale.Get("K45_WTS_CONFIGURATIONSOURCE", m_currentSource.ToString())}");
//                        GUILayout.FlexibleSpace();
//                    }
//                    using (new GUILayout.VerticalScope(GUILayout.MaxWidth(300)))
//                    {
//                        using (new GUILayout.HorizontalScope())
//                        {
//                            if (CurrentLibState == FooterBarStatus.Normal)
//                            {
//                                GUI.tooltip = "";
//                                GUILayout.FlexibleSpace();
//                                GUIKwyttoCommons.SquareTextureButton(m_goToFile, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_OPENGLOBALSFOLDER"), GoToGlobalFolder);
//                                GUIKwyttoCommons.SquareTextureButton(m_reloadFiles, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_RELOADDESCRIPTORS"), ReloadFiles);
//                                GUILayout.FlexibleSpace();
//                                GUIKwyttoCommons.SquareTextureButton(m_createNew, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_NEWINCITY"), CreateNew, !isEditable);
//                                GUIKwyttoCommons.SquareTextureButton(m_deleteFromCity, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_DELETEFROMCITY"), DeleteFromCity, m_currentSource == ConfigurationSource.CITY);
//                                GUIKwyttoCommons.SquareTextureButton(m_cloneToCity, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCITY"), CloneToCity, m_currentSource == ConfigurationSource.ASSET || m_currentSource == ConfigurationSource.GLOBAL);
//                                GUILayout.FlexibleSpace();
//                                GUIKwyttoCommons.SquareTextureButton(m_exportGlobal, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTASGLOBAL"), ExportGlobal, m_currentSource == ConfigurationSource.CITY);
//                                GUIKwyttoCommons.SquareTextureButton(m_exportAsset, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTTOASSETFOLDER"), ExportAsset, m_currentSource == ConfigurationSource.CITY && m_currentInfo.name.EndsWith("_Data"));
//                                GUIKwyttoCommons.SquareTextureButton(m_save, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_SAVESKIN"), ExportSkin, m_currentSource == ConfigurationSource.SKIN);
//                                GUILayout.FlexibleSpace();
//                                GUIKwyttoCommons.SquareTextureButton(m_copy, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCLIPBOARD"), CopyToClipboard, m_currentSource != ConfigurationSource.NONE);
//                                GUIKwyttoCommons.SquareTextureButton(m_paste, Locale.Get("K45_WTS_BUILDINGEDITOR_BUTTONROWACTION_PASTEFROMCLIPBOARD"), PasteFromClipboard, isEditable && !(m_clipboard is null));
//                                if (isEditable)
//                                {
//                                    GUILayout.FlexibleSpace();
//                                    GUIKwyttoCommons.SquareTextureButton(m_importLib, Locale.Get("K45_WTS_IMPORTLAYOUT_LIB"), ImportLayout);
//                                    GUIKwyttoCommons.SquareTextureButton(m_exportLib, Locale.Get("K45_WTS_EXPORTLAYOUT_LIB"), ExportLayout);
//                                }
//                                else if (m_currentSource != ConfigurationSource.NONE)
//                                {
//                                    GUILayout.FlexibleSpace();
//                                    GUIKwyttoCommons.SquareTextureButton(m_exportLib, Locale.Get("K45_WTS_EXPORTLAYOUT_LIB"), ExportLayout);
//                                }
//                            }
//                            else
//                            {
//                                m_vehicleLib.Draw(null, null, () => m_currentLayout);
//                            }
//                        }
//                        GUILayout.FlexibleSpace();
//                        using (new GUILayout.HorizontalScope())
//                        {
//                            GUILayout.Label($"<i>{GUI.tooltip}</i>", new GUIStyle(GUI.skin.label)
//                            {
//                                richText = true,
//                                alignment = TextAnchor.MiddleRight
//                            }, GUILayout.Height(40));
//                            GUI.tooltip = "";
//                        }

//                    }
//                }
//                if (isEditable)
//                {
//                    m_fontFilter.DrawButton(size.x, m_currentLayout?.FontName);
//                }
//                else if (m_currentSource != ConfigurationSource.NONE)
//                {
//                    m_fontFilter.DrawButtonDisabled(size.x, m_currentLayout?.FontName);
//                }
//                if (m_currentSource != ConfigurationSource.NONE)
//                {
//                    if (Event.current.type == EventType.Repaint)
//                    {
//                        var lastRect = GUILayoutUtility.GetLastRect();
//                        m_offsetYContent = lastRect.yMax + 8;
//                    }
//                    m_tabsContainer.DrawListTabs(new Rect(0, m_offsetYContent, size.x, size.y - m_offsetYContent), isEditable);
//                }
//            }
//        }
//        private void OnChangeInfo(VehicleInfo vehicleInfo, VehicleInfo parentVehicle, string skin = "")
//        {
//            m_currentInfo = vehicleInfo;
//            m_currentParentInfo = parentVehicle;
//            if (m_currentInfo is null)
//            {
//                return;
//            }
//            m_currentSkin = skin;
//            m_availableSkins = ModInstance.Controller.ConnectorVS.ListAllSkins(vehicleInfo);
//            m_availableSkinsOptions = m_availableSkins.Select(x => x.IsNullOrWhiteSpace() ? "<DEFAULT>" : x).ToArray();
//            ReloadSkin();
//        }

//        private void ReloadSkin()
//        {
//            WTSVehicleTextsSingleton.GetTargetDescriptor(m_currentInfo, -1, out m_currentSource, out m_currentLayout, m_currentSkin);
//            m_preview.component.isVisible = m_currentSource != ConfigurationSource.NONE;
//            m_cachedItemList = m_currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
//            OnTabChanged(-1);
//            m_tabsContainer.Reset();
//            m_vehicleLib.ResetStatus();
//        }

//        private void OnTabChanged(int tabIdx) => m_preview.SetParams(m_currentInfo, tabIdx, m_currentSkin, m_currentLayout);
//        private void GoTo(State newState) => CurrentLocalState = newState;

//        #region Top buttons
//        private void ExportLayout() => m_vehicleLib.GoToExport();
//        private void ImportLayout() => m_vehicleLib.GoToImport();
//        private void ExportAsset() => ExportTo(Path.Combine(Path.GetDirectoryName(PackageManager.FindAssetByName(m_currentInfo.name)?.package?.packagePath), $"{MainController.m_defaultFileNameVehiclesXml}.xml"));

//        private void ExportGlobal() => ExportTo(Path.Combine(MainController.DefaultVehiclesConfigurationFolder, $"{MainController.m_defaultFileNameVehiclesXml}_{PackageManager.FindAssetByName(m_currentParentInfo.name)?.package.packageMainAsset ?? m_currentParentInfo.name}.xml"));
//        private void ExportSkin() => ModInstance.Controller.ConnectorVS.ApplySkin(m_currentInfo, m_currentSkin, XmlUtils.DefaultXmlSerialize(m_currentLayout));

//        private void ExportTo(string output)
//        {
//            if (!(m_currentInfo is null))
//            {
//                var assetId = m_currentInfo.name.Split('.')[0] + ".";
//                var descriptorsToExport = new List<LayoutDescriptorVehicleXml>();
//                foreach (var asset in VehiclesIndexes.instance.PrefabsData
//                .Where((x) => x.Value.PrefabName.StartsWith(assetId) || x.Value.PrefabName == m_currentInfo.name)
//                .Select(x => x.Value.Info))
//                {
//                    WTSVehicleTextsSingleton.GetTargetDescriptor(asset as VehicleInfo, -1, out _, out LayoutDescriptorVehicleXml target);
//                    if (target != null)
//                    {
//                        target.VehicleAssetName = asset.name;
//                        descriptorsToExport.Add(target);
//                    }
//                }
//                if (descriptorsToExport.Count > 0)
//                {
//                    var exportableLayouts = new ExportableLayoutDescriptorVehicleXml
//                    {
//                        Descriptors = descriptorsToExport.ToArray()
//                    };
//                    File.WriteAllText(output, XmlUtils.DefaultXmlSerialize(exportableLayouts));

//                    K45DialogControl.ShowModal(new K45DialogControl.BindProperties
//                    {
//                        title = Locale.Get("K45_WTS_VEHICLE_EXPORTLAYOUT"),
//                        message = string.Format(Locale.Get("K45_WTS_VEHICLE_EXPORTLAYOUT_SUCCESSSAVEDATA"), output),
//                        showButton1 = true,
//                        textButton1 = Locale.Get("EXCEPTION_OK"),
//                        showButton2 = true,
//                        textButton2 = Locale.Get("K45_CMNS_GOTO_FILELOC"),
//                        useFullWindowWidth = true
//                    }, (x) =>
//                    {
//                        if (x == 2)
//                        {
//                            ColossalFramework.Utils.OpenInFileBrowser(output);
//                            return false;
//                        }
//                        return true;
//                    });

//                    ModInstance.Controller?.VehicleTextsSingleton?.LoadAllVehiclesConfigurations();
//                }
//            }
//        }
//        private void PasteFromClipboard()
//        {
//            if (m_currentSkin.IsNullOrWhiteSpace())
//            {
//                WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, XmlUtils.DefaultXmlDeserialize<LayoutDescriptorVehicleXml>(m_clipboard));
//            }
//            else
//            {
//                ModInstance.Controller.ConnectorVS.ApplySkin(m_currentInfo, m_currentSkin, m_clipboard);
//            }

//            OnChangeInfo(m_currentInfo, m_currentParentInfo, m_currentSkin);
//        }

//        private void CopyToClipboard() => m_clipboard = XmlUtils.DefaultXmlSerialize(m_currentLayout);
//        private void CloneToCity()
//        {
//            WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, XmlUtils.CloneViaXml(m_currentLayout));
//            OnChangeInfo(m_currentInfo, m_currentParentInfo);
//        }
//        private void CreateNew()
//        {
//            WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, new LayoutDescriptorVehicleXml());
//            OnChangeInfo(m_currentInfo, m_currentParentInfo);
//        }
//        private void DeleteFromCity()
//        {
//            WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, null);
//            OnChangeInfo(m_currentInfo, m_currentParentInfo);
//        }

//        private void ReloadFiles()
//        {
//            ModInstance.Controller?.VehicleTextsSingleton?.LoadAllVehiclesConfigurations();
//            OnChangeInfo(m_currentInfo, m_currentParentInfo);
//        }
//        private void GoToGlobalFolder() => ColossalFramework.Utils.OpenInFileBrowser(MainController.DefaultVehiclesConfigurationFolder);
//        #endregion

//        #region Search font

//        private readonly GUIFilterItemsScreen<State> m_fontFilter;
//        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
//        {
//            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
//            yield return 0;
//        }
//        private void OnSelectFont(int _, string fontName) => m_currentLayout.FontName = fontName;
//        #endregion
//    }

//}
