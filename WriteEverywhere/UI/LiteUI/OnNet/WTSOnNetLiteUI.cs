using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSOnNetLiteUI : GUIRootWindowBase
    {
        public static WTSOnNetLiteUI Instance { get; private set; }
        private GUIBasicListingTabsContainer<OnNetInstanceCacheContainerXml> m_tabsContainer;

        private GUIStyle m_redButton;
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

        public void Awake()
        {
            Instance = this;
            Instance.Init("On Net Editor", new Rect(128, 128, 680, 420), resizable: true, minSize: new Vector2(340, 260));
            m_colorPicker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_colorPicker.Visible = false;
            Instance.m_tabsContainer = new GUIBasicListingTabsContainer<OnNetInstanceCacheContainerXml>(new IGUITab<OnNetInstanceCacheContainerXml>[] {
                new WTSOnNetBasicTab(Instance.OnImportSingle, Instance.OnDelete, this),
                new WTSOnNetTargetsTab(),
                new WTSOnNetParamsTab(),
                new WTSOnNetTextTab(m_colorPicker, () => CurrentEditingInstance?.BoardsData[ListSel]?.SimpleCachedProp, () =>
                {
                    if( CurrentEditingInstance?.BoardsData[ListSel] != null)
                    {
                        return ref CurrentEditingInstance.BoardsData[ListSel].RefTextDescriptors;
                    }
                    throw new System.Exception("Invalid call!!!");
                }, () =>
                {
                    if( CurrentEditingInstance?.BoardsData[ListSel] != null)
                    {
                        return ref CurrentEditingInstance.BoardsData[ListSel].RefFontName;
                    }
                    throw new System.Exception("Invalid call!!!");
                })
            }, Instance.OnAdd, Instance.GetSideList, Instance.GetSelectedItem, Instance.OnSetCurrentItem);
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
        private readonly GUIXmlLib<WTSLibOnNetPropLayoutList, ExportableBoardInstanceOnNetListXml> xmlLibList = new GUIXmlLib<WTSLibOnNetPropLayoutList, ExportableBoardInstanceOnNetListXml>()
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
            ModInstance.Controller.ConnectorADR.GetAddressStreetAndNumber(NetManager.instance.m_segments.m_buffer[CurrentSegmentId].m_middlePosition, NetManager.instance.m_segments.m_buffer[CurrentSegmentId].m_middlePosition, out int num, out string streetName);
            Title = $"{Str.WTS_SEGMENTPLACING_TITLE}: {streetName}, ~{num}m";
            if (WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId] == null)
            {
                WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId] = new WriteOnNetGroupXml();
            }
            CurrentEditingInstance = WTSOnNetData.Instance.m_boardsContainers[CurrentSegmentId];
            m_tabsContainer.Reset();
            xmlLibList.ResetStatus();
        }

        public int ListSel => m_tabsContainer.ListSel;

        protected override void DrawWindow()
        {
            if (currentSegmentId == 0)
            {
                Visible = false;
                return;
            }
            if (xmlLibList.Status != FooterBarStatus.AskingToImport)
            {
                RegularDraw();
            }
            else
            {
                using (new GUILayout.AreaScope(new Rect(5, 30, WindowRect.width - 10, WindowRect.height - 30)))
                {
                    xmlLibList.DrawImportView(OnSelectBoardList);
                }
            }
        }


        private void RegularDraw()
        {
            if (CurrentEditingInstance is null)
            {
                return;
            }
            var headerArea = new Rect(5, 24, WindowRect.width - 10, 20);
            if (xmlLibList.Status == FooterBarStatus.Normal)
            {
                using (new GUILayout.AreaScope(headerArea))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        LockSelection = GUILayout.Toggle(LockSelection, Str.WTS_SEGMENTEDITOR_BUTTONROWACTION_LOCKCAMERASELECTION);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(Str.WTS_SEGMENT_IMPORTDATA))
                        {
                            xmlLibList.GoToImport();
                        }
                        if (GUILayout.Button(Str.WTS_SEGMENT_EXPORTDATA))
                        {
                            xmlLibList.GoToExport();
                        }
                        if (GUILayout.Button(Str.WTS_SEGMENT_CLEARDATA))
                        {
                            xmlLibList.GoToRemove();
                        }

                    }
                }

            }
            else
            {
                xmlLibList.Draw(RedButton, OnDeleteList, OnGetCurrentList);
            }
            m_tabsContainer.DrawListTabs(new Rect(0, 44, WindowRect.width, WindowRect.height - 40));
        }

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

        private void OnSelectBoardList(ExportableBoardInstanceOnNetListXml obj)
        {
            CurrentEditingInstance.BoardsData = CurrentEditingInstance.BoardsData.Concat(obj.Instances.Select(x => XmlUtils.TransformViaXml<WriteOnNetXml, OnNetInstanceCacheContainerXml>(x)).Where(x => !(x?.SaveName is null))).ToArray();
            foreach (var x in obj.Layouts)
            {
                if (WTSPropLayoutData.Instance.Get(x.Key) is null)
                {
                    var value = XmlUtils.CloneViaXml(x.Value);
                    WTSPropLayoutData.Instance.Add(x.Key, value);
                }
            };
            m_tabsContainer.Reset();
        }
        private ExportableBoardInstanceOnNetListXml OnGetCurrentList() => new ExportableBoardInstanceOnNetListXml
        {
            Instances = CurrentEditingInstance.BoardsData.Select((x) => XmlUtils.DefaultXmlDeserialize<WriteOnNetXml>(XmlUtils.DefaultXmlSerialize(x))).ToArray(),
        };

        private void OnDeleteList()
        {
            CurrentEditingInstance.BoardsData = new OnNetInstanceCacheContainerXml[0];
            m_tabsContainer.Reset();
        }

        private void OnDelete()
        {
            CurrentEditingInstance.BoardsData = CurrentEditingInstance.BoardsData.Where((k, i) => i != m_tabsContainer.ListSel).ToArray();
            m_tabsContainer.Reset();
        }

        private void OnImportSingle(OnNetInstanceCacheContainerXml data)
        {
            data.SaveName = CurrentEditingInstance.BoardsData[m_tabsContainer.ListSel].SaveName;
            CurrentEditingInstance.BoardsData[m_tabsContainer.ListSel] = data;
        }
    }
}
