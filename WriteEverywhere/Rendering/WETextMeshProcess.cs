extern alias TLM;
extern alias VS;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Font.Utility;
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
                return GetTextForVehicle(textDescriptor, refID, boardIdx, secIdx, vehicleDescriptor);
            }
            else if (instance is WriteOnBuildingPropXml buildingDescritpor)
            {
                return GetTextForBuilding(buildingXml, textDescriptor, refID, boardIdx, secIdx, buildingDescritpor, out var multipleOutput) ?? multipleOutput?.FirstOrDefault();
            }
            else if (instance is OnNetInstanceCacheContainerXml onNet)
            {
                return GetTextForOnNet(textDescriptor, refID, boardIdx, secIdx, onNet, out var multipleOutput) ?? multipleOutput?.FirstOrDefault();
            }
            return WTSCacheSingleton.GetTextData(textDescriptor.Value?.ToString() ?? "", textDescriptor.m_prefix, textDescriptor.m_suffix, null, textDescriptor.m_overrideFont);
        }

        private static BasicRenderInformation GetTextForVehicle(TextToWriteOnXml textDescriptor, ushort refID, int boardIdx, int secIdx, LayoutDescriptorVehicleXml vehicleDescriptor)
        {
            return SwitchContent(null, vehicleDescriptor, textDescriptor, refID, boardIdx, secIdx, out _);
        }

        private static BasicRenderInformation GetTextForOnNet(TextToWriteOnXml textDescriptor, ushort segmentId, int boardIdx, int secIdx, OnNetInstanceCacheContainerXml propDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var data = WTSOnNetData.Instance.m_boardsContainers[segmentId];
            if (data == null)
            {
                return null;
            }
            return SwitchContent(null, propDescriptor, textDescriptor, segmentId, boardIdx, secIdx, out multipleOutput);
        }
        private static BasicRenderInformation GetTextForBuilding(WriteOnBuildingXml buildingXml, TextToWriteOnXml textDescriptor, ushort refID, int boardIdx, int secIdx, WriteOnBuildingPropXml buildingDescritpor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            return SwitchContent(buildingXml, buildingDescritpor, textDescriptor, refID, boardIdx, secIdx, out multipleOutput);
        }

        private static BasicRenderInformation SwitchContent(WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml propDescriptor, TextToWriteOnXml textDescriptor, ushort segmentId, int boardIdx, int secIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                case TextContent.LinesSymbols:
                case TextContent.LinesNameList:
                case TextContent.TimeTemperature:
                case TextContent.HwShield:
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
