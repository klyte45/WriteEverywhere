extern alias TLM;

using TLM::Bridge_WE2TLM;

namespace WriteEverywhere.Rendering
{

    public class IntercityTransportLineItemCache : ITransportLineItemCache
    {
        public ushort nodeId;
        public long? Id { get => nodeId; set => nodeId = (ushort)(value ?? 0); }

        public FormatableString Name
        {
            get
            {
                if (name is null)
                {
                    identifier = ModInstance.Controller.ConnectorTLM.GetLineName(new WTSLine() { lineId = nodeId, regional = true });
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
                    identifier = ModInstance.Controller.ConnectorTLM.GetLineIdString(new WTSLine() { lineId = nodeId, regional = true });
                }
                return identifier;
            }
        }

        private FormatableString name;
        private string identifier;
        public void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refID)
        {
            if (cacheToPurge.Has(CacheErasingFlags.NodeParameter))
            {
                name = null;
                identifier = null;
            }
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
