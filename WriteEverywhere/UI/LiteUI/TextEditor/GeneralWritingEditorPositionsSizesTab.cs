using ColossalFramework.UI;
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
        private readonly string[] m_alignmentOptions = EnumI18nExtensions.GetAllValuesI18n<UIHorizontalAlignment>();
        private readonly GUIRootWindowBase m_root;
        public GeneralWritingEditorPositionsSizesTab(GUIRootWindowBase parent)
        {
            m_root = parent;

        }
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_MoveCross);

        public bool DrawArea(Vector2 tabAreaSize, ref BoardTextDescriptorGeneralXml currentItem, int currentItemIdx)
        {
            GUILayout.Label($"<i>{Str.WTS_TEXT_SIZE_ATTRIBUTES}</i>");
            var item = currentItem;
            bool isEditable = true;
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Position, Str.WTS_RELATIVE_POS, Str.WTS_RELATIVE_POS, isEditable);
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Rotation, Str.WTS_RELATIVE_ROT, Str.WTS_RELATIVE_ROT, isEditable);
            GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.LineMaxDimensions, Str.WTS_LINEDIMENSIONS, Str.WTS_LINEDIMENSIONS, isEditable, 0);
            if (GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.WTS_TEXT_ALIGN_HOR, (int)item.m_textAlign, m_alignmentOptions, out var newVal, m_root, isEditable))
            {
                item.m_textAlign = (UIHorizontalAlignment)newVal;
            }
            GUIKwyttoCommons.AddToggle(Str.WTS_RESIZE_Y_TEXT_OVERFLOW, ref item.m_applyOverflowResizingOnY, isEditable);
            if (item.m_applyOverflowResizingOnY)
            {
                GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_verticalAlignmentLineText, ref item.m_verticalAlignment, 0, 1, isEditable);
            }
            GUIKwyttoCommons.AddToggle(Str.WTS_CREATE_CLONE_180DEG, ref item.PlacingConfig.m_create180degYClone, isEditable);
            GUIKwyttoCommons.AddToggle(Str.WTS_CLONE_180DEG_INVERT_TEXT_HOR_ALIGN, ref item.PlacingConfig.m_invertYCloneHorizontalAlign, isEditable, item.PlacingConfig.m_create180degYClone);
            return false;
        }
        public void Reset() { }
    }
}
