extern alias WE;

using Kwytto.Interfaces;
using WE::WriteEverywhere.Xml;

namespace Bridge_WE2VS
{
    public interface ILayoutDescriptorVehicleXml : ILibable
    {

        string VehicleAssetName { get; }
        BaseTextToWriteOnXml[] TextDescriptors { get; }
        string FontName { get; }
    }
}
