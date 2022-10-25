
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSVehicleInfoDetailLiteUI
    {
        private enum State
        {
            Normal,
            GeneralFontPicker
        }

        private VehicleInfo m_currentInfo;
        private VehicleInfo m_currentParentInfo;
        private string m_currentSkin;
        private string[] m_availableSkins;
        private string[] m_availableSkinsOptions;
        private ConfigurationSource m_currentSource;
        private LayoutDescriptorVehicleXml m_currentLayout;

        private readonly Texture2D m_grabModel;
        private readonly Texture2D m_grabModeWaiting;
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
        private readonly Texture2D m_save;
        private readonly Texture2D m_exportLib;
        private readonly GUIColorPicker m_colorPicker;
        private readonly GUIRootWindowBase m_root;

        private readonly GUIBasicListingTabsContainer<TextToWriteOnXml> m_tabsContainer;

        private string m_clipboard;
        private string[] m_cachedItemList;
        private float m_offsetYContent;
        private readonly GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml> m_vehicleLib = new GUIXmlLib<WTSLibVehicleLayout, LayoutDescriptorVehicleXml>()
        {
            DeleteQuestionI18n = Str.WTS_PROPEDIT_CONFIGDELETE_MESSAGE,
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE
        };

        private FooterBarStatus CurrentLibState => m_vehicleLib.Status;
        private State CurrentLocalState { get; set; } = State.Normal;

        public ushort CurrentGrabbedId { get; set; }
        public int TextDescriptorIndexSelected => m_tabsContainer.ListSel;
        public VehicleInfo CurrentEditingInfo => m_currentInfo;
        public bool IsOnTextDimensionsView => m_tabsContainer.CurrentTabIdx == m_sizeEditorTabIdx;
        public string CurrentSkin => m_availableSkinsOptions is null || m_availableSkinsOptions.Length == 0 || m_currentSkin == m_availableSkinsOptions[0] ? null : m_currentSkin;
        private readonly int m_sizeEditorTabIdx;

        public WTSVehicleInfoDetailLiteUI(GUIColorPicker colorPicker)
        {
            var viewAtlas = UIView.GetAView().defaultAtlas;

            m_grabModel = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Dropper);
            m_grabModeWaiting = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Lock);
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
            m_save = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Save);

            m_colorPicker = colorPicker;
            m_root = colorPicker.GetComponentInParent<GUIRootWindowBase>();

            m_fontFilter = new GUIFilterItemsScreen<State>(Str.WTS_OVERRIDE_FONT, ModInstance.Controller, OnFilterParam, OnSelectFont, GoTo, State.Normal, State.GeneralFontPicker, acceptsNull: true);


            var root = colorPicker.GetComponentInParent<GUIRootWindowBase>();
            GeneralWritingEditorPositionsSizesTab positionTab;
            var tabs = new IGUITab<TextToWriteOnXml>[]{
                    new GeneralWritingEditorGeneralTab(()=> m_currentLayout.TextDescriptors),
                    positionTab = new GeneralWritingEditorPositionsSizesTab(root),
                    new GeneralWritingEditorForegroundTab(m_colorPicker,TextRenderingClass.Vehicle),
                    new GeneralWritingEditorBgMeshSettingsTab(m_colorPicker, ()=> m_currentInfo,TextRenderingClass.Vehicle),
                    new GeneralWritingEditorFrameSettingsTab(m_colorPicker,()=>m_currentInfo,TextRenderingClass.Vehicle),
                    new GeneralWritingEditorIlluminationTab(m_colorPicker),
                    new GeneralWritingEditorContentTab(m_colorPicker, ()=> m_currentInfo, TextRenderingClass.Vehicle)
                    };
            m_tabsContainer = new GUIBasicListingTabsContainer<TextToWriteOnXml>(
                tabs,
                OnAddItem,
                GetList,
                GetCurrentItem, SetCurrentItem);
            m_sizeEditorTabIdx = Array.IndexOf(tabs, positionTab);
            m_tabsContainer.EventListItemChanged += OnTabChanged;
        }

        private TextToWriteOnXml GetCurrentItem(int arg) => m_currentLayout.TextDescriptors[arg] as TextToWriteOnXml;
        private string[] GetList() => m_cachedItemList;
        private void OnAddItem()
        {
            m_currentLayout.TextDescriptors = m_currentLayout.TextDescriptors.Concat(new[] { new TextToWriteOnXml() { SaveName = "NEW" } }).ToArray();
            m_cachedItemList = m_currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
        }

        private void SetCurrentItem(int arg, TextToWriteOnXml val)
        {
            if (val is null)
            {
                m_currentLayout.TextDescriptors = m_currentLayout.TextDescriptors.Where((x, i) => i != arg).ToArray();
                m_tabsContainer.Reset();
            }
            else
            {
                m_currentLayout.TextDescriptors[arg] = val;
            }
            m_cachedItemList = m_currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
        }

        public void DoDraw(Rect area, VehicleInfo vehicleInfo, VehicleInfo parentVehicle)
        {
            if (m_currentInfo != vehicleInfo)
            {
                OnChangeInfo(vehicleInfo, parentVehicle);
            }
            if (m_currentInfo is null)
            {
                return;
            }
            if (GUIKwyttoCommons.AddComboBox(area.width, Str.WTS_LAYOUTSKIN, ref m_currentSkin, m_availableSkinsOptions, m_availableSkins, m_root))
            {
                ReloadSkin();
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
                            m_fontFilter.DrawSelectorView(area.height - 20);
                            break;
                    }
                    break;
                case FooterBarStatus.AskingToImport:
                    m_vehicleLib.DrawImportView((x, _) =>
                    {
                        if (SceneUtils.IsAssetEditor)
                        {
                            WTSVehicleTextsSingleton.SetAssetDescriptor(CurrentEditingInfo, x);
                        }
                        else
                        {
                            WTSVehicleTextsSingleton.SetCityDescriptor(CurrentEditingInfo, x);
                        }
                    });
                    break;
            }

        }

        private void RegularDraw(Vector2 size)
        {
            using (new GUILayout.VerticalScope())
            {
                var isEditable = (SceneUtils.IsAssetEditor ? m_currentSource == ConfigurationSource.ASSET : m_currentSource == ConfigurationSource.CITY) || m_currentSource == ConfigurationSource.SKIN;
                using (new GUILayout.HorizontalScope(GUILayout.Height(70)))
                {
                    using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                    {
                        var skinNoWrap = new GUIStyle(GUI.skin.label) { wordWrap = false };
                        GUILayout.Label($"<color=#FFFF00>{Str.WTS_CURRENTSELECTION}</color>", skinNoWrap);
                        GUILayout.Label(m_currentInfo.GetUncheckedLocalizedTitle(), skinNoWrap);
                        GUILayout.Label($"<color=#FFFF00>{Str.WTS_CURRENTLY_USING}</color>", skinNoWrap);
                        GUILayout.Label(m_currentSource.ValueToI18n(), skinNoWrap);
                        GUILayout.FlexibleSpace();
                    }
                    using (new GUILayout.VerticalScope(GUILayout.MaxWidth(300 * GUIWindow.ResolutionMultiplier)))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            if (CurrentLibState == FooterBarStatus.Normal)
                            {
                                bool waitingGrab = ModInstance.Controller.VehicleTextsSingleton.WaitingGrab;
                                GUI.tooltip = "";
                                if (!SceneUtils.IsAssetEditor)
                                {
                                    GUILayout.FlexibleSpace();
                                    GUIKwyttoCommons.SquareTextureButton(waitingGrab ? m_grabModeWaiting : m_grabModel, waitingGrab ? Str.we_vehicleEditor_waitingGrabVehicle : Str.we_vehicleEditor_pickOrSpawnAVehicle, GrabUnit);
                                    GUILayout.FlexibleSpace();
                                    GUIKwyttoCommons.SquareTextureButton(m_goToFile, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_OPENGLOBALSFOLDER, GoToGlobalFolder);
                                }
                                GUIKwyttoCommons.SquareTextureButton(m_reloadFiles, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_RELOADDESCRIPTORS, ReloadFiles);
                                GUILayout.FlexibleSpace();
                                GUIKwyttoCommons.SquareTextureButton(m_createNew, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_NEWINCITY, CreateNew, !isEditable);
                                if (!SceneUtils.IsAssetEditor)
                                {
                                    GUIKwyttoCommons.SquareTextureButton(m_deleteFromCity, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_DELETEFROMCITY, DeleteFromCity, m_currentSource == ConfigurationSource.CITY);
                                    GUIKwyttoCommons.SquareTextureButton(m_cloneToCity, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCITY, CloneToCity, m_currentSource == ConfigurationSource.ASSET || m_currentSource == ConfigurationSource.GLOBAL);
                                }
                                else
                                {
                                    GUIKwyttoCommons.SquareTextureButton(m_deleteFromCity, Str.we_assetEditor_deleteFromAsset, DeleteFromAsset, m_currentSource == ConfigurationSource.ASSET);
                                }
                                GUILayout.FlexibleSpace();
                                if (SceneUtils.IsAssetEditor)
                                {
                                    GUIKwyttoCommons.SquareTextureButton(m_save, Str.we_assetEditor_saveDefaultSkin, ExportAsset, m_currentSource == ConfigurationSource.ASSET);
                                }
                                else
                                {
                                    GUIKwyttoCommons.SquareTextureButton(m_exportGlobal, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTASGLOBAL, ExportGlobal, m_currentSource == ConfigurationSource.CITY);
                                    GUIKwyttoCommons.SquareTextureButton(m_exportAsset, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_EXPORTTOASSETFOLDER, ExportAsset, m_currentSource == ConfigurationSource.CITY && m_currentInfo.name.EndsWith("_Data"));
                                }
                                GUIKwyttoCommons.SquareTextureButton(m_save, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_SAVESKIN, ExportSkin, m_currentSource == ConfigurationSource.SKIN);
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
                                m_vehicleLib.Draw(null, null, () => m_currentLayout);
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
                    m_fontFilter.DrawButton(size.x, m_currentLayout?.FontName);
                }
                else if (m_currentSource != ConfigurationSource.NONE)
                {
                    m_fontFilter.DrawButtonDisabled(size.x, m_currentLayout?.FontName);
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

        private void GrabUnit()
        {
            ModInstance.Controller.VehicleTextsSingleton.AskForGrab(m_currentInfo, (x) =>
            {
                if (x == default)
                {
                    KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                    {
                        buttons = KwyttoDialog.basicOkButtonBar,
                        message = Str.we_vehicleEditor_failedPickingAVehicle,
                        messageAlign = TextAnchor.MiddleCenter,
                        messageTextSizeMultiplier = 1.4f,
                    });
                    CurrentGrabbedId = 0;
                }
                else
                {
                    CurrentGrabbedId = VehicleManager.instance.m_vehicles.m_buffer[x].GetFirstVehicle(x);
                    ToolsModifierControl.cameraController.SetTarget(new InstanceID { Vehicle = x }, default, true);
                }
            });
        }

        private void OnChangeInfo(VehicleInfo vehicleInfo, VehicleInfo parentVehicle, string skin = "")
        {
            m_currentInfo = vehicleInfo;
            m_currentParentInfo = parentVehicle;
            if (m_currentInfo is null)
            {
                return;
            }
            m_currentSkin = skin;
            m_availableSkins = ModInstance.Controller.ConnectorVS.ListAllSkins(vehicleInfo);
            m_availableSkinsOptions = m_availableSkins.Select(x => x.IsNullOrWhiteSpace() ? "<DEFAULT>" : x).ToArray();
            ReloadSkin();
        }

        private void ReloadSkin()
        {
            WTSVehicleTextsSingleton.GetTargetDescriptor(m_currentInfo, -1, out m_currentSource, out var currentLayout, 0, m_currentSkin);
            m_currentLayout = currentLayout as LayoutDescriptorVehicleXml;
            m_cachedItemList = currentLayout?.TextDescriptors.Select(x => x.SaveName).ToArray();
            OnTabChanged(-1);
            m_tabsContainer.Reset();
            m_vehicleLib.ResetStatus();
        }

        private void OnTabChanged(int tabIdx)
        {

        }
        private void GoTo(State newState) => CurrentLocalState = newState;

        #region Top buttons
        private void ExportLayout() => m_vehicleLib.GoToExport();
        private void ImportLayout() => m_vehicleLib.GoToImport();
        private void ExportAsset() => ExportTo(AssetLayoutFileName());

        private string AssetLayoutFileName()
        {
            return Path.Combine(WTSVehicleTextsSingleton.GetDirectoryForAssetOwn(m_currentInfo), $"{WEMainController.m_defaultFileNameVehiclesXml}.xml");
        }

        private void ExportGlobal() => ExportTo(Path.Combine(WEMainController.DefaultVehiclesConfigurationFolder, $"{WEMainController.m_defaultFileNameVehiclesXml}_{PackageManager.FindAssetByName(m_currentParentInfo.name)?.package.packageMainAsset ?? m_currentParentInfo.name}.xml"));
        private void ExportSkin() => ModInstance.Controller.ConnectorVS.ApplySkin(m_currentInfo, m_currentSkin, XmlUtils.DefaultXmlSerialize(m_currentLayout));

        private void ExportTo(string output)
        {
            if (!(m_currentInfo is null))
            {
                KFileUtils.EnsureFolderCreation(Directory.GetParent(output).FullName);
                WTSVehicleTextsSingleton.GetTargetDescriptor(m_currentInfo, -1, out _, out ILayoutDescriptorVehicleXml target, 0);
                if (target is LayoutDescriptorVehicleXml layout)
                {
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
                    ModInstance.Controller?.VehicleTextsSingleton?.LoadAllVehiclesConfigurations();
                }
                m_currentInfo = null;
            }
        }
        private void PasteFromClipboard()
        {
            if (m_currentSkin.IsNullOrWhiteSpace())
            {
                if (SceneUtils.IsAssetEditor) WTSVehicleTextsSingleton.SetAssetDescriptor(m_currentInfo, XmlUtils.DefaultXmlDeserialize<LayoutDescriptorVehicleXml>(m_clipboard));
                else WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, XmlUtils.DefaultXmlDeserialize<LayoutDescriptorVehicleXml>(m_clipboard));
            }
            else
            {
                ModInstance.Controller.ConnectorVS.ApplySkin(m_currentInfo, m_currentSkin, m_clipboard);
            }

            OnChangeInfo(m_currentInfo, m_currentParentInfo, m_currentSkin);
        }

        private void CopyToClipboard() => m_clipboard = XmlUtils.DefaultXmlSerialize(m_currentLayout);
        private void CloneToCity()
        {
            WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, XmlUtils.CloneViaXml(m_currentLayout));
            OnChangeInfo(m_currentInfo, m_currentParentInfo);
        }
        private void CreateNew()
        {
            if (SceneUtils.IsAssetEditor)
            {
                WTSVehicleTextsSingleton.SetAssetDescriptor(m_currentInfo, new LayoutDescriptorVehicleXml());
            }
            else
            {
                WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, new LayoutDescriptorVehicleXml());
            }
            OnChangeInfo(m_currentInfo, m_currentParentInfo);
        }
        private void DeleteFromCity()
        {
            WTSVehicleTextsSingleton.SetCityDescriptor(m_currentInfo, null);
            OnChangeInfo(m_currentInfo, m_currentParentInfo);
        }
        private void DeleteFromAsset()
        {
            File.Delete(AssetLayoutFileName());
            WTSVehicleTextsSingleton.SetAssetDescriptor(m_currentInfo, null);
            OnChangeInfo(m_currentInfo, m_currentParentInfo);
        }

        private void ReloadFiles()
        {
            ModInstance.Controller?.VehicleTextsSingleton?.LoadAllVehiclesConfigurations();
            OnChangeInfo(m_currentInfo, m_currentParentInfo);
        }
        private void GoToGlobalFolder() => ColossalFramework.Utils.OpenInFileBrowser(WEMainController.DefaultVehiclesConfigurationFolder);
        #endregion

        #region Search font

        private readonly GUIFilterItemsScreen<State> m_fontFilter;
        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            setResult(FontServer.instance.GetAllFonts().Where(x => searchText.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x, searchText, CompareOptions.IgnoreCase) >= 0).OrderBy(x => x).ToArray());
            yield return 0;
        }
        private void OnSelectFont(int _, string fontName) => m_currentLayout.FontName = fontName;
        #endregion
    }

}
