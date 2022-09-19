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
        }

        private float m_TargetScale;

        public void DrawArea(Vector2 tabAreaSize)
        {
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_settings_uiScale, ref m_TargetScale, 0.5f, 2f);
                    if (GUILayout.Button(Str.we_settings_applyUiScale))
                    {
                        ModInstance.UIScaleSaved.value = m_TargetScale;
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
