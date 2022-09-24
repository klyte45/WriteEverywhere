using Kwytto.LiteUI;
using Kwytto.Tools;
using UnityEngine;

namespace WriteEverywhere.Tools
{
    public class BuildingEditorPickerTool : KwyttoBuildingToolBase
    {
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {

            if (m_hoverBuilding != 0)
            {
                Color toolColor = m_hoverColor;
                RenderOverlay(cameraInfo, toolColor, m_hoverBuilding);
                return;
            }

        }

        protected override void OnLeftClick()
        {
            if (m_hoverBuilding != 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    buttons = KwyttoDialog.basicOkButtonBar,
                    message = $"PICKED! {m_hoverBuilding} => {BuildingBuffer[m_hoverBuilding].Info.GetUncheckedLocalizedTitle()}"
                });
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }


    }

}
