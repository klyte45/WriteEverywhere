using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    public class WESettingsUITab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.we_settings_ui;

        public WESettingsUITab()
        {
            m_TargetScale = ModInstance.UIScaleSaved.value;
            m_TargetOpacity = ModInstance.UIOpacitySaved.value;
        }

        private float m_TargetScale;
        private float m_TargetOpacity;

        public void DrawArea(Vector2 tabAreaSize)
        {
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_settings_uiScale, ref m_TargetScale, 0.5f, 2f);
                    GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_settings_uiOpacity, ref m_TargetOpacity, 0, 1f);
                    if (GUILayout.Button(Str.we_settings_applyUiSettings))
                    {
                        ModInstance.UIScaleSaved.value = m_TargetScale;
                        ModInstance.UIOpacitySaved.value = Mathf.Clamp01(m_TargetOpacity);
                        foreach (var ui in GameObject.FindObjectsOfType<IOpacityChangingGUI>())
                        {
                            ui.BgOpacity = ModInstance.UIOpacitySaved.value;
                        }
                    }
                }
            }
        }



        public void Reset()
        {
            m_TargetScale = ModInstance.UIScaleSaved.value;
        }
    }
}
