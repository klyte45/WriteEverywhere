namespace WriteEverywhere.Rendering
{
    public class VehicleItemCache : IItemCache
    {
        public ushort vehicleId;
        public long? Id { get => vehicleId; set => vehicleId = (ushort)(value ?? 0); }
        public string Identifier
        {
            get
            {
                if (identifier is null)
                {
                    identifier = ModInstance.Controller.ConnectorTLM.GetVehicleIdentifier(vehicleId);
                }
                return identifier;
            }
        }

        private string identifier;
        public void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refID)
        {
            if (cacheToPurge.Has(CacheErasingFlags.VehicleParameters))
            {
                identifier = null;
            }
        }
    }

}
