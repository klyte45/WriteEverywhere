extern alias TLM;

using System.Collections.Generic;
using System.Linq;
using TLM::Bridge_WE2TLM;
using WriteEverywhere.Layout;

namespace WriteEverywhere.Utils
{
    internal static class WTSStopUtils
    {
        internal static StopInformation[] GetAllTargetStopInfo(WriteOnBuildingPropXml descriptor, ushort buildingId)
            => descriptor?.m_platforms?.SelectMany(platform =>
            {
                if (ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId] != null && ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId]?.ElementAtOrDefault(platform)?.Length > 0)
                {
                    StopInformation[] line = ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId][platform];
                    return line;
                }
                return new StopInformation[0];
            }).ToArray();

        internal static StopInformation[] GetTargetStopInfo(WriteOnBuildingPropXml descriptor, ushort buildingId)
        {
            foreach (int platform in descriptor.m_platforms)
            {
                if (ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId] != null && ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId].ElementAtOrDefault(platform)?.Length > 0)
                {
                    StopInformation[] line = ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId][platform];
                    return line;
                }
            }
            return m_emptyInfo;
        }
        internal static StopInformation[] GetTargetStopInfo(IEnumerable<int> platforms, ushort buildingId)
        {
            foreach (int platform in platforms)
            {
                if (ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId] != null && ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId].ElementAtOrDefault(platform)?.Length > 0)
                {
                    StopInformation[] line = ModInstance.Controller.BuildingPropsSingleton.m_platformToLine[buildingId][platform];
                    return line;
                }
            }
            return m_emptyInfo;
        }
        internal static StopInformation GetStopDestinationData(ushort targetStopId)
        {
            var result = ModInstance.Controller.BuildingPropsSingleton.m_stopInformation[targetStopId];
            if (result.m_lineId == 0)
            {
                var line = ModInstance.Controller.ConnectorTLM.GetStopLine(targetStopId);
                if (line.lineId > 0)
                {
                    ModInstance.Controller.ConnectorTLM.MapLineDestinations(line, ref ModInstance.Controller.BuildingPropsSingleton.m_stopInformation);
                    result = ModInstance.Controller.BuildingPropsSingleton.m_stopInformation[targetStopId];
                }
                else
                {
                    result.m_lineId = ushort.MaxValue;
                }
            }
            return result;
        }


        internal static readonly StopInformation[] m_emptyInfo = new StopInformation[0];
    }
}

