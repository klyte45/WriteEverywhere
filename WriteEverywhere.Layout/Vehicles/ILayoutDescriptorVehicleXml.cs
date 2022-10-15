

using Kwytto.Interfaces;

namespace WriteEverywhere.Xml
{
    public interface ILayoutDescriptorVehicleXml : ILibable
    {

        string VehicleAssetName { get; }
        BaseTextToWriteOnXml[] TextDescriptors { get; }
        string FontName { get; }
    }
}
