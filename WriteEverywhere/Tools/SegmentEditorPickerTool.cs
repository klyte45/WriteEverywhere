using Kwytto.Tools;
using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere.UI;

namespace WriteEverywhere.Tools
{

    public class SegmentEditorPickerTool : KwyttoSegmentToolBase
    {
        public void OnSelectSegment()
        {
            WTSOnNetLiteUI.Instance.CurrentSegmentId = m_hoverSegment;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (m_hoverSegment != 0)
            {
                Color toolColor = m_hoverColor;
                RenderOverlayUtils.RenderNetSegmentOverlay(cameraInfo, toolColor, m_hoverSegment);
                return;
            }
        }

        protected override void OnLeftClick()
        {
            if (m_hoverSegment != 0)
            {
                OnSelectSegment();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
        protected override void OnRightClick() => ToolsModifierControl.SetTool<DefaultTool>();

        protected override void OnEnable()
        {
            base.OnEnable();
            WTSOnNetLiteUI.Instance.Visible = false;
        }
    }

}
