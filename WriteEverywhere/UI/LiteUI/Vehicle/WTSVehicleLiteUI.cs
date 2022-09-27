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
    internal class WTSVehicleLiteUI : IOpacityChangingGUI
    {
        public static WTSVehicleLiteUI Instance { get; private set; }
        public int TrailerSel { get; private set; } = 0;
        public ushort CurrentGrabbedId => m_detailUI.CurrentGrabbedId;
        public int CurrentTextSel => m_detailUI.TextDescriptorIndexSelected;
        public bool IsOnTextDimensionsView => m_detailUI.IsOnTextDimensionsView;
        public VehicleInfo CurrentEditingInfo => m_detailUI.CurrentEditingInfo;
        public string CurrentSkin => m_detailUI.CurrentSkin;

        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.WTS_VEHICLEEDITOR_WINDOWTITLE}", new Rect(128, 128, 500, 350), resizable: true, minSize: new Vector2(500, 500));
            m_modelFilter = new GUIFilterItemsScreen<State>(Str.WTS_VEHICLEEDITOR_SELECTMODEL, ModInstance.Controller, OnFilterParam, OnVehicleSet, GoTo, State.Normal, State.SelectVehicle, otherFilters: DrawExtraFilter, extraButtonsSearch: ExtraButtonsSearch);
            m_colorPicker = GameObjectUtils.CreateElement<GUIColorPicker>(transform).Init();
            m_colorPicker.Visible = false;
            m_detailUI = new WTSVehicleInfoDetailLiteUI(m_colorPicker);
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
            SelectVehicle
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
            if (GUILayout.Button(Str.we_vehicleEditor_pickerBtn, currentTool is VehicleEditorTool currentToolVeh ? GreenButton : GUI.skin.button, GUILayout.Width(100)))
            {
                var vehTool = ToolsModifierControl.toolController.GetComponent<VehicleEditorTool>();
                vehTool.OnVehicleSelect += (x) =>
                     {
                         var head = VehicleManager.instance.m_vehicles.m_buffer[x].GetFirstVehicle(x);
                         CurrentInfo = VehicleManager.instance.m_vehicles.m_buffer[head].Info;
                         m_currentState = State.Normal;
                         m_detailUI.CurrentGrabbedId = head;
                     };
                vehTool.OnParkedVehicleSelect += (x) =>
                {
                    CurrentInfo = VehicleManager.instance.m_parkedVehicles.m_buffer[x].Info;
                    m_currentState = State.Normal;
                };
                ToolsModifierControl.SetTool<VehicleEditorTool>();
            }
            return 0;
        }

        private WTSVehicleInfoDetailLiteUI m_detailUI;
        public GUIColorPicker m_colorPicker;

        public static Texture2D BgTextureSubgroup;
        public static Texture2D BgTextureNote;

        public static Color bgSubgroup;
        public static Color bgNote;

        static WTSVehicleLiteUI()
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

        internal void Reset()
        {
            m_currentState = State.Normal;
            TrailerSel = 0;
        }

        private State m_currentState = State.Normal;
        private void GoTo(State newState) => m_currentState = newState;

        public VehicleInfo CurrentInfo
        {
            get => m_currentInfo; set
            {
                if (m_currentInfo != value)
                {
                    m_currentInfo = value;
                    if (!(value is null))
                    {
                        m_currentInfoList = new List<VehicleInfo>()
                        {
                            m_currentInfo
                        };
                        if (!(m_currentInfo.m_trailers is null))
                        {
                            m_currentInfoList.AddRange(m_currentInfo.m_trailers.Select(x => x.m_info).Distinct().Where(x => x != m_currentInfo));
                        }
                        TrailerSel = 0;
                    }
                }
            }
        }

        protected override bool showOverModals { get; } = false;
        protected override bool requireModal { get; } = false;

        private List<VehicleInfo> m_currentInfoList;
        private VehicleInfo m_currentInfo;
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
                    case State.SelectVehicle:
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
                        TrailerSel = GUILayout.SelectionGrid(TrailerSel, m_currentInfoList.Select((_, i) => i == 0 ? Str.we_vehicleEditor_headVehicleTitle : string.Format(Str.we_vehicleEditor_trailerNumTitle, i)).ToArray(), m_currentInfoList.Count, GUILayout.MinWidth(40));
                        m_horizontalScroll = scope.scrollPosition;
                    }
                }
                using (new GUILayout.AreaScope(bodyArea, BgTextureSubgroup, GUI.skin.box))
                {
                    m_detailUI.DoDraw(new Rect(default, bodyArea.size), m_currentInfoList[TrailerSel], m_currentInfoList[0]);
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
            yield return VehiclesIndexes.instance.BasicInputFiltering(searchText, m_searchResultWrapper);
            m_cachedResultList.Clear();
            m_cachedResultList.AddRange(m_searchResultWrapper.Value.Cast<IndexedPrefabData<VehicleInfo>>()
                .Where(x => x.Prefab.m_placementStyle == ItemClass.Placement.Automatic &&
                (!m_searchOnlyWithActiveRules
                || WTSVehicleData.Instance.CityDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null)
                || WTSVehicleData.Instance.GlobalDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null)
                || WTSVehicleData.Instance.AssetsDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null))
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

        private void OnVehicleSet(int selectLayout, string _ = null)
        {
            CurrentInfo = (m_cachedResultList[selectLayout].Info as VehicleInfo);
            ToolsModifierControl.SetTool<DefaultTool>();
        }
        #endregion
    }
}
