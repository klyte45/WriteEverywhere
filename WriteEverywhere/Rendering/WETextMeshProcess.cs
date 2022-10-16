extern alias TLM;
extern alias VS;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Rendering
{

    public static class WETextMeshProcess
    {
        internal static BasicRenderInformation GetTextMesh(WriteOnBuildingXml buildingXml, TextToWriteOnXml textDescriptor, ushort refID, int boardIdx, int secIdx, BaseWriteOnXml instance)
        {
            if (instance is LayoutDescriptorVehicleXml vehicleDescriptor)
            {
                return SwitchContent(null, vehicleDescriptor, textDescriptor, refID, boardIdx, secIdx, out var multipleOutput) ?? multipleOutput?.FirstOrDefault();
            }
            else if (instance is WriteOnBuildingPropXml buildingDescritpor)
            {
                return SwitchContent(buildingXml, buildingDescritpor, textDescriptor, refID, boardIdx, secIdx, out var multipleOutput) ?? multipleOutput?.FirstOrDefault();
            }
            else if (instance is OnNetInstanceCacheContainerXml onNet)
            {
                return WTSOnNetData.Instance.m_boardsContainers[refID] is WriteOnNetGroupXml
                    ? SwitchContent(null, onNet, textDescriptor, refID, boardIdx, secIdx, out var multipleOutput) ?? multipleOutput?.FirstOrDefault()
                    : null;
            }
            return WTSCacheSingleton.GetTextData(textDescriptor.Value?.ToString() ?? "", textDescriptor.m_prefix, textDescriptor.m_suffix, null, textDescriptor.m_overrideFont);
        }

        private static BasicRenderInformation SwitchContent(WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml propDescriptor, TextToWriteOnXml textDescriptor, ushort segmentId, int boardIdx, int secIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    break;
                case TextContent.ParameterizedText:
                case TextContent.ParameterizedSpriteFolder:
                case TextContent.ParameterizedSpriteSingle:
                    return TextParameterWrapperRendering.GetRenderInfo(propGroupDescriptor, propDescriptor, textDescriptor, segmentId, boardIdx, secIdx, out multipleOutput);
                case TextContent.TextParameterSequence:
                    return TextParameterWrapperRendering.GetRenderInfo(propGroupDescriptor, textDescriptor.ParameterSequence?.GetAt(SimulationManager.instance.m_referenceFrameIndex, segmentId), propDescriptor, textDescriptor, segmentId, boardIdx, secIdx, out multipleOutput);
            }
            return null;
        }
    }


}
