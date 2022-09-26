namespace WriteEverywhere.Rendering
{
    public interface ITransportLineItemCache : IItemCache
    {
        string Identifier { get; }
        FormattableString Name { get; }
    }

}
