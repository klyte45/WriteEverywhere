using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorPositionsSizesTab : IGUITab<BoardTextDescriptorGeneralXml>
    {
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_MoveCross);

        public bool DrawArea(Vector2 tabAreaSize, ref BoardTextDescriptorGeneralXml currentItem, int currentItemIdx)
        {
            GUILayout.Label($"<i>{Str.WTS_TEXT_SIZE_ATTRIBUTES}</i>");
            var item = currentItem;
            bool isEditable = true;
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Position, "K45_WTS_RELATIVE_POS", "K45_WTS_RELATIVE_POS", isEditable);
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Rotation, "K45_WTS_RELATIVE_ROT", "K45_WTS_RELATIVE_ROT", isEditable);
            GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.LineMaxDimensions, "K45_WTS_LINEDIMENSIONS", "K45_WTS_LINEDIMENSIONS", isEditable, 0);
            GUIKwyttoCommons.AddToggle("K45_WTS_RESIZE_Y_TEXT_OVERFLOW", ref item.m_applyOverflowResizingOnY, isEditable);
            GUIKwyttoCommons.AddToggle("K45_WTS_CREATE_CLONE_180DEG", ref item.PlacingConfig.m_create180degYClone, isEditable);
            GUIKwyttoCommons.AddToggle("K45_WTS_CLONE_180DEG_INVERT_TEXT_HOR_ALIGN", ref item.PlacingConfig.m_invertYCloneHorizontalAlign, isEditable, item.PlacingConfig.m_create180degYClone);
            return false;
        }
        public void Reset() { }
    }
}
