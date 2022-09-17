using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using WriteEverywhere.Tools;
using WriteEverywhere.Xml;
using UnityEngine;

namespace WriteEverywhere.UI
{
    public class WTSOnNetTargetsTab : IGUITab<OnNetInstanceCacheContainerXml>
    {
        private const string f_base = "K45_WTS_OnNetInstanceCacheContainerXml_";
        private const string f_Targets = f_base + "Target";

        private Vector2 m_tabViewScroll;

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_MapIcon);
        public void Reset() => CurrentSegmentInSelect = -1;

        public bool DrawArea(Vector2 areaRect, ref OnNetInstanceCacheContainerXml item, int _)
        {
            using (var scroll = new GUILayout.ScrollViewScope(m_tabViewScroll))
            {
                areaRect.x -= 20;
                DrawTargetSegmentSelectionList(item, areaRect);
                m_tabViewScroll = scroll.scrollPosition;
            }
            return false;
        }

        private void DrawTargetSegmentSelectionList(OnNetInstanceCacheContainerXml item, Vector2 areaRect)
        {
            for (int i = 0; i <= 4; i++)
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(areaRect.x)))
                {
                    GUILayout.Label(string.Format(Locale.Get("K45_WTS_ONNETEDITOR_TARGET"), i), GUILayout.Width(areaRect.x / 4));
                    GUI.SetNextControlName(f_Targets + i);
                    if (GUILayout.Button(GetSegmentName(item, i)))
                    {
                        OnEnterPickTarget(item, i);
                    }
                };
            }
        }

        private string GetSegmentName(OnNetInstanceCacheContainerXml item, int i)
        {
            if (ModInstance.Controller.RoadSegmentToolInstance.enabled && CurrentSegmentInSelect == i)
            {
                return Locale.Get("ANIMAL_STATUS_WAITING");
            }
            var targSeg = item.GetTargetSegment(i);
            if (cachedSegmentNames[i] is null || targSeg != cachedSegmentNames[i].First)
            {
                if (targSeg == 0)
                {
                    cachedSegmentNames[i] = Tuple.New(targSeg, Locale.Get("K45_WTS_ONNETEDITOR_UNSETTARGETDESC"));
                }
                else
                {
                    var pos = NetManager.instance.m_segments.m_buffer[targSeg].m_middlePosition;
                    ModInstance.Controller.ConnectorADR.GetAddressStreetAndNumber(pos, pos, out int num, out string streetName);
                    cachedSegmentNames[i] = Tuple.New(targSeg, $"{((streetName?.Length ?? 0) == 0 ? NetManager.instance.m_segments.m_buffer[targSeg].Info.GetLocalizedTitle() : streetName)}, ~{num}m");
                }
            }
            return cachedSegmentNames[i].Second;
        }

        private void OnEnterPickTarget(OnNetInstanceCacheContainerXml item, int idx)
        {
            ToolsModifierControl.SetTool<DefaultTool>();
            ModInstance.Controller.RoadSegmentToolInstance.OnSelectSegment += (k) => item.SetTargetSegment(idx, k);
            CurrentSegmentInSelect = idx;
            ToolsModifierControl.SetTool<RoadSegmentTool>();
        }

        private int CurrentSegmentInSelect = -1;
        private Tuple<ushort, string>[] cachedSegmentNames = new Tuple<ushort, string>[5];
    }
}
