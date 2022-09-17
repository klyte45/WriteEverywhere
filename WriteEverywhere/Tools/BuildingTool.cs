//using ColossalFramework;
//using ColossalFramework.Math;
//using ColossalFramework.UI;
//using Kwytto;
//using System;
//using System.Diagnostics;
//using UnityEngine;

//namespace WriteEverywhere.Tools
//{

//    public class BuildingEditorTool : BaseBuildingTool<BuildingEditorTool>
//    {
//        public event Action<ushort> OnBuildingSelect;

//        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
//        {

//            if (m_hoverBuilding != 0)
//            {
//                Color toolColor = m_hoverColor;
//                RenderOverlay(cameraInfo, toolColor, m_hoverBuilding);
//                return;
//            }

//        }

//        protected override void OnLeftClick()
//        {
//            if (m_hoverBuilding != 0)
//            {
//                OnBuildingSelect?.Invoke(m_hoverBuilding);
//                ToolsModifierControl.SetTool<DefaultTool>();
//            }
//        }

//        protected override void OnDisable()
//        {
//            OnBuildingSelect = null;
//            base.OnDisable();
//        }

//    }

//}
