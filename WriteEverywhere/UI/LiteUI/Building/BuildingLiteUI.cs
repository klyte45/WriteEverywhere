using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Tools;

namespace WriteEverywhere.UI
{
    internal class BuildingLiteUI : IOpacityChangingGUI
    {
        public static BuildingLiteUI Instance { get; private set; }
        public int SubBuildingSel { get; private set; } = 0;
        public ushort CurrentGrabbedId => m_detailUI.CurrentGrabbedId;
        public int CurrentTextSel => m_detailUI.TextDescriptorIndexSelected;
        public bool IsOnTextDimensionsView => m_detailUI.IsOnTextDimensionsView;
        public BuildingInfo CurrentEditingInfo => m_detailUI.CurrentEditingInfo;

        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.we_buildingEditor_windowTitle}", new Rect(128, 128, 500, 500), resizable: true, minSize: new Vector2(500, 500));
            m_modelFilter = new GUIFilterItemsScreen<State>(Str.we_buildingEditor_selectBuilding, ModInstance.Controller, OnFilterParam, OnBuildingSet, GoTo, State.Normal, State.SelectBuilding, otherFilters: DrawExtraFilter, extraButtonsSearch: ExtraButtonsSearch);
            m_colorPicker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_colorPicker.Visible = false;
            m_detailUI = new BuildingInfoDetailLiteUI(m_colorPicker);
        }
        protected override void OnOpacityChanged(float newVal)
        {
            base.OnOpacityChanged(newVal);
            BgTextureSubgroup.SetPixel(0, 0, new Color(bgSubgroup.r, bgSubgroup.g, bgSubgroup.b, ModInstance.UIOpacitySaved));
            BgTextureSubgroup.Apply();
            BgTextureNote.SetPixel(0, 0, new Color(bgNote.r, bgNote.g, bgNote.b, ModInstance.UIOpacitySaved));
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
            if (GUILayout.Button(Str.we_buildingEditor_pickerBtn, currentTool is BuildingEditorTool currentToolVeh ? GreenButton : GUI.skin.button, GUILayout.Width(100)))
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

        private BuildingInfoDetailLiteUI m_detailUI;
        public GUIColorPicker m_colorPicker;

        public static Texture2D BgTextureSubgroup;
        public static Texture2D BgTextureNote;

        public static Color bgSubgroup;
        public static Color bgNote;

        static BuildingLiteUI()
        {
            bgSubgroup = ModInstance.Instance.ModColor.SetBrightness(.20f);

            BgTextureSubgroup = new Texture2D(1, 1);
            BgTextureSubgroup.SetPixel(0, 0, new Color(bgSubgroup.r, bgSubgroup.g, bgSubgroup.b, ModInstance.UIOpacitySaved));
            BgTextureSubgroup.Apply();


            bgNote = ModInstance.Instance.ModColor.SetBrightness(.60f);
            BgTextureNote = new Texture2D(1, 1);
            BgTextureNote.SetPixel(0, 0, new Color(bgNote.r, bgNote.g, bgNote.b, ModInstance.UIOpacitySaved));
            BgTextureNote.Apply();

        }

        public void Update()
        {
            if (Visible && Event.current.button == 1)
            {

            }
        }

        private GUIStyle m_greenButton;
        private GUIStyle m_redButton;
        internal GUIStyle GreenButton
        {
            get
            {
                if (m_greenButton is null)
                {
                    m_greenButton = new GUIStyle(Skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkGreenTexture,
                            textColor = Color.white
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.greenTexture,
                            textColor = Color.black
                        },
                    };
                }
                return m_greenButton;
            }
        }



        internal GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(Skin.button)
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
                        m_currentInfoList = new[] { Str.we_buildingEditor_mainBuildingTitle }.Concat(m_currentInfo.m_subBuildings.Select((_, i) => string.Format(Str.we_buildingEditor_subBuildingNumTitle, i + 1))).ToArray();
                        SubBuildingSel = 0;
                    }
                }
            }
        }

        protected override bool showOverModals { get; } = false;
        protected override bool requireModal { get; } = false;

        private string[] m_currentInfoList;
        private BuildingInfo m_currentInfo;
        private Vector2 m_horizontalScroll;

        protected override void DrawWindow()
        {
            var area = new Rect(5, 25, WindowRect.width - 10, WindowRect.height - 25);
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

            if (CurrentInfo)
            {
                var headerArea = new Rect(0, 25, size.x, 25); ;
                var bodyArea = new Rect(0, 50, size.x, size.y - 50);
                using (new GUILayout.AreaScope(headerArea))
                {
                    using (var scope = new GUILayout.ScrollViewScope(m_horizontalScroll))
                    {
                        SubBuildingSel = GUILayout.SelectionGrid(SubBuildingSel, m_currentInfoList, m_currentInfoList.Length, GUILayout.MinWidth(40));
                        m_horizontalScroll = scope.scrollPosition;
                    }
                }
                using (new GUILayout.AreaScope(bodyArea, BgTextureSubgroup, GUI.skin.box))
                {
                    m_detailUI.DoDraw(new Rect(default, bodyArea.size), SubBuildingSel, m_currentInfo);
                }


            }
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
            ToolsModifierControl.SetTool<DefaultTool>();
        }
        #endregion
    }
}
