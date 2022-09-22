using Kwytto.Tools;
using System;

namespace WriteEverywhere.Tools
{

    public class VehicleEditorTool : KwyttoVehicleToolBase
    {
        public event Action<ushort> OnVehicleSelect;
        public event Action<ushort> OnParkedVehicleSelect;

        protected override void OnLeftClick()
        {
            if (m_hoverVehicle != 0 && !(OnVehicleSelect is null))
            {
                OnVehicleSelect.Invoke(m_hoverVehicle);
                ToolsModifierControl.SetTool<DefaultTool>();
            }
            else if (m_hoverParkedVehicle != 0 && !(OnParkedVehicleSelect is null))
            {
                OnParkedVehicleSelect.Invoke(m_hoverParkedVehicle);
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }

        protected override void OnDisable()
        {
            OnVehicleSelect = null;
            base.OnDisable();
        }

    }

}
