extern alias TLM;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Font;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Plugins;
using WriteEverywhere.Rendering;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{
    internal static class TextParameterValueWrapperRendering
    {
        internal static BasicRenderInformation GetTargetText(this TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml instance, TextToWriteOnXml textDescriptor, DynamicSpriteFont targetFont, ushort refId, int secRefId, int tercRefId, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            string targetStr = wrapper.m_originalCommand;
            switch (instance)
            {
                case OnNetInstanceCacheContainerXml cc:
                    targetStr = wrapper.GetTargetTextForNet(cc, refId, textDescriptor, out multipleOutput);
                    break;
                case WriteOnBuildingPropXml bd:
                    targetStr = wrapper.GetTargetTextForBuilding(propGroupDescriptor, bd, refId, textDescriptor, out multipleOutput);
                    break;
                case LayoutDescriptorVehicleXml ve:
                    targetStr = wrapper.GetTargetTextForVehicle(refId, textDescriptor, out multipleOutput);
                    break;
                default:
                    multipleOutput = null;
                    break;
            }
            return multipleOutput is null ? targetFont.DrawString(ModInstance.Controller, targetStr, default, FontServer.instance.ScaleEffective) : null;
        }



        public static string GetTargetTextForBuilding(this TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var type = wrapper.type;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            switch (type)
            {
                case VariableType.CurrentBuilding:
                    return buildingId == 0 || buildingDescriptor is null || !(subtype is VariableBuildingSubType targetSubtype2) || targetSubtype2 == VariableBuildingSubType.None
                        ? $"{subtype}@currBuilding"
                        : $"{targetSubtype2.GetFormattedString(buildingDescriptor.m_platforms, buildingId, wrapper) ?? originalCommand}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{targetCitySubtype.GetFormattedString(wrapper) ?? originalCommand}";
                    }
                    break;
                case VariableType.Parameter:
                    var buildingParam = WTSBuildingData.Instance.Parameters.TryGetValue(WTSBuildingPropsSingleton.FindParentBuilding(buildingId), out var parameter) ? parameter : null;
                    if (buildingParam == null || buildingParam.TextParameters.Count == 0)
                    {
                        return "<NO PARAMS SET FOR BUILDING!>";
                    }
                    var paramIdx = wrapper.paramContainer.paramIdx;
                    switch (textDescriptor.textContent)
                    {
                        case TextContent.None:
                            LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                            return null;
                        case TextContent.ParameterizedText:
                        Text:
                            if (buildingParam.GetParameter(paramIdx) is TextParameterWrapper tpw)
                            {
                                var result = tpw.GetTargetText(propGroupDescriptor, buildingDescriptor, textDescriptor, TextParameterWrapperRendering.GetTargetFont(propGroupDescriptor, buildingDescriptor, textDescriptor), buildingId, 0, 0, out multipleOutput);
                                if (result is null && (multipleOutput is null || multipleOutput?.Count() == 0))
                                {
                                    return $"<EMPTY PARAM#{paramIdx} NOT SET>";
                                }
                                if (multipleOutput is null)
                                {
                                    multipleOutput = new[] { result };
                                }
                                return null;
                            }
                            return $"<PARAM#{paramIdx} NOT SET>";
                        case TextContent.ParameterizedSpriteFolder:
                        ImageFolder:
                            multipleOutput = buildingParam.GetParameter(paramIdx) is TextParameterWrapper tpw2
                                ? (new[] { tpw2.GetSpriteFromCycle(textDescriptor, buildingDescriptor.TargetAssetParameter, buildingId, 0, 0) })
                                : (IEnumerable<BasicRenderInformation>)(new[] { ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet") });
                            return null;
                        case TextContent.ParameterizedSpriteSingle:
                        ImageSingle:
                            multipleOutput = new[] {buildingParam.GetParameter(paramIdx)  is TextParameterWrapper tpw3
                                ? tpw3.GetSpriteFromParameter(buildingDescriptor.TargetAssetParameter)
                                : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet") };
                            return null;
                        case TextContent.Any:
                        case TextContent.TextParameterSequence:
                            if (buildingParam.GetParameter(paramIdx) is TextParameterWrapper tpw4)
                            {
                                switch (tpw4.ParamType)
                                {
                                    case ParameterType.TEXT:
                                    case ParameterType.VARIABLE:
                                        goto Text;
                                    case ParameterType.IMAGE:
                                        goto ImageSingle;
                                    case ParameterType.FOLDER:
                                        goto ImageFolder;
                                    case ParameterType.EMPTY:
                                        break;
                                }
                            }
                            return $"<ANY PARAM#{paramIdx} NOT SET>";
                        case TextContent.LinesNameList:
                            break;
                        case TextContent.HwShield:
                            break;
                        case TextContent.TimeTemperature:
                            break;
                        case TextContent.LinesSymbols:
                            break;
                    }
                    break;
            }

            return originalCommand;
        }

        public static string GetTargetTextForVehicle(this TextParameterVariableWrapper wrapper, ushort vehicleId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var type = wrapper.type;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            switch (type)
            {
                case VariableType.CurrentBuilding:
                    var buildingId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_sourceBuilding;
                    return buildingId == 0 || !(subtype is VariableBuildingSubType targetSubtype) || targetSubtype == VariableBuildingSubType.None
                        ? $"{subtype}@vehicleSrcBuilding"
                        : $"{targetSubtype.GetFormattedString(null, buildingId, wrapper) ?? originalCommand}";
                case VariableType.CurrentVehicle:
                    return vehicleId == 0 || !(subtype is VariableVehicleSubType targetSubtype2) || targetSubtype2 == VariableVehicleSubType.None
                        ? $"{subtype}@currVehicle"
                        : $"{targetSubtype2.GetFormattedString(vehicleId, wrapper) ?? originalCommand}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{targetCitySubtype.GetFormattedString(wrapper) ?? originalCommand}";
                    }
                    break;
            }
            return originalCommand;
        }

        internal static string GetTargetTextForNet(this TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var type = wrapper.type;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            var paramContainer = wrapper.paramContainer;
            var index = wrapper.index;
            switch (type)
            {
                case VariableType.SegmentTarget:
                    var targId = propDescriptor?.GetTargetSegment(index) ?? 0;
                    return targId == 0 || !(subtype is VariableSegmentTargetSubType targetSubtype) || targetSubtype == VariableSegmentTargetSubType.None
                        ? $"{paramContainer.prefix}{subtype}@targ{index}{paramContainer.suffix}"
                        : $"{paramContainer.prefix}{targetSubtype.GetFormattedString(propDescriptor, targId, wrapper) ?? originalCommand}{paramContainer.suffix}";
                case VariableType.CurrentSegment:
                    return segmentId == 0 || !(subtype is VariableSegmentTargetSubType targetSubtype2) || targetSubtype2 == VariableSegmentTargetSubType.None
                        ? $"{paramContainer.prefix}{subtype}@currSeg"
                        : $"{paramContainer.prefix}{targetSubtype2.GetFormattedString(propDescriptor, segmentId, wrapper) ?? originalCommand}{paramContainer.suffix}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{paramContainer.prefix}{targetCitySubtype.GetFormattedString(wrapper) ?? originalCommand}{paramContainer.suffix}";
                    }
                    break;
                case VariableType.Invalid:
                    return $"<UNSUPPORTED PATH: {originalCommand}>";
                case VariableType.Parameter:
                    var paramIdx = paramContainer.paramIdx;
                    switch (textDescriptor.textContent)
                    {
                        case TextContent.None:
                            LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                            return null;
                        case TextContent.ParameterizedText:
                        Text:
                            if (propDescriptor.GetParameter(paramIdx) is TextParameterWrapper tpw)
                            {
                                var result = tpw.GetTargetText(null, propDescriptor, textDescriptor, TextParameterWrapperRendering.GetTargetFont(null, propDesc: propDescriptor, textDesc: textDescriptor), segmentId, 0, 0, out multipleOutput);
                                if (result is null && (multipleOutput is null || multipleOutput?.Count() == 0))
                                {
                                    return $"<EMPTY PARAM#{paramIdx} NOT SET>";
                                }
                                if (multipleOutput is null)
                                {
                                    multipleOutput = new[] { result };
                                }
                                return null;
                            }
                            return $"<PARAM#{paramIdx} NOT SET>";
                        case TextContent.ParameterizedSpriteFolder:
                        ImageFolder:
                            multipleOutput = propDescriptor.GetParameter(paramIdx) is TextParameterWrapper tpw2
                                ? (new[] { tpw2.GetSpriteFromCycle(textDescriptor, propDescriptor.TargetAssetParameter, segmentId, 0, 0) })
                                : (IEnumerable<BasicRenderInformation>)(new[] { ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet") });
                            return null;
                        case TextContent.ParameterizedSpriteSingle:
                        ImageSingle:
                            multipleOutput = new[] {propDescriptor.GetParameter(paramIdx)  is TextParameterWrapper tpw3
                                ? tpw3.GetSpriteFromParameter(propDescriptor.TargetAssetParameter)
                                : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet") };
                            return null;
                        case TextContent.Any:
                        case TextContent.TextParameterSequence:
                            if (propDescriptor.GetParameter(paramIdx) is TextParameterWrapper tpw4)
                            {
                                switch (tpw4.ParamType)
                                {
                                    case ParameterType.TEXT:
                                    case ParameterType.VARIABLE:
                                        goto Text;
                                    case ParameterType.IMAGE:
                                        goto ImageSingle;
                                    case ParameterType.FOLDER:
                                        goto ImageFolder;
                                    case ParameterType.EMPTY:
                                        break;
                                }
                            }
                            return $"<ANY PARAM#{paramIdx} NOT SET>";
                        case TextContent.LinesNameList:
                            break;
                        case TextContent.HwShield:
                            break;
                        case TextContent.TimeTemperature:
                            break;
                        case TextContent.LinesSymbols:
                            break;
                    }
                    break;
            }
            return originalCommand;
        }

        public static string TryFormat(this TextParameterVariableWrapper wrapper, float value, float multiplier) => (value * multiplier).ToString(wrapper.paramContainer.numberFormat);
        public static string TryFormat(this TextParameterVariableWrapper wrapper, long value) => value.ToString(wrapper.paramContainer.numberFormat);
        public static string TryFormat(this TextParameterVariableWrapper wrapper, FormattableString value) => value.GetFormatted(wrapper.paramContainer.stringFormat);
    }
}
