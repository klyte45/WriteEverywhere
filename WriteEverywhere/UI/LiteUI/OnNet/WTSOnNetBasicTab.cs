using ColossalFramework;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Libraries;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class WTSOnNetBasicTab : IGUITab<OnNetInstanceCacheContainerXml>
    {
        private const string f_base = "K45_WE_OnNetInstanceCacheContainerXml_";
        private const string f_SaveName = f_base + "SaveName";
        private const string f_ModelSelect = f_base + "ModelSelect";
        private const string f_ModelSelectType = f_base + "ModelSelectType";


        private const string f_InstanceMode = f_base + "InstanceMode";
        private const string f_SegmentPathSingle = f_base + "SegmentPathSingle";
        private const string f_SegmentPathStart = f_base + "SegmentPathStart";
        private const string f_SegmentPathEnd = f_base + "SegmentPathEnd";
        private const string f_SegmentRepeatCount = f_base + "SegmentRepeatCount";


        private const string f_SegmentPositionOffset = f_base + "SegmentPositionOffset";
        private const string f_SegmentRotationOffset = f_base + "SegmentRotationOffset";
        private const string f_SegmentScaleOffset = f_base + "SegmentScaleonOffset";


        private static readonly PivotPosition[] pivotOptionsValues = Enum.GetValues(typeof(PivotPosition)).Cast<PivotPosition>().ToArray();
        private static readonly string[] pivotOptions = pivotOptionsValues.Select(x => x.ValueToI18n()).ToArray();

        private enum State
        {
            Normal,
            GetLayout
        }

        private Vector2 m_tabViewScroll;
        private readonly Wrapper<IIndexedPrefabData[]> m_searchResultPrefab = new Wrapper<IIndexedPrefabData[]>();
        private readonly Wrapper<string[]> m_searchResultLayouts = new Wrapper<string[]>();
        private OnNetInstanceCacheContainerXml m_lastItem;
        private State m_currentState = State.Normal;
        private readonly Action<OnNetInstanceCacheContainerXml, bool> m_onImportFromLib;
        private readonly Action m_onDelete;
        private readonly GUIFilterItemsScreen<State> m_layoutFilter;
        private readonly GUIXmlLib<WTSLibOnNetPropLayout, WriteOnNetXml, OnNetInstanceCacheContainerXml> xmlLibItem = new GUIXmlLib<WTSLibOnNetPropLayout, WriteOnNetXml, OnNetInstanceCacheContainerXml>()
        {
            DeleteQuestionI18n = Str.WTS_PROPEDIT_CONFIGDELETE_MESSAGE,
            ImportI18n = Str.WTS_SEGMENT_IMPORTDATA,
            ExportI18n = Str.WTS_SEGMENT_EXPORTDATA,
            DeleteButtonI18n = Str.WTS_SEGMENT_REMOVEITEM,
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,

        };
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

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Settings);
        public static int LockSelectionInstanceNum { get => lockSelectionInstanceNum ?? -1; private set => lockSelectionInstanceNum = value; }

        private GUIRootWindowBase baseContainer;
        private static int? lockSelectionInstanceNum;

        public WTSOnNetBasicTab(Action<OnNetInstanceCacheContainerXml, bool> onImportFromLib, Action onDelete, GUIRootWindowBase baseContainer)
        {
            m_onImportFromLib = onImportFromLib;
            m_onDelete = onDelete;

            m_layoutFilter = new GUIFilterItemsScreen<State>(Str.WTS_BUILDINGEDITOR_MODELLAYOUTSELECT, ModInstance.Controller, OnFilterLayouts, OnModelSet, GoTo, State.Normal, State.GetLayout);
            this.baseContainer = baseContainer;
        }

        #region Layout Selection
        private void GoTo(State obj) => m_currentState = obj;

        private IEnumerator OnFilterLayouts(string input, Action<string[]> setOptions)
        {
            yield return PropIndexes.instance.BasicInputFiltering(input, m_searchResultPrefab);
            setOptions(m_searchResultPrefab.Value.Select(x => x.DisplayName).ToArray());

        }
        private void OnModelSet(int selectLayout, string _)
        {
            m_lastItem.SimpleProp = m_searchResultPrefab.Value[selectLayout]?.Info as PropInfo;
        }
        #endregion

        private void Reset(OnNetInstanceCacheContainerXml item)
        {
            m_lastItem = item;
            GoTo(State.Normal);
            xmlLibItem.ResetStatus();
        }
        public bool DrawArea(Vector2 areaRect, ref OnNetInstanceCacheContainerXml item, int _, bool _2)
        {
            if (item != m_lastItem)
            {
                Reset(item);
            }
            using (var scroll = new GUILayout.ScrollViewScope(m_tabViewScroll))
            {
                if (m_currentState == State.GetLayout)
                {
                    m_layoutFilter.DrawSelectorView(areaRect.y);
                }
                else if (xmlLibItem.Status == FooterBarStatus.AskingToImport)
                {
                    xmlLibItem.DrawImportView(m_onImportFromLib);
                }
                else
                {
                    DrawRegularTab(item, areaRect);
                }
                m_tabViewScroll = scroll.scrollPosition;
            }
            return false;
        }


        private void DrawRegularTab(OnNetInstanceCacheContainerXml item, Vector2 areaRect)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Str.WTS_ONNETEDITOR_NAME);
                var newName = GUITextField.TextField(f_SaveName, item.SaveName);
                if (!newName.IsNullOrWhiteSpace() && newName != item.SaveName)
                {
                    item.SaveName = newName;
                }
            };

            m_layoutFilter.DrawButton(areaRect.x, PropIndexes.GetListName(item.SimpleProp));

            using (new GUILayout.HorizontalScope())
            {
                GUI.SetNextControlName(f_InstanceMode);
                item.SegmentPositionRepeating = GUILayout.Toggle(item.SegmentPositionRepeating, Str.WTS_POSITIONINGMODE_ISMULTIPLE);
            };

            if (item.SegmentPositionRepeating)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Str.WTS_ONNETEDITOR_SEGMENTPOSITION_START);
                    GUILayout.Space(areaRect.x / 3);
                    var rect = GUILayoutUtility.GetLastRect();
                    item.SegmentPositionStart = GUI.HorizontalSlider(new Rect(rect.x, rect.yMin + 7, rect.width, 15), item.SegmentPositionStart, 0, 1);
                    item.SegmentPositionStart = GUIFloatField.FloatField(f_SegmentPathStart, item.SegmentPositionStart, 0, 1);
                };
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Str.WTS_ONNETEDITOR_SEGMENTPOSITION_END);
                    GUILayout.Space(areaRect.x / 3);
                    var rect = GUILayoutUtility.GetLastRect();
                    item.SegmentPositionEnd = GUI.HorizontalSlider(new Rect(rect.x, rect.yMin + 7, rect.width, 15), item.SegmentPositionEnd, 0, 1);
                    item.SegmentPositionEnd = GUIFloatField.FloatField(f_SegmentPathEnd, item.SegmentPositionEnd, 0, 1);
                };
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Str.WTS_ONNETEDITOR_SEGMENTPOSITION_COUNT);
                    item.SegmentPositionRepeatCount = (ushort)GUIIntField.IntField(f_SegmentRepeatCount, item.SegmentPositionRepeatCount, 1, ushort.MaxValue);
                };
                GUIKwyttoCommons.AddIntField(areaRect.x, Str.we_roadEditor_lockCameraAtInstance, ref lockSelectionInstanceNum, true, 0, item.SegmentPositionRepeatCount - 1);

            }
            else
            {
                lockSelectionInstanceNum = 0;
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Str.WTS_ONNETEDITOR_SEGMENTPOSITION);
                    GUILayout.Space(areaRect.x / 3);
                    var rect = GUILayoutUtility.GetLastRect();
                    item.SegmentPosition = GUI.HorizontalSlider(new Rect(rect.x, rect.yMin + 7, rect.width, 15), item.SegmentPosition, 0, 1);
                    item.SegmentPosition = GUIFloatField.FloatField(f_SegmentPathSingle, item.SegmentPosition, 0, 1);
                };
            }
            GUILayout.Space(12);


            GUIKwyttoCommons.AddComboBox(areaRect.x, Str.we_roadEditor_pivotPosition, item.PivotPosition, pivotOptions, pivotOptionsValues, baseContainer, (x) => item.PivotPosition = x);


            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Str.WTS_ONNETEDITOR_LOCATION_SETTINGS);
            };

            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.PropPosition, Str.WTS_ONNETEDITOR_POSITIONOFFSET, f_SegmentPositionOffset);
            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.PropRotation, Str.WTS_ONNETEDITOR_ROTATION, f_SegmentRotationOffset);
            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.Scale, Str.WTS_ONNETEDITOR_SCALE, f_SegmentScaleOffset);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCLIPBOARD))
                {
                    m_clipboard = XmlUtils.DefaultXmlSerialize(item);
                }
                if (m_clipboard != null)
                {
                    if (GUILayout.Button(Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_PASTEFROMCLIPBOARD))
                    {
                        m_onImportFromLib(XmlUtils.DefaultXmlDeserialize<OnNetInstanceCacheContainerXml>(m_clipboard), false);
                    }
                }
                else
                {
                    GUILayout.FlexibleSpace();
                }

            };

            GUILayout.FlexibleSpace();
            xmlLibItem.Draw(RedButton, m_onDelete, () => m_lastItem, xmlLibItem.FooterDraw);
        }

        private string m_clipboard;
        public void Reset()
        {
            xmlLibItem.ResetStatus();
            GoTo(State.Normal);
        }
    }
}
