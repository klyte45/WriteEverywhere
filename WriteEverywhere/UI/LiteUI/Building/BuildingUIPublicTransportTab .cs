using HarmonyLib;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;

namespace WriteEverywhere.UI
{
    internal class BuildingUIPublicTransportTab : IGUITab<WriteOnBuildingPropXml>
    {
        public BuildingUIPublicTransportTab(Func<BuildingInfo> currentInfoGetter)
        {
            m_currentInfoGetter = currentInfoGetter;
        }

        readonly Func<BuildingInfo> m_currentInfoGetter;
        Vector2 m_scrollPos;

        public static int LockSelectionInstanceNum { get; internal set; }

        public Texture TabIcon => KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Bus);

        public bool DrawArea(Vector2 tabAreaSize, ref WriteOnBuildingPropXml currentItem, int currentItemIdx, bool isEditable)
        {
            var info = m_currentInfoGetter();
            var changed = false;
            if (info.m_buildingAI is TransportStationAI)
            {
                var stops = WTSBuildingPropsSingleton.GetStopPointsDescriptorFor(info.name);
                if (stops.Length == 0)
                {
                    GUILayout.Label(Str.we_buildingEditor_thisStationHasNoStops);
                    return changed;
                }
                var selectedPlats = currentItem.m_platforms;
                var currentItemLoc = currentItem;
                GUILayout.Label($"<i>{Str.we_buildingEditor_transportStationSettings}</i>");
                GUILayout.Label(Str.we_buildingEditor_transportStation_platformSelectionInfo);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Str.we_buildingEditor_platformNameClickToToggle);
                    GUILayout.Label(Str.we_buildingEditor_changePriorityOrder, GUILayout.Width(60 * GUIWindow.ResolutionMultiplier));
                }
                using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
                {
                    var line = 0;
                    foreach (var seqItem in selectedPlats)
                    {
                        DrawLine(currentItem, isEditable, selectedPlats.Length, ref changed, currentItemLoc, line, seqItem, true);
                        line++;
                    }
                    foreach (var seqItem in stops.Select((_, i) => i).Where(i => !selectedPlats.Contains(i)))
                    {
                        DrawLine(currentItem, isEditable, 0, ref changed, currentItemLoc, line, seqItem, false);
                        line++;
                    }
                    m_scrollPos = scroll.scrollPosition;
                }
            }
            else
            {
                GUILayout.Label(Str.we_buildingEditor_buildingIsNotTransportStation);
            }
            return changed;
        }

        private bool DrawLine(WriteOnBuildingPropXml currentItem, bool isEditable, int ordenableLength, ref bool changed, WriteOnBuildingPropXml currentItemLoc, int line, int seqItem, bool isSelected)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(string.Format(Str.we_buildingEditor_platformNamePlaceholder, seqItem + 1), new GUIStyle(isSelected ? WEUIUtils.GreenButton : GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                }))
                {
                    TogglePlatform(seqItem, currentItemLoc);
                    changed = true;
                }
                GUILayout.Space(30);
                var rect = GUILayoutUtility.GetLastRect();
                rect.height = 18;
                if (isSelected && isEditable && line > 0 && GUI.Button(rect, "↑"))
                {
                    MoveUp(seqItem, currentItem);
                    changed = true;
                }
                GUILayout.Space(30);
                rect = GUILayoutUtility.GetLastRect();
                rect.height = 18;
                if (isSelected && isEditable && line < ordenableLength - 1 && GUI.Button(rect, "↓"))
                {
                    MoveDown(seqItem, currentItem);
                    changed = true;
                }
            }

            return changed;
        }

        private void MoveDown(int seqItem, WriteOnBuildingPropXml currentItem)
        {
            var currentPos = Array.IndexOf(currentItem.m_platforms, seqItem);
            if (currentPos != -1 && currentPos < currentItem.m_platforms.Length - 1)
            {
                var temp = currentItem.m_platforms[currentPos + 1];
                currentItem.m_platforms[currentPos + 1] = seqItem;
                currentItem.m_platforms[currentPos] = temp;
            }
        }

        private void MoveUp(int seqItem, WriteOnBuildingPropXml currentItem)
        {
            var currentPos = Array.IndexOf(currentItem.m_platforms, seqItem);
            if (currentPos != -1 && currentPos > 0)
            {
                var temp = currentItem.m_platforms[currentPos - 1];
                currentItem.m_platforms[currentPos - 1] = seqItem;
                currentItem.m_platforms[currentPos] = temp;
            }
        }

        private void TogglePlatform(int seqItem, WriteOnBuildingPropXml currentItem)
        {
            if (currentItem.m_platforms.Contains(seqItem))
            {
                currentItem.m_platforms = currentItem.m_platforms.Where(x => x != seqItem).ToArray();
            }
            else
            {
                currentItem.m_platforms = currentItem.m_platforms.AddItem(seqItem).ToArray();
            }
        }

        public void Reset()
        {
        }
    }
}
