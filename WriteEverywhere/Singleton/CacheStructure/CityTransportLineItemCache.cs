extern alias TLM;

using TLM::Bridge_WE2TLM;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Rendering
{
    public class CityTransportLineItemCache : ITransportLineItemCache
    {
        public ushort transportLineId;
        public long? Id { get => transportLineId; set => transportLineId = (ushort)(value ?? 0); }
        public FormattableString Name
        {
            get
            {
                if (name is null)
                {
                    name = ModInstance.Controller.ConnectorTLM.GetLineName(new WTSLine() { lineId = transportLineId, regional = false });
                }
                return name.AsFormattable();
            }
        }

        public string Identifier
        {
            get
            {
                if (identifier is null)
                {
                    identifier = ModInstance.Controller.ConnectorTLM.GetLineIdString(new WTSLine() { lineId = transportLineId, regional = false });
                }
                return identifier;
            }
        }

        private string name;
        private string identifier;
        public void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refID)
        {
            if (cacheToPurge.Has(CacheErasingFlags.LineName))
            {
                name = null;
            }
            if (cacheToPurge.Has(CacheErasingFlags.LineId))
            {
                identifier = null;
            }
        }
    }

}
