using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Rendering;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSOnNetTextTab : GeneralWritingGUI, IGUITab<OnNetInstanceCacheContainerXml>
    {
        public WTSOnNetTextTab(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, RefGetter<BoardTextDescriptorGeneralXml[]> getDescriptorArray, RefGetter<string> getFont) : base(colorPicker, TextRenderingClass.PlaceOnNet, infoGetter, getDescriptorArray, getFont)
        {
        }

        public static int LockSelectionInstanceNum { get; internal set; }

        public Texture TabIcon => KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AutoNameIcon);

        public bool DrawArea(Vector2 tabAreaSize, ref OnNetInstanceCacheContainerXml currentItem, int currentItemIdx, bool isEditable)
        {
            DoDraw(new Rect(default, tabAreaSize));
            return true;
        }



        public override void Reset()
        {
            base.Reset();
            ReloadList();
        }
    }
}
