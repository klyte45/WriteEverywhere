using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSOnNetTextTab : GeneralWritingGUI, IGUITab<OnNetInstanceCacheContainerXml>
    {
        public WTSOnNetTextTab(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, RefGetter<BoardTextDescriptorGeneralXml[]> getDescriptorArray, RefGetter<string> getFont) : base(colorPicker, infoGetter, getDescriptorArray, getFont)
        {
        }

        public Texture TabIcon => KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AutoNameIcon);

        public int CurrentTab { get; private set; }

        public bool DrawArea(Vector2 tabAreaSize, ref OnNetInstanceCacheContainerXml currentItem, int currentItemIdx)
        {
            DoDraw(new Rect(default, tabAreaSize));
            return true;
        }

        public void Reset()
        {
            CurrentTab = 0;
        }
    }
}
