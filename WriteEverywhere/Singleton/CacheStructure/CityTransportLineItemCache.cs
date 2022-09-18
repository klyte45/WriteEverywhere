extern alias TLM;

using TLM::Bridge_WE2TLM;

namespace WriteEverywhere.Rendering
{
    public class CityTransportLineItemCache : ITransportLineItemCache
    {
        public ushort transportLineId;
        public long? Id { get => transportLineId; set => transportLineId = (ushort)(value ?? 0); }
        public FormatableString Name
        {
            get
            {
                if (name is null)
                {
                    name = new FormatableString(ModInstance.Controller.ConnectorTLM.GetLineName(new WTSLine() { lineId = transportLineId, regional = false }));
                }
                return name;
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

        private FormatableString name;
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
