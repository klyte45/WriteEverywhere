using ColossalFramework.Plugins;
using ImprovedTransportManager.ModShared;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.ModShared;
using WriteEverywhere.Singleton;
using WriteEverywhere.TransportLines;

namespace K45_WE2ITM
{
    public class BridgeITM : IBridgeTLM
    {
        public override int Priority => -1;

        public override bool IsBridgeEnabled => true;

        public BridgeITM()
        {
            if (!PluginManager.instance.GetPluginsInfo().Any(x => x.assemblyCount > 0 && x.isEnabled && x.ContainsAssembly(typeof(ITMFacade).Assembly)))
            {
                throw new Exception("The ITM bridge isn't available due to the mod not being active. Using fallback!");
            }
            ITMFacade.Instance.EventLineDestinationsChanged += (lineId) =>
            {
                WEFacade.BuildingPropsSingleton.ResetLines();
                WTSCacheSingleton.ClearCacheLineName(new WTSLine(lineId, false));
            };

            ITMFacade.Instance.EventLineAttributeChanged += (x) =>
            {
                WTSCacheSingleton.ClearCacheLine(x);
            };
        }

        public override WTSLine GetStopLine(ushort stopId)
            => new WTSLine(NetManager.instance.m_nodes.m_buffer[stopId].m_transportLine, false);
        public override string GetLineName(WTSLine lineObj)
            => lineObj.regional ? "" : TransportManager.instance.GetLineName((ushort)lineObj.lineId);
        public override Color GetLineColor(WTSLine lineObj)
            => lineObj.regional ? Color.white : TransportManager.instance.GetLineColor((ushort)lineObj.lineId);


        public override string GetLineIdString(WTSLine lineObj)
            => ITMFacade.Instance.GetLineIdentifier(lineObj.lineId);

        public override LineLogoParameter GetLineLogoParameters(WTSLine lineObj)
            => null;

        public override string GetLineSortString(WTSLine lineObj)
            => GetLineIdString(lineObj);

        public override ushort GetStopBuildingInternal(ushort stopId, WTSLine lineObj)
            => ITMFacade.Instance.GetStopBuilding(stopId);

        public override string GetStopName(ushort stopId, WTSLine lineObj)
            => ITMFacade.Instance.GetStopName(stopId);

        public override WTSLine GetVehicleLine(ushort vehicleId)
            => new WTSLine(VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_transportLine, false);

        public override void MapLineDestinations(WTSLine lineObj, ref StopInformation[] cacheToUpdate)
            => FillStops(lineObj, ITMFacade.Instance.MapAllTerminals(lineObj.lineId).Select((x) => new DestinationPoco { stopId = x }).ToList(), ref cacheToUpdate);
    }
}
