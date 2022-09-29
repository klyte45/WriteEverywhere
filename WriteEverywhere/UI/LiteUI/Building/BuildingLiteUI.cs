using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Tools;
using WriteEverywhere.Utils;

namespace WriteEverywhere.UI
{
    internal class BuildingLiteUI : GUIOpacityChanging
    {
        public static BuildingLiteUI Instance { get; private set; }
        public int SubBuildingSel { get; private set; } = 0;
        public ushort CurrentGrabbedId => m_detailUI.CurrentGrabbedId;
        public int CurrentPropSel => m_detailUI.PropSel;
        public int CurrentTextSel => m_detailUI.TextSel;
        public bool IsOnTextDimensionsView => m_detailUI.IsOnTextDimensionsView;
        public BuildingInfo CurrentEditingInfo => m_detailUI.CurrentEditingInfo;

        private BuildingInfoDetailLiteUI m_detailUI;
        private BuildingParamsTab m_paramsUI;
        public GUIColorPicker m_colorPicker;

        public static Texture2D BgTextureSubgroup;
        public static Texture2D BgTextureNote;

        public static Color bgSubgroup;
        public static Color bgNote;

        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.we_buildingEditor_windowTitle}", new Rect(128, 128, 500, 500), resizable: true, minSize: new Vector2(500, 500));
            m_modelFilter = new GUIFilterItemsScreen<State>(Str.we_buildingEditor_selectBuilding, ModInstance.Controller, OnFilterParam, OnBuildingSet, GoTo, State.Normal, State.SelectBuilding, otherFilters: DrawExtraFilter, extraButtonsSearch: ExtraButtonsSearch);
            m_colorPicker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_colorPicker.Visible = false;
            m_detailUI = new BuildingInfoDetailLiteUI(m_colorPicker);
            m_paramsUI = new BuildingParamsTab(() => CurrentGrabbedId, (x) => m_detailUI.CurrentGrabbedId = x, () => m_currentInfoList.Select(x =>
            {
                WTSBuildingPropsSingleton.GetTargetDescriptor(x.name, out _, out var desc);
                return desc;
            }), () => m_currentInfo);
        }
        protected override void OnOpacityChanged(float newVal)
        {
            base.OnOpacityChanged(newVal);
            BgTextureSubgroup.SetPixel(0, 0, new Color(bgSubgroup.r, bgSubgroup.g, bgSubgroup.b, ModInstance.Instance.UIOpacity / 4));
            BgTextureSubgroup.Apply();
            BgTextureNote.SetPixel(0, 0, new Color(bgNote.r, bgNote.g, bgNote.b, ModInstance.Instance.UIOpacity / 3));
            BgTextureNote.Apply();
        }
        private enum State
        {
            Normal,
            SelectBuilding
        }
        public static void Destroy()
        {
            Instance = null;
        }

        public void Start() => Visible = false;


        private float ExtraButtonsSearch(out bool hasChanged)
        {
            hasChanged = false;
            var currentTool = ToolsModifierControl.toolController.CurrentTool;
            if (GUILayout.Button(Str.we_buildingEditor_pickerBtn, currentTool is BuildingEditorTool currentToolVeh ? WEUIUtils.GreenButton : GUI.skin.button))
            {
                var vehTool = ToolsModifierControl.toolController.GetComponent<BuildingEditorTool>();
                vehTool.OnBuildingSelect += (x) =>
                     {
                         CurrentInfo = BuildingManager.instance.m_buildings.m_buffer[x].Info;
                         m_currentState = State.Normal;
                         m_detailUI.CurrentGrabbedId = x;
                     };
                ToolsModifierControl.SetTool<BuildingEditorTool>();
            }
            return 0;
        }

        static BuildingLiteUI()
        {
            bgSubgroup = ModInstance.Instance.ModColor.SetBrightness(.20f);

            BgTextureSubgroup = new Texture2D(1, 1);
            BgTextureSubgroup.SetPixel(0, 0, new Color(bgSubgroup.r, bgSubgroup.g, bgSubgroup.b, ModInstance.Instance.UIOpacity));
            BgTextureSubgroup.Apply();


            bgNote = ModInstance.Instance.ModColor.SetBrightness(.60f);
            BgTextureNote = new Texture2D(1, 1);
            BgTextureNote.SetPixel(0, 0, new Color(bgNote.r, bgNote.g, bgNote.b, ModInstance.Instance.UIOpacity));
            BgTextureNote.Apply();

        }

        public void Update()
        {
            if (Visible && Event.current.button == 1)
            {

            }
        }
        internal void Reset()
        {
            m_currentState = State.Normal;
            SubBuildingSel = 0;
        }

        private State m_currentState = State.Normal;
        private void GoTo(State newState) => m_currentState = newState;

        public BuildingInfo CurrentInfo
        {
            get => m_currentInfo; set
            {
                if (m_currentInfo != value)
                {
                    m_currentInfo = value;
                    if (!(value is null))
                    {
                        m_currentInfoListTitles = new[] { Str.we_buildingEditor_mainBuildingTitle }.Concat(m_currentInfo.m_subBuildings.Select((_, i) => string.Format(Str.we_buildingEditor_subBuildingNumTitle, i + 1))).ToArray();
                        m_currentInfoList = new[] { m_currentInfo }.Concat(m_currentInfo.m_subBuildings.Select((x, i) => x.m_buildingInfo)).GroupBy(x => x).Select(x => x.Key).ToArray();
                        SubBuildingSel = 0;
                    }
                }
            }
        }

        protected override bool showOverModals { get; } = false;
        protected override bool requireModal { get; } = false;
        public static bool LockSelection { get; internal set; } = true;
        public bool IsOnTextEditor => m_detailUI.IsOnTextEditor;

        private string[] m_currentInfoListTitles;
        private BuildingInfo[] m_currentInfoList;
        private BuildingInfo m_currentInfo;
        private Vector2 m_horizontalScrollSubBuildings;
        private Vector2 m_horizontalScrollGeneral;
        private int m_editorTypeSel;

        protected override void DrawWindow(Vector2 size)
        {
            var area = new Rect(5 * GUIWindow.ResolutionMultiplier, 0, size.x - 10 * GUIWindow.ResolutionMultiplier, size.y);
            using (new GUILayout.AreaScope(area))
            {
                switch (m_currentState)
                {
                    case State.Normal:
                        DrawNormal(area.size);
                        break;
                    case State.SelectBuilding:
                        m_modelFilter.DrawSelectorView(area.height);
                        break;
                }
            }
        }

        protected void DrawNormal(Vector2 size)
        {
            m_modelFilter.DrawButton(size.x, m_currentInfo?.GetUncheckedLocalizedTitle());
            var headerArea0 = new Rect(0, 25 * ResolutionMultiplier, size.x, 25 * ResolutionMultiplier);
            var headerArea1 = new Rect(0, 50 * ResolutionMultiplier, size.x, 45 * ResolutionMultiplier);
            var bodyArea = new Rect(0, 95 * ResolutionMultiplier, size.x, size.y - 95 * ResolutionMultiplier);
            if (CurrentInfo && m_detailUI.CurrentSource != Xml.ConfigurationSource.NONE)
            {
                using (new GUILayout.AreaScope(headerArea0))
                {
                    using (var scope = new GUILayout.ScrollViewScope(m_horizontalScrollGeneral))
                    {
                        m_editorTypeSel = GUILayout.SelectionGrid(m_editorTypeSel, new[] { Str.we_buildingEditor_layoutEditorTabText, Str.we_buildingEditor_paramEditorTabText }, 2, GUILayout.MinWidth(40 * GUIWindow.ResolutionMultiplier));
                        m_horizontalScrollGeneral = scope.scrollPosition;
                    }
                }
            }
            else
            {
                m_editorTypeSel = 0;
            }
            if (CurrentInfo)
            {
                using (new GUILayout.AreaScope(headerArea1))
                {
                    if (GUIKwyttoCommons.AddToggle(Str.we_buildingEditor_lockCameraToEditingProp, LockSelection, out var newval))
                    {
                        LockSelection = newval;
                    }
                    using (var scope = new GUILayout.ScrollViewScope(m_horizontalScrollSubBuildings))
                    {
                        SubBuildingSel = GUILayout.SelectionGrid(SubBuildingSel, m_currentInfoListTitles, m_currentInfoListTitles.Length, GUILayout.MinWidth(40 * GUIWindow.ResolutionMultiplier));
                        m_horizontalScrollSubBuildings = scope.scrollPosition;
                    }
                }
            }
            using (new GUILayout.AreaScope(bodyArea, BgTextureSubgroup, GUI.skin.box))
            {
                if (m_editorTypeSel == 0)
                {
                    DrawLayoutEditor(bodyArea.size);
                }
                else
                {
                    DrawParamEditor(bodyArea.size);
                }

            }
        }

        private void DrawLayoutEditor(Vector2 size)
        {
            var bodyArea = new Rect(0, 0, size.x, size.y);
            using (new GUILayout.AreaScope(bodyArea, BgTextureSubgroup, GUI.skin.box))
            {
                m_detailUI.DoDraw(new Rect(default, bodyArea.size), SubBuildingSel, m_currentInfo);
            }
        }
        private void DrawParamEditor(Vector2 size)
        {
            ushort parent = WTSBuildingPropsSingleton.FindParentBuilding(CurrentGrabbedId);
            if (!WTSBuildingData.Instance.Parameters.TryGetValue(parent, out var paramData))
            {
                WTSBuildingData.Instance.Parameters[parent] = paramData = new BuildingParametersData();
            }
            m_paramsUI.DrawArea(size, paramData, true);
        }
        protected override void OnWindowDestroyed() => Destroy(m_colorPicker);


        #region Model Select


        private GUIFilterItemsScreen<State> m_modelFilter;


        private readonly Wrapper<IIndexedPrefabData[]> m_searchResultWrapper = new Wrapper<IIndexedPrefabData[]>();
        private readonly List<IIndexedPrefabData> m_cachedResultList = new List<IIndexedPrefabData>();
        private bool m_searchOnlyWithActiveRules;
        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
        {
            m_detailUI.CurrentGrabbedId = 0;
            yield return BuildingIndexes.instance.BasicInputFiltering(searchText, m_searchResultWrapper);
            m_cachedResultList.Clear();
            m_cachedResultList.AddRange(m_searchResultWrapper.Value.Cast<IndexedPrefabData<BuildingInfo>>()
                .Where(x => x.Prefab.m_placementStyle == ItemClass.Placement.Automatic &&
                (!m_searchOnlyWithActiveRules
                || WTSBuildingData.Instance.CityDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_subBuildings?.Any(z => z.m_buildingInfo.name == y.Key) ?? false)) && y.Value != null)
                || WTSBuildingData.Instance.GlobalDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_subBuildings?.Any(z => z.m_buildingInfo.name == y.Key) ?? false)) && y.Value != null)
                || WTSBuildingData.Instance.AssetsDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_subBuildings?.Any(z => z.m_buildingInfo.name == y.Key) ?? false)) && y.Value != null))
                )
                .OrderBy(x => x.Info.GetUncheckedLocalizedTitle()).Take(500).Cast<IIndexedPrefabData>()); ;
            setResult(m_cachedResultList.Select(x => x.Info.GetUncheckedLocalizedTitle()).ToArray());
        }
        private float DrawExtraFilter(out bool hasChanged)
        {
            hasChanged = false;
            if (m_searchOnlyWithActiveRules != GUILayout.Toggle(m_searchOnlyWithActiveRules, Str.WTS_ACTIVERULESONLY))
            {
                m_searchOnlyWithActiveRules = !m_searchOnlyWithActiveRules;
                hasChanged = true;
            }
            return 12;
        }

        private void OnBuildingSet(int selectLayout, string _ = null)
        {
            CurrentInfo = (m_cachedResultList[selectLayout].Info as BuildingInfo);
            m_editorTypeSel = 0;
            ToolsModifierControl.SetTool<DefaultTool>();
        }
        #endregion
    }
}
