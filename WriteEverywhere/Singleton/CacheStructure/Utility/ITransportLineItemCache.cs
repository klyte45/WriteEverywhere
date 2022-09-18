namespace WriteEverywhere.Rendering
{
    public interface ITransportLineItemCache : IItemCache
    {
        string Identifier { get; }
        FormatableString Name { get; }
    }

}
