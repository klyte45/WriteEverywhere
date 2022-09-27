using Kwytto.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bridge_WE2TLM
{
    public abstract class IBridge : IBridgePrioritizable
    {
        public abstract int Priority { get; }
        public abstract bool IsBridgeEnabled { get; }

        public abstract LineLogoParameter GetLineLogoParameters(WTSLine lineObj);
        public abstract ushort GetStopBuildingInternal(ushort stopId, WTSLine lineObj);
        public abstract string GetStopName(ushort stopId, WTSLine lineObj);
        public abstract string GetLineSortString(WTSLine lineObj);
        public abstract string GetVehicleIdentifier(ushort vehicleId);
        public abstract string GetLineIdString(WTSLine lineObj);
        public abstract void MapLineDestinations(WTSLine lineObj, ref StopInformation[] cacheToUpdate);
        public abstract WTSLine GetVehicleLine(ushort vehicleId);
        public abstract WTSLine GetStopLine(ushort stopId);

        protected void FillStops(WTSLine lineObj, List<DestinationPoco> destinations, StopInformation[] cacheToUpdate)
        {
            if (destinations.Count == 0)
            {
                return;
            }

            var firstStop = destinations[0].stopId;
            var curStop = firstStop;
            var nextStop = TransportLine.GetNextStop(curStop);
            ushort prevStop = TransportLine.GetPrevStop(curStop);
            var destinationId = destinations[1 % destinations.Count].stopId;
            var destinationStr = destinations[1 % destinations.Count].stopName;
            do
            {
                var destinationInfo = destinations.Where(x => x.stopId == curStop).FirstOrDefault();
                if (!(destinationInfo is null))
                {
                    var nextDest = destinations.IndexOf(destinationInfo) + 1;
                    destinationId = destinations[nextDest % destinations.Count].stopId;
                    destinationStr = destinations[nextDest % destinations.Count].stopName;
                    if (destinationStr is null)
                    {
                        destinationStr = GetStopName(destinationId, lineObj);
                    }
                }

                cacheToUpdate[curStop] = new StopInformation
                {
                    m_lineId = (ushort)lineObj.lineId,
                    m_regionalLine = lineObj.regional,
                    m_destinationId = destinationId,
                    m_nextStopId = nextStop,
                    m_previousStopId = prevStop,
                    m_stopId = curStop,
                    m_destinationString = destinationStr
                };
                prevStop = curStop;
                curStop = nextStop;
                nextStop = TransportLine.GetNextStop(nextStop);

            } while (curStop != 0 && curStop != firstStop);
        }

        public abstract void OnAutoNameParameterChanged();

        public abstract string GetLineName(WTSLine lineObj);
        public abstract Color GetLineColor(WTSLine lineObj);

    }
}
