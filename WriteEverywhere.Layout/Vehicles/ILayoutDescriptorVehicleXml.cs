

using Kwytto.Interfaces;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{
    public interface ILayoutDescriptorVehicleXml : ILibable
    {

        string VehicleAssetName { get; }
        BaseTextToWriteOnXml[] TextDescriptors { get; }
        string FontName { get; }
    }
}
