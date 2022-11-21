using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Layout;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;

namespace WriteEverywhere.UI
{
    internal class WTSOnNetLiteUI : GUIOpacityChanging
    {
        public static WTSOnNetLiteUI Instance { get; private set; }
        private GUIBasicListingTabsContainer<OnNetInstanceCacheContainerXml> m_tabsContainer;
        private WTSOnNetTextTab m_textEditorTab;
        private int m_textEditorTabIdx;

        private GUIStyle m_redButton;
        protected override float FontSizeMultiplier => .9f;
        private GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(GUI.skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkRedTexture,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.redTexture,
                            textColor = Color.white
                        },
                    };
                }
                return m_redButton;
            }
        }

        public override void Awake()
        {
            Instance = this;
            base.Awake();
            Init($"{ModInstance.Instance.GeneralName} - {Str.we_roadEditor_windowTitle}", new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(340, 260));
            m_colorPicker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_colorPicker.Visible = false;
            var tabs = new IGUITab<OnNetInstanceCacheContainerXml>[] {
                new WTSOnNetBasicTab(OnImportSingle, OnDelete, this),
                new WTSOnNetTargetsTab(),
                new WTSOnNetParamsTab(),
                m_textEditorTab =  new WTSOnNetTextTab(m_colorPicker, () => CurrentEditingInstance?.BoardsData[ListSel]?.SimpleCachedProp,
                () =>
                {
                    if(ListSel>=0 && CurrentEditingInstance?.BoardsData[ListSel] != null)
                    {
                        return ref CurrentEditingInstance.BoardsData[ListSel].RefTextDescriptors;
                    }
                    throw new System.Exception("Invalid call!!!");
                }, () =>
                {
                    if(ListSel>=0 && CurrentEditingInstance?.BoardsData[ListSel] != null)
                    {
                        return ref CurrentEditingInstance.BoardsData[ListSel].RefFontName;
                    }
                    throw new System.Exception("Invalid call!!!");
                }, ()=>ListSel>=0)
            };
            m_tabsContainer = new GUIBasicListingTabsContainer<OnNetInstanceCacheContainerXml>(tabs, OnAdd, GetSideList, GetSelectedItem, OnSetCurrentItem, AddExtraButtonsList);
            m_textEditorTabIdx = Array.IndexOf(tabs, m_textEditorTab);
        }
        public static void Destroy()
        {
            Instance = null;
        }

        public void Start() => Visible = false;

        public void Update()
        {
            if (Visible && Event.current.button == 1)
            {
                ToolsModifierControl.SetTool<SegmentEditorPickerTool>();
            }
        }

        private ushort currentSegmentId;
        private GUIColorPicker m_colorPicker;
        private readonly GUIXmlFolderLib<ExportableBoardInstanceOnNetListXml> xmlLibList = new GUIOnNetPropListLib
        {
            DeleteQuestionI18n = Str.WTS_SEGMENT_CLEARDATA_AYS,
            ImportI18n = Str.WTS_SEGMENT_IMPORTDATA,
            ExportI18n = Str.WTS_SEGMENT_EXPORTDATA,
            DeleteButtonI18n = Str.WTS_SEGMENT_REMOVEITEM,
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,

        };

        public static bool LockSelection { get; internal set; } = true;
        public static int LockSelectionInstanceNum => WTSOnNetBasicTab.LockSelectionInstanceNum;
        public static int LockSelectionTextIdx => Instance.m_textEditorTab.SelectedTextItem;
        private WriteOnNetGroupXml CurrentEditingInstance { get; set; }
        public ushort CurrentSegmentId
        {
            get => currentSegmentId; set
            {
                currentSegmentId = value;
                Visible = value != 0;
                if (value != 0)
                {
                    ReloadSegment();
                }
            }
        }
        private void ReloadSegment()
        {
            ModInstance.Controller.ConnectorCD.GetAddressStreetAndNumber(NetManager.instance.m_segments.m_buffer[CurrentSegmentId].m_middlePosition, NetManager.instance.m_segments.m_buffer[CurrentSegmentId].m_middlePosition, out int num, out string streetName);
            Title = $"{Str.WTS_SEGMENTPLACING_TITLE}: {streetName}, ~{num}m";
            if (WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId] == null)
            {
                WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId] = new WriteOnNetGroupXml();
            }
            CurrentEditingInstance = WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId];
            m_tabsContainer.Reset();
            xmlLibList.ResetStatus();
            m_textEditorTab.Reset();
        }

        public int ListSel => m_tabsContainer.ListSel;
        public int CurrentTabIdx => m_tabsContainer.CurrentTabIdx;

        public bool IsOnTextEditor => CurrentTabIdx == m_textEditorTabIdx;
        public bool IsOnTextEditorSizeView => IsOnTextEditor && m_textEditorTab.IsOnTextDimensionsView;

        protected override bool showOverModals => false;

        protected override bool requireModal => false;

        protected override void DrawWindow(Vector2 size)
        {
            if (currentSegmentId == 0)
            {
                Visible = false;
                return;
            }
            if (xmlLibList.Status != FooterBarStatus.AskingToImport && xmlLibList.Status != FooterBarStatus.AskingToImportAdditive)
            {
                RegularDraw(size);
            }
            else
            {
                using (new GUILayout.AreaScope(new Rect(5, 5, size.x - 10, size.y - 10)))
                {
                    xmlLibList.DrawImportView(OnSelectBoardList);
                }
            }
        }

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


        private void RegularDraw(Vector2 size)
        {
            if (CurrentEditingInstance is null)
            {
                return;
            }
            var headerArea = new Rect(5, 0, size.x - 10, 20);
            if (xmlLibList.Status == FooterBarStatus.Normal)
            {
                using (new GUILayout.AreaScope(headerArea))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        LockSelection = GUILayout.Toggle(LockSelection, Str.WTS_SEGMENTEDITOR_BUTTONROWACTION_LOCKCAMERASELECTION);
                    }
                }

            }
            else
            {
                xmlLibList.Draw(RedButton, OnDeleteList, OnGetCurrentList);
            }
            m_tabsContainer.DrawListTabs(new Rect(0, 20, size.x, size.y - 20));
        }

        #region Tab Actions
        private OnNetInstanceCacheContainerXml GetSelectedItem(int listSel) => CurrentEditingInstance.BoardsData[listSel];
        private string[] GetSideList() => CurrentEditingInstance.BoardsData.Select((y, i) => y.SaveName).ToArray();
        private void OnAdd() => CurrentEditingInstance.BoardsData = CurrentEditingInstance.BoardsData.Concat(new[] { new OnNetInstanceCacheContainerXml() { SaveName = "NEW" } }).ToArray();
        private void OnSetCurrentItem(int listSel, OnNetInstanceCacheContainerXml newVal)
        {
            if (newVal is null)
            {
                OnDelete();
            }
            else
            {
                CurrentEditingInstance.BoardsData[listSel] = newVal;
            }
        }

        private void OnSelectBoardList(ExportableBoardInstanceOnNetListXml obj, bool additive)
        {
            var arrImport = obj.Instances.Select(x => XmlUtils.TransformViaXml<WriteOnNetXml, OnNetInstanceCacheContainerXml>(x)).Where(x => !(x?.SaveName is null));
            CurrentEditingInstance.BoardsData = additive ? CurrentEditingInstance.BoardsData.Concat(arrImport).ToArray() : arrImport.ToArray();
            ReloadSegment();
        }
        private ExportableBoardInstanceOnNetListXml OnGetCurrentList() => new ExportableBoardInstanceOnNetListXml
        {
            Instances = CurrentEditingInstance.BoardsData.Select((x) => XmlUtils.DefaultXmlDeserialize<WriteOnNetXml>(XmlUtils.DefaultXmlSerialize(x))).ToArray(),
        };

        private void OnDeleteList()
        {
            CurrentEditingInstance.BoardsData = new OnNetInstanceCacheContainerXml[0];
            ReloadSegment();
        }

        private void OnDelete()
        {
            CurrentEditingInstance.BoardsData = CurrentEditingInstance.BoardsData.Where((k, i) => i != m_tabsContainer.ListSel).ToArray();
            ReloadSegment();
        }

        private void OnImportSingle(WriteOnNetXml data, bool _)
        {
            data.SaveName = CurrentEditingInstance.BoardsData[m_tabsContainer.ListSel].SaveName;
            CurrentEditingInstance.BoardsData[m_tabsContainer.ListSel] = XmlUtils.TransformViaXml<WriteOnNetXml, OnNetInstanceCacheContainerXml>(data);
            var oldSel = m_tabsContainer.ListSel;
            ReloadSegment();
            m_tabsContainer.ListSel = oldSel;
        }
        #endregion
    }
}
