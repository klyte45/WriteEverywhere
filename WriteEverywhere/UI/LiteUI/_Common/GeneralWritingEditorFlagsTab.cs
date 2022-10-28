using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class GeneralFlaggedTab<F, F1, F2> : IGUITab<F> where F : FlaggedSettings where F1 : Enum, IConvertible where F2 : Enum, IConvertible
    {
        private readonly F1[] m_flagsToSelect1
            = Enum.GetValues(typeof(F1))
            .Cast<F1>()
            .Where(x => Enum.GetName(typeof(F1), x) != null)
            .OrderBy(x => x.ToString())
            .ToArray();
        private readonly F2[] m_flagsToSelect2
            = Enum.GetValues(typeof(F2))
            .Cast<F2>()
            .Where(x => Enum.GetName(typeof(F2), x) != null)
            .OrderBy(x => x.ToString())
            .ToArray();
        private readonly FlagEditor<F1, F2> m_flagEditor = new FlagEditor<F1, F2>();
        private Vector2 m_scrollFlags;

        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_Flag);


        public bool DrawArea(Vector2 areaRect, ref F item, int _, bool canEdit)
        {
            GUILayout.Label($"<color=#FFFF00>{Str.WTS_FLAGSREQUREDFORBIDDEN}</color>");
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollFlags))
            {
                m_flagEditor.DrawFlagsEditor(areaRect.x, m_flagsToSelect1, m_flagsToSelect2, item, canEdit);
                m_scrollFlags = scroll.scrollPosition;
            }
            return false;

        }

        public void Reset()
        {
        }
    }
}
