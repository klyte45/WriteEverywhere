using ColossalFramework.UI;
using SpriteFontPlus;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.Utils
{
    internal class WTSUtils
    {

        public static void ReloadFontsOf(UIDropDown target, string targetValue, bool hasDefaultOption = false, bool force = false)
        {
            try
            {
                if (force)
                {
                    MainController.ReloadFontsFromPath();
                }
                var items = FontServer.instance.GetAllFonts().ToList();
                items.Sort();
                items.Remove(MainController.DEFAULT_FONT_KEY);
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
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

    
    }
}

