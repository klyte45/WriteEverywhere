using ColossalFramework.UI;
using Kwytto.LiteUI;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Utils
{
    internal static class WEUIUtils
    {
        private static GUIStyle m_redButton;
        public static GUIStyle RedButton
        {
            get
            {
                if (m_redButton is null)
                {
                    m_redButton = new GUIStyle(GUI.skin.button)
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

        private static GUIStyle m_greenButton;
        public static GUIStyle GreenButton
        {
            get
            {
                if (m_greenButton is null)
                {
                    m_greenButton = new GUIStyle(GUI.skin.button)
                    {
                        normal = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.greenTexture,
                            textColor = Color.black
                        },
                        hover = new GUIStyleState()
                        {
                            background = GUIKwyttoCommons.darkGreenTexture,
                            textColor = Color.black
                        },
                    };
                }
                return m_greenButton;
            }
        }
    }
    internal class WTSUtils
    {

        public static void ReloadFontsOf(UIDropDown target, string targetValue, bool hasDefaultOption = false, bool force = false)
        {
            try
            {
                if (force)
                {
                    WEMainController.ReloadFontsFromPath();
                }
                var items = FontServer.instance.GetAllFonts().ToList();
                items.Sort();
                items.Remove(WEMainController.DEFAULT_FONT_KEY);
                items.Insert(0, Str.WTS_DEFAULT_FONT_LABEL);
                if (hasDefaultOption)
                {
                    items.Insert(0, Str.WTS_USE_GROUP_SETTING_FONT);
                }
                target.items = items.ToArray();
                if (items.Contains(targetValue))
                {
                    target.selectedIndex = items.IndexOf(targetValue);
                }
                else
                {
                    target.selectedIndex = 0;
                }
            }
            catch
            {
                GameObject.DestroyImmediate(target);
            }
        }
        public static void SolveTangents(Mesh mesh)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
        }
    }
}

