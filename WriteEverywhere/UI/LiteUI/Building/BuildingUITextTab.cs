using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class BuildingUITextTab : GeneralWritingGUI, IGUITab<WriteOnBuildingPropXml>
    {
        public BuildingUITextTab(GUIColorPicker colorPicker, Func<PrefabInfo> infoGetter, RefGetter<TextToWriteOnXml[]> getDescriptorArray, RefGetter<string> getFont)
            : base(colorPicker, TextRenderingClass.Buildings, infoGetter, getDescriptorArray, getFont)
        {
        }

        public static int LockSelectionInstanceNum { get; internal set; }

        public Texture TabIcon => KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AutoNameIcon);

        public bool DrawArea(Vector2 tabAreaSize, ref WriteOnBuildingPropXml currentItem, int currentItemIdx, bool isEditable)
        {
            DoDraw(new Rect(default, tabAreaSize), isEditable);
            return true;
        }



        public override void Reset()
        {
            base.Reset();
            ReloadList();
        }
    }
}
