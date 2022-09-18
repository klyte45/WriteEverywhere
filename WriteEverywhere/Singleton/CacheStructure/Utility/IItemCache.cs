using Kwytto.Interfaces;

namespace WriteEverywhere.Rendering
{
    public interface IItemCache : IIdentifiable
    {
        void PurgeCache(CacheErasingFlags cacheToPurge, InstanceID refId);
    }

}
