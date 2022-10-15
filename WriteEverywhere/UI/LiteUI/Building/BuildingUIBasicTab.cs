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
    public class BuildingUIBasicTab : IGUITab<WriteOnBuildingPropXml>
    {
        private const string f_base = "K45_WE_WriteOnBuildingPropXml_";
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
        public static int CurrentFocusInstance { get => m_currentFocusInstance; private set => m_currentFocusInstance = value; }

        private enum State
        {
            Normal,
            GetLayout
        }

        private Vector2 m_tabViewScroll;
        private readonly Wrapper<IIndexedPrefabData[]> m_searchResultPrefab = new Wrapper<IIndexedPrefabData[]>();
        private readonly Wrapper<string[]> m_searchResultLayouts = new Wrapper<string[]>();
        private WriteOnBuildingPropXml m_lastItem;
        private State m_currentState = State.Normal;
        private readonly Action<WriteOnBuildingPropXml, bool> m_onImport;
        private readonly Action m_onDelete;
        private readonly GUIFilterItemsScreen<State> m_layoutFilter;
        private readonly GUIXmlLib<WTSLibOnBuildingPropLayout, WriteOnBuildingPropXml> xmlLibItem = new GUIXmlLib<WTSLibOnBuildingPropLayout, WriteOnBuildingPropXml>()
        {
            DeleteQuestionI18n = Str.WTS_PROPEDIT_CONFIGDELETE_MESSAGE,
            ImportI18n = Str.WTS_SEGMENT_IMPORTDATA,
            ExportI18n = Str.WTS_SEGMENT_EXPORTDATA,
            DeleteButtonI18n = Str.WTS_SEGMENT_REMOVEITEM,
            NameAskingI18n = Str.WTS_EXPORTDATA_NAMEASKING,
            NameAskingOverwriteI18n = Str.WTS_EXPORTDATA_NAMEASKING_OVERWRITE,
        };

        private readonly Texture2D m_deleteItem;
        private readonly Texture2D m_copy;
        private readonly Texture2D m_paste;
        private readonly Texture2D m_importLib;
        private readonly Texture2D m_exportLib;
        private string m_clipboard;

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Settings);

        private GUIRootWindowBase baseContainer;
        private static int m_currentFocusInstance;

        public BuildingUIBasicTab(Action<WriteOnBuildingPropXml, bool> onImport, Action onDelete, GUIRootWindowBase baseContainer)
        {
            m_deleteItem = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Delete);
            m_copy = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Copy);
            m_paste = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Paste);
            m_importLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Import);
            m_exportLib = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Export);
            m_onImport = onImport;
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

        private void Reset(WriteOnBuildingPropXml item)
        {
            m_lastItem = item;
            GoTo(State.Normal);
            xmlLibItem.ResetStatus();
        }
        public bool DrawArea(Vector2 areaRect, ref WriteOnBuildingPropXml item, int _, bool canEdit)
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
                    xmlLibItem.DrawImportView(m_onImport);
                }
                else
                {
                    DrawRegularTab(item, areaRect, canEdit);
                }
                m_tabViewScroll = scroll.scrollPosition;
            }
            return false;
        }


        private void DrawRegularTab(WriteOnBuildingPropXml item, Vector2 areaRect, bool canEdit)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Str.WTS_ONNETEDITOR_LOCATION_SETTINGS);
            };
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
            GUILayout.Space(12);

            GUIKwyttoCommons.AddIntField(areaRect.x, Str.we_buildingEditor_repeatLayoutTimes, item.ArrayRepeatTimes, (x) => item.ArrayRepeatTimes = (int)x, canEdit, 1);
            if (item.ArrayRepeatTimes > 1)
            {
                GUIKwyttoCommons.AddVector3Field(areaRect.x, item.ArrayRepeat, Str.we_buildingEditor_repeatLayoutDirection, "ArrayDir", canEdit);
                GUIKwyttoCommons.AddIntField(areaRect.x, Str.we_buildingEditor_currentFocusInstance, m_currentFocusInstance, (x) => m_currentFocusInstance = x.Value, true, 0, item.ArrayRepeatTimes - 1);
            }
            else
            {
                m_currentFocusInstance = 0;
            }

            GUILayout.Space(12);

            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.PropPosition, Str.WTS_ONNETEDITOR_POSITIONOFFSET, f_SegmentPositionOffset, canEdit);
            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.PropRotation, Str.WTS_ONNETEDITOR_ROTATION, f_SegmentRotationOffset, canEdit);
            GUIKwyttoCommons.AddVector3Field(areaRect.x, item.Scale, Str.WTS_ONNETEDITOR_SCALE, f_SegmentScaleOffset, canEdit);
            using (new GUILayout.HorizontalScope())
            {
                if (xmlLibItem.Status == FooterBarStatus.Normal)
                {
                    GUI.tooltip = "";
                    GUILayout.FlexibleSpace();
                    GUIKwyttoCommons.SquareTextureButton(m_deleteItem, Str.WTS_DELETETEXTITEM, m_onDelete, canEdit);
                    GUILayout.FlexibleSpace();
                    GUIKwyttoCommons.SquareTextureButton(m_copy, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_COPYTOCLIPBOARD, () => CopyToClipboard(item));
                    GUIKwyttoCommons.SquareTextureButton(m_paste, Str.WTS_BUILDINGEDITOR_BUTTONROWACTION_PASTEFROMCLIPBOARD, () => m_onImport(XmlUtils.DefaultXmlDeserialize<WriteOnBuildingPropXml>(m_clipboard), false), canEdit && !(m_clipboard is null));
                    GUILayout.FlexibleSpace();
                    GUIKwyttoCommons.SquareTextureButton(m_importLib, Str.WTS_IMPORTLAYOUT_LIB, xmlLibItem.GoToImport, canEdit); ;
                    GUIKwyttoCommons.SquareTextureButton(m_exportLib, Str.WTS_EXPORTLAYOUT_LIB, xmlLibItem.GoToExport);
                }
                else
                {
                    xmlLibItem.Draw(null, null, () => item);
                }
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"<i>{GUI.tooltip}</i>", new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    alignment = TextAnchor.MiddleRight
                }, GUILayout.Height(40));
                GUI.tooltip = "";
            }
            GUILayout.Space(12);
            GUILayout.FlexibleSpace();

        }

        public void Reset()
        {
            xmlLibItem.ResetStatus();
            GoTo(State.Normal);
        }

        private void CopyToClipboard(WriteOnBuildingPropXml item) => m_clipboard = XmlUtils.DefaultXmlSerialize(item);
    }
}
