using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class WTSOnNetTextTab : GeneralWritingGUI<NetSegment.Flags, NetSegment.Flags2>, IGUITab<OnNetInstanceCacheContainerXml>
    {
        public WTSOnNetTextTab(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, RefGetter<TextToWriteOnXml[]> getDescriptorArray, RefGetter<string> getFont, Func<bool> isAnyPropScreenLoaded)
            : base(colorPicker, TextRenderingClass.PlaceOnNet, infoGetter, getDescriptorArray, getFont)
        {
            m_isAnyPropScreenLoaded = isAnyPropScreenLoaded;
        }

        private Func<bool> m_isAnyPropScreenLoaded;

        public static int LockSelectionInstanceNum { get; internal set; }

        public Texture TabIcon => KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AutoNameIcon);

        public bool DrawArea(Vector2 tabAreaSize, ref OnNetInstanceCacheContainerXml currentItem, int currentItemIdx, bool isEditable)
        {
            DoDraw(new Rect(default, tabAreaSize), true);
            return true;
        }
        public override void Reset()
        {
            base.Reset();
            if (m_isAnyPropScreenLoaded())
            {
                ReloadList();
            }
        }
    }
}
