﻿using Kwytto.Tools;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace WriteEverywhere.Tools
{
    public class TargetSegmentPickerTool : KwyttoSegmentToolBase
    {
        public event Action<ushort> OnSelectSegment;
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
                OnSelectSegment?.Invoke(m_hoverSegment);
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
        protected override void OnRightClick() => ToolsModifierControl.SetTool<DefaultTool>();

        protected override void OnDisable()
        {
            OnSelectSegment = null;
            base.OnDisable();
        }

    }
}
