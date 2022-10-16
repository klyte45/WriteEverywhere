using Kwytto.LiteUI;
using Kwytto.UI;
using System;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    public class WESettingsClockTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.we_settings_clockAndFormatting;
        private readonly string[] m_clockPrecisionOptions = new string[] { "30", "20", "15 (DEFAULT)", "12", "10", "7.5", "6", "5", "4", "<color=#FFFF00>3 (!)</color>", "<color=#FF8800>2 (!!)</color>", "<color=#FF0000>1 (!!!!)</color>" };
        public readonly float[] m_clockPrecision = new float[] { 30, 20, 15, 12, 10, 7.5f, 6, 5, 4, 3, 2, 1 };
        private readonly GUIRootWindowBase root;

        public WESettingsClockTab(GUIRootWindowBase root)
        {
            this.root = root;
        }

        public void DrawArea(Vector2 tabAreaSize)
        {
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(Str.we_settings_formattingSettings);
                    GUIKwyttoCommons.TextWithLabel(tabAreaSize.x, Str.we_settings_formatterLocaleCode, WTSEtcData.FormatCultureString, (x) => WTSEtcData.FormatCultureString = x);
                    if (WTSEtcData.FormatCultureString != WTSEtcData.FormatCulture.ToString())
                    {
                        GUILayout.Label($"<color=#FF0000>{string.Format(Str.we_settings_invalidLocaleCode, $"{WTSEtcData.FormatCulture} ({WTSEtcData.FormatCulture?.EnglishName})")}</color>");
                    }
                    else
                    {
                        GUILayout.Label($"<color=#00ff00>{WTSEtcData.FormatCulture.EnglishName}</color>");
                    }

                    GUILayout.Label(Str.we_settings_clockFormat);
                    if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_CLOCK_MINUTES_PRECISION, Array.IndexOf(m_clockPrecision, ModInstance.ClockPrecision), m_clockPrecisionOptions, out int newValue, root))
                    {
                        ModInstance.ClockPrecision.value = m_clockPrecision[newValue];
                    }
                    bool newValBool = ModInstance.ClockShowLeadingZero.value;
                    if (GUIKwyttoCommons.AddToggle(Str.WTS_CLOCK_SHOW_LEADING_ZERO, ref newValBool))
                    {
                        ModInstance.ClockShowLeadingZero.value = newValBool;
                    };
                    newValBool = ModInstance.Clock12hFormat.value;
                    if (GUIKwyttoCommons.AddToggle(Str.WTS_CLOCK_12H_CLOCK, ref newValBool))
                    {
                        ModInstance.Clock12hFormat.value = newValBool;
                    };

                }
            }
        }

        public void Reset()
        {
        }
    }
}
