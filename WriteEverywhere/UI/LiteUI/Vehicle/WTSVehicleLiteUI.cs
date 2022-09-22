//using ColossalFramework.Globalization;
//using Kwytto;
//using Kwytto.LiteUI;
//using Kwytto.Utils;
//using WriteEverywhere.Data;
//using WriteEverywhere.Tools;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace WriteEverywhere.UI
//{
//    internal class WTSVehicleLiteUI : GUIRootWindowBase
//    {
//        public static WTSVehicleLiteUI Instance { get; private set; }
     
//        public WTSVehicleLiteUI()
//           : base($"{WriteTheSignsMod.Instance.SimpleName} v{WriteTheSignsMod.FullVersion} - {Locale.Get("K45_WTS_VEHICLEEDITOR_WINDOWTITLE")}", new Rect(128, 128, 500, 350), resizable: true, minSize: new Vector2(500, 350))
//        {
//            Instance = this;

//            m_modelFilter = new GUIFilterItemsScreen<State>(Locale.Get("K45_WTS_VEHICLEEDITOR_SELECTMODEL"), WriteTheSignsMod.Controller, OnFilterParam, OnVehicleSet, GoTo, State.Normal, State.SelectVehicle, otherFilters: DrawExtraFilter, extraButtonsSearch: ExtraButtonsSearch);
//            m_colorPicker = KlyteMonoUtils.CreateElement<GUIColorPicker>(transform);
//            m_colorPicker.Visible = false;
//            m_detailUI = new WTSVehicleInfoDetailLiteUI(m_colorPicker);
//        }
//        private enum State
//        {
//            Normal,
//            SelectVehicle
//        }


//        private float ExtraButtonsSearch(out bool hasChanged)
//        {
//            hasChanged = false;
//            var currentTool = ToolsModifierControl.toolController.CurrentTool;
//            if (GUILayout.Button("Picker", currentTool is VehicleEditorTool ? GreenButton : GUI.skin.button, GUILayout.Width(100)))
//            {
//                VehicleEditorTool.instance.OnVehicleSelect += (x) =>
//                {
//                    var head = VehicleManager.instance.m_vehicles.m_buffer[x].GetFirstVehicle(x);
//                    CurrentInfo = VehicleManager.instance.m_vehicles.m_buffer[head].Info;
//                    m_currentState = State.Normal;
//                };
//                VehicleEditorTool.instance.OnParkedVehicleSelect += (x) =>
//                {
//                    CurrentInfo = VehicleManager.instance.m_parkedVehicles.m_buffer[x].Info;
//                    m_currentState = State.Normal;
//                };
//                ToolsModifierControl.SetTool<VehicleEditorTool>();
//            }
//            return 0;
//        }
//        public void Start() => Visible = false;

//        private readonly WTSVehicleInfoDetailLiteUI m_detailUI;
//        public readonly GUIColorPicker m_colorPicker;

//        public static Texture2D BgTextureSubgroup;
//        public static Texture2D BgTextureNote;

//        public static Color bgSubgroup;
//        public static Color bgNote;

//        public static Texture2D BgTextureBasic => BgTexture;

//        static WTSVehicleLiteUI()
//        {
//            bgSubgroup = CommonProperties.ModColor.SetBrightness(.20f);

//            BgTextureSubgroup = new Texture2D(1, 1);
//            BgTextureSubgroup.SetPixel(0, 0, new Color(bgSubgroup.r, bgSubgroup.g, bgSubgroup.b, 1));
//            BgTextureSubgroup.Apply();


//            bgNote = CommonProperties.ModColor.SetBrightness(.60f);
//            BgTextureNote = new Texture2D(1, 1);
//            BgTextureNote.SetPixel(0, 0, new Color(bgNote.r, bgNote.g, bgNote.b, 1));
//            BgTextureNote.Apply();

//        }

//        public void Update()
//        {
//            if (Visible && Event.current.button == 1)
//            {

//            }
//        }

//        protected override void OnWindowClosed()
//        {
//            base.OnWindowClosed();
//            WriteTheSignsMod.Controller.BridgeUUI.Close();
//        }


//        private GUIStyle m_greenButton;
//        private GUIStyle m_redButton;
//        internal GUIStyle GreenButton
//        {
//            get
//            {
//                if (m_greenButton is null)
//                {
//                    m_greenButton = new GUIStyle(Skin.button)
//                    {
//                        normal = new GUIStyleState()
//                        {
//                            background = GUIKlyteCommons.darkGreenTexture,
//                            textColor = Color.white
//                        },
//                        hover = new GUIStyleState()
//                        {
//                            background = GUIKlyteCommons.greenTexture,
//                            textColor = Color.black
//                        },
//                    };
//                }
//                return m_greenButton;
//            }
//        }



//        internal GUIStyle RedButton
//        {
//            get
//            {
//                if (m_redButton is null)
//                {
//                    m_redButton = new GUIStyle(Skin.button)
//                    {
//                        normal = new GUIStyleState()
//                        {
//                            background = GUIKlyteCommons.darkRedTexture,
//                            textColor = Color.white
//                        },
//                        hover = new GUIStyleState()
//                        {
//                            background = GUIKlyteCommons.redTexture,
//                            textColor = Color.white
//                        },
//                    };
//                }
//                return m_redButton;
//            }
//        }

//        internal void Reset()
//        {
//            m_currentState = State.Normal;
//            CurrentTab = 0;
//        }

//        private State m_currentState = State.Normal;
//        private void GoTo(State newState) => m_currentState = newState;

//        public int CurrentTab { get; private set; } = 0;
//        public VehicleInfo CurrentInfo
//        {
//            get => m_currentInfo; set
//            {
//                if (m_currentInfo != value)
//                {
//                    m_currentInfo = value;
//                    if (!(value is null))
//                    {
//                        m_currentInfoList = new List<VehicleInfo>()
//                        {
//                            m_currentInfo
//                        };
//                        if (!(m_currentInfo.m_trailers is null))
//                        {
//                            m_currentInfoList.AddRange(m_currentInfo.m_trailers.Select(x => x.m_info).Distinct().Where(x => x != m_currentInfo));
//                        }
//                        CurrentTab = 0;
//                    }
//                }
//            }
//        }

//        private List<VehicleInfo> m_currentInfoList;
//        private VehicleInfo m_currentInfo;
//        private Vector2 m_horizontalScroll;

//        protected override void DrawWindow()
//        {
//            var area = new Rect(5, 25, WindowRect.width - 10, WindowRect.height - 25);
//            using (new GUILayout.AreaScope(area))
//            {
//                switch (m_currentState)
//                {
//                    case State.Normal:
//                        DrawNormal(area.size);
//                        break;
//                    case State.SelectVehicle:
//                        m_modelFilter.DrawSelectorView(area.height);
//                        break;
//                }
//            }
//        }

//        protected void DrawNormal(Vector2 size)
//        {
//            m_modelFilter.DrawButton(size.x, m_currentInfo?.GetUncheckedLocalizedTitle());

//            if (CurrentInfo)
//            {
//                var headerArea = new Rect(0, 25, size.x, 25); ;
//                var bodyArea = new Rect(0, 50, size.x, size.y - 50);
//                using (new GUILayout.AreaScope(headerArea))
//                {
//                    using (var scope = new GUILayout.ScrollViewScope(m_horizontalScroll))
//                    {
//                        CurrentTab = GUILayout.SelectionGrid(CurrentTab, m_currentInfoList.Select((_, i) => i == 0 ? "Head" : $"Trailer {i}").ToArray(), m_currentInfoList.Count, GUILayout.MinWidth(40));
//                        m_horizontalScroll = scope.scrollPosition;
//                    }
//                }
//                using (new GUILayout.AreaScope(bodyArea, BgTextureSubgroup, GUI.skin.box))
//                {
//                    m_detailUI.DoDraw(new Rect(default, bodyArea.size), m_currentInfoList[CurrentTab], m_currentInfoList[0]);
//                }


//            }
//        }
//        protected override void OnWindowDestroyed() => Destroy(m_colorPicker);


//        #region Model Select


//        private readonly GUIFilterItemsScreen<State> m_modelFilter;


//        private readonly Wrapper<IIndexedPrefabData[]> m_searchResultWrapper = new Wrapper<IIndexedPrefabData[]>();
//        private readonly List<IIndexedPrefabData> m_cachedResultList = new List<IIndexedPrefabData>();
//        private bool m_searchOnlyWithActiveRules;
//        private IEnumerator OnFilterParam(string searchText, Action<string[]> setResult)
//        {
//            yield return VehiclesIndexes.instance.BasicInputFilteringDetailed(searchText, m_searchResultWrapper);
//            m_cachedResultList.Clear();
//            m_cachedResultList.AddRange(m_searchResultWrapper.Value.Cast<IndexedPrefabData<VehicleInfo>>()
//                .Where(x => x.Prefab.m_placementStyle == ItemClass.Placement.Automatic &&
//                (!m_searchOnlyWithActiveRules
//                || WTSVehicleData.Instance.CityDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null)
//                || WTSVehicleData.Instance.GlobalDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null)
//                || WTSVehicleData.Instance.AssetsDescriptors.Any(y => (y.Key == x.PrefabName || (x.Prefab.m_trailers?.Any(z => z.m_info.name == y.Key) ?? false)) && y.Value != null))
//                )
//                .OrderBy(x => x.Info.GetUncheckedLocalizedTitle()).Take(500).Cast<IIndexedPrefabData>()); ;
//            setResult(m_cachedResultList.Select(x => x.Info.GetUncheckedLocalizedTitle()).ToArray());
//        }
//        private float DrawExtraFilter(out bool hasChanged)
//        {
//            hasChanged = false;
//            if (m_searchOnlyWithActiveRules != GUILayout.Toggle(m_searchOnlyWithActiveRules, Locale.Get("K45_WTS_ACTIVERULESONLY")))
//            {
//                m_searchOnlyWithActiveRules = !m_searchOnlyWithActiveRules;
//                hasChanged = true;
//            }
//            return 12;
//        }

//        private void OnVehicleSet(int selectLayout, string _ = null) => CurrentInfo = (m_cachedResultList[selectLayout].Info as VehicleInfo);
//        #endregion
//    }
//}
