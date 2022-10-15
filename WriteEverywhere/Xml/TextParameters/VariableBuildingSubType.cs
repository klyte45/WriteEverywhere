extern alias TLM;
using System.Collections.Generic;
using System.Linq;
using TLM::Bridge_WE2TLM;
using WriteEverywhere.Plugins;
using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;

namespace WriteEverywhere.Xml
{

    internal static class VariableBuildingSubTypeExtensions
    {

        public static string GetFormattedString(this VariableBuildingSubType var, IEnumerable<int> platforms, ushort buildingId, TextParameterVariableWrapper varWrapper)
        {
            if (buildingId == 0)
            {
                return null;
            }
            StopInformation targetStop;
            switch (var)
            {
                case VariableBuildingSubType.OwnName:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetBuilding(buildingId).Name);
                case VariableBuildingSubType.NextStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_nextStopId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.PrevStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_previousStopId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.LastStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_destinationId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.PlatformNumber:
                    return varWrapper.TryFormat(platforms.FirstOrDefault() + 1);
                default:
                    return null;
            }
        }

    }
}
