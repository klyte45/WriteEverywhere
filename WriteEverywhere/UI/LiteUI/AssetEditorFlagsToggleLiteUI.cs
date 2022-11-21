using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Utils;

namespace WriteEverywhere.UI
{
    internal class AssetEditorFlagsToggleLiteUI : GUIOpacityChanging
    {
        public static AssetEditorFlagsToggleLiteUI Instance { get; private set; }

        protected override bool showOverModals => false;

        protected override bool requireModal => false;
        protected override float FontSizeMultiplier => .9f;

        public int CurrentFlags1 { get; private set; }
        public int CurrentFlags2 { get; private set; }

        private Tuple<int, string>[] m_currentEnum1;
        private Tuple<int, string>[] m_currentEnum2;
        private Type m_loadedKindOfFlag;

        public override void Awake()
        {
            base.Awake();
            Instance = this;
            Init($"{ModInstance.Instance.GeneralName} - {Str.we_assetEditor_toggleFlags}", new Rect(128, 128, 500, 500), resizable: true, minSize: new Vector2(500, 500));

            Visible = false;
        }

        private Tuple<int, string>[] ToFlagArray<F1>() where F1 : Enum, IConvertible
            => Enum.GetValues(typeof(F1))
                .Cast<Enum>()
                .Where(x => Enum.GetName(typeof(F1), x) != null)
                .Select(x => Tuple.New((int)x.ToUInt64(), Enum.GetName(typeof(F1), x)))
                .Where(x => x.First != 0 && x.First != ~0)
                .OrderBy(x => x.Second)
                .ToArray();


        protected override void DrawWindow(Vector2 size)
        {
            if (!SceneUtils.IsAssetEditor)
            {
                return;
            }
            switch (ToolsModifierControl.toolController.m_editPrefabInfo)
            {
                case BuildingInfo i:
                    if (m_loadedKindOfFlag != i.GetType())
                    {
                        m_currentEnum1 = ToFlagArray<Building.Flags>();
                        m_currentEnum2 = ToFlagArray<Building.Flags2>();
                        m_loadedKindOfFlag = i.GetType();
                    }
                    break;
                case VehicleInfo i:
                    if (m_loadedKindOfFlag != i.GetType())
                    {
                        m_currentEnum1 = ToFlagArray<Vehicle.Flags>();
                        m_currentEnum2 = ToFlagArray<Vehicle.Flags2>();
                        m_loadedKindOfFlag = i.GetType();
                    }
                    break;
                default:
                    if (m_loadedKindOfFlag != null)
                    {
                        m_currentEnum1 = null;
                        m_currentEnum2 = null;
                        m_loadedKindOfFlag = null;
                    }
                    return;
            }
            int counter = 0;
            var itemsPerLine = Mathf.FloorToInt((size.x - 10) / 200);
            var width = (size.x - (6 * itemsPerLine)) / itemsPerLine;
            for (; counter < m_currentEnum1.Length + m_currentEnum2.Length;)
            {
                using (new GUILayout.HorizontalScope())
                {
                    for (int i = 0; i < itemsPerLine; i++)
                    {
                        if (counter < m_currentEnum1.Length)
                        {
                            var flag = m_currentEnum1[counter].First;
                            var isOn = (CurrentFlags1 & flag) != 0;
                            if (GUILayout.Button(m_currentEnum1[counter].Second, isOn ? WEUIUtils.GreenButton : GUI.skin.button, GUILayout.Width(width)))
                            {
                                CurrentFlags1 = isOn ? (CurrentFlags1 & ~flag) : (CurrentFlags1 | flag);
                            }
                        }
                        else
                        {
                            var flag = m_currentEnum1[counter].First;
                            var isOn = (CurrentFlags2 & flag) != 0;
                            if (GUILayout.Button(m_currentEnum1[counter].Second, isOn ? WEUIUtils.GreenButton : GUI.skin.button, GUILayout.Width(width)))
                            {
                                CurrentFlags2 = isOn ? (CurrentFlags2 & ~flag) : (CurrentFlags2 | flag);
                            }
                        }
                        counter++;
                        if (counter >= m_currentEnum1.Length + m_currentEnum2.Length)
                        {
                            GUILayout.FlexibleSpace();
                            break;
                        }
                    }
                }
            }

        }
    }
}
