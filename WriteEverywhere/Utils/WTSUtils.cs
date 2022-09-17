using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Localization;
//using Kwytto.Utils;
//using WriteEverywhere.Data;
using SpriteFontPlus;
//using System;
//using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        //public static string ApplyAbbreviations(string name)
        //{
        //    if (ModInstance.Controller.AbbreviationFiles.TryGetValue(WTSRoadNodesData.Instance.AbbreviationFile ?? "", out Dictionary<string, string> translations))
        //    {
        //        foreach (string key in translations.Keys.Where(x => x.Contains(" ")))
        //        {
        //            name = TextUtils.ReplaceCaseInsensitive(name, key, translations[key], StringComparison.OrdinalIgnoreCase);

        //        }
        //        string[] parts = name.Split(' ');
        //        for (int i = 0; i < parts.Length; i++)
        //        {
        //            if ((i == 0 && translations.TryGetValue($"^{parts[i]}", out string replacement))
        //                || (i == parts.Length - 1 && translations.TryGetValue($"{parts[i]}$", out replacement))
        //                || (i > 0 && i < parts.Length - 1 && translations.TryGetValue($"={parts[i]}=", out replacement))
        //                || translations.TryGetValue(parts[i], out replacement))
        //            {
        //                parts[i] = replacement;
        //            }
        //        }
        //        return string.Join(" ", parts.Where(x => !x.IsNullOrWhiteSpace()).ToArray());

        //    }
        //    else
        //    {
        //        return name;
        //    }
        //}
    }
}

