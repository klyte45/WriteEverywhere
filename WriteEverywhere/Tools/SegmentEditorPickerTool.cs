using WriteEverywhere.UI;

namespace WriteEverywhere.Tools
{

    public class SegmentEditorPickerTool : RoadSegmentTool
    {
        protected override void OnEnable()
        {
            OnSelectSegment += (x) => WTSOnNetLiteUI.Instance.CurrentSegmentId = m_hoverSegment;
            base.OnEnable();
        }

    }

}
