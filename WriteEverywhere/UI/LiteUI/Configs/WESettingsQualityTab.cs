using FontStashSharp;
using Kwytto.LiteUI;
using Kwytto.UI;
using SpriteFontPlus;
using System;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    public class WESettingsQualityTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.we_settings_quality;
        private readonly string[] m_textureQualityOptions = new string[] { "512", "1024", "2048", "<color=#FFFF00>4096 (!)</color>", "<color=#FF8800>8192 (!!!)</color>", "<color=#FF0000>16384 (WTF??)</color>" };
        private readonly string[] m_fontRenderingQuality = new string[] { "50%", "75%", "100%", "125%", "<color=#FFFF00>150% (!)</color>", "<color=#FF8800>200% (!!!)</color>", "<color=#FF0000>400% (BEWARE!)</color>", "<color=#FF00FF>800% (You don't need this!)</color>" };
        private readonly string[] m_maxParallelWordsOptions = new string[] { "1", "2", "4", "8", "16", "32", "64", "<color=#FFFF00>128 (!)</color>", "<color=#FF8800>256 (!!)</color>", "<color=#FF0000>512 (Your game may freeze)</color>", "<color=#FF00FF>1024 (Your game WILL freeze)</color>" };
        public static readonly float[] m_qualityArray = new float[] { .5f, .75f, 1f, 1.25f, 1.5f, 2f, 4f, 8f };
        private readonly GUIRootWindowBase root;

        public WESettingsQualityTab(GUIRootWindowBase root)
        {
            this.root = root;
        }

        public void DrawArea(Vector2 tabAreaSize)
        {
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (new GUILayout.VerticalScope())
                {
                    if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_INITIAL_TEXTURE_SIZE_FONT, ModInstance.StartTextureSizeFont, m_textureQualityOptions, out int newValue, root))
                    {
                        ModInstance.StartTextureSizeFont.value = newValue;
                    }
                    if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_FONT_QUALITY, ModInstance.FontQuality, m_fontRenderingQuality, out newValue, root))
                    {
                        ModInstance.FontQuality.value = newValue;
                        FontServer.instance.SetQualityMultiplier(m_qualityArray[newValue]);
                        MainController.ReloadFontsFromPath();
                    };
                    if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_MAX_PARALLEL_WORD_PROCESSES, Convert.ToString(FontSystem.MaxCoroutines, 2).Length - 1, m_maxParallelWordsOptions, out newValue, root))
                    {
                        FontSystem.MaxCoroutines.value = 1 << newValue;
                    }
                }
            }
        }

        public void Reset()
        {
        }
    }
}
