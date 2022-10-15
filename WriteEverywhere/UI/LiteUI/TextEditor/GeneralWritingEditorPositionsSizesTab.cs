using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorPositionsSizesTab : IGUITab<TextToWriteOnXml>
    {
        private readonly string[] m_cloneOptionsStr = EnumI18nExtensions.GetAllValuesI18n<YCloneType>();
        private readonly YCloneType[] m_cloneOptions = Enum.GetValues(typeof(YCloneType)).Cast<YCloneType>().ToArray();
        private readonly GUIRootWindowBase m_root;
        public GeneralWritingEditorPositionsSizesTab(GUIRootWindowBase parent)
        {
            m_root = parent;

        }
        public Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_MoveCross);

        public bool DrawArea(Vector2 tabAreaSize, ref TextToWriteOnXml currentItem, int currentItemIdx, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_TEXT_SIZE_ATTRIBUTES}</i>");
            var item = currentItem;
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Position, Str.WTS_RELATIVE_POS, Str.WTS_RELATIVE_POS, isEditable);
            GUIKwyttoCommons.AddVector3Field(tabAreaSize.x, item.PlacingConfig.Rotation, Str.WTS_RELATIVE_ROT, Str.WTS_RELATIVE_ROT, isEditable);
            GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.LineMaxDimensions, Str.WTS_LINEDIMENSIONS, Str.WTS_LINEDIMENSIONS, isEditable, 0);
            GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_ALIGN_HOR, ref item.m_horizontalAlignment, 0, 1, isEditable);
            GUIKwyttoCommons.AddToggle(Str.WTS_RESIZE_Y_TEXT_OVERFLOW, ref item.m_applyOverflowResizingOnY, isEditable);
            if (item.m_applyOverflowResizingOnY)
            {
                GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_verticalAlignmentLineText, ref item.m_verticalAlignment, 0, 1, isEditable);
            }
            GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_cloneType180, ref item.PlacingConfig.m_yCloneType, m_cloneOptionsStr, m_cloneOptions, m_root, isEditable);
            GUIKwyttoCommons.AddToggle(Str.WTS_CLONE_180DEG_INVERT_TEXT_HOR_ALIGN, ref item.PlacingConfig.m_invertYCloneHorizontalAlign, isEditable, item.PlacingConfig.m_yCloneType != YCloneType.None);
            return false;
        }
        public void Reset() { }
    }
}
