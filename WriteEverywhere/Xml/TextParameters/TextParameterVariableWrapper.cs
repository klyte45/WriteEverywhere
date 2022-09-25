extern alias VS;

using Kwytto.Utils;
using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Rendering;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{
    public class TextParameterVariableWrapper
    {
        public readonly string m_originalCommand;
        public readonly VariableType m_varType;

        public TextParameterVariableWrapper(string input, TextRenderingClass renderingClass = TextRenderingClass.Any)
        {
            m_originalCommand = input;
            var parameterPath = CommandLevel.GetParameterPath(input);
            if (parameterPath.Length > 0)
            {
                m_varType = VariableType.Invalid;
                try
                {
                    m_varType = (VariableType)Enum.Parse(typeof(VariableType), parameterPath[0]);
                }
                catch { }
                if (!m_varType.Supports(renderingClass))
                {
                    return;
                }
                switch (m_varType)
                {
                    case VariableType.SegmentTarget:
                        if (parameterPath.Length >= 3 && byte.TryParse(parameterPath[1], out byte targIdx))
                        {
                            try
                            {
                                if (Enum.Parse(typeof(VariableSegmentTargetSubType), parameterPath[2]) is VariableSegmentTargetSubType tt
                                    && tt.ReadData(parameterPath.Skip(3).ToArray(), ref subtype, out paramContainer))
                                {
                                    index = targIdx;
                                    type = VariableType.SegmentTarget;
                                }
                            }
                            catch { }
                        }
                        break;
                    case VariableType.CurrentSegment:
                        if (parameterPath.Length >= 2)
                        {
                            try
                            {
                                if (Enum.Parse(typeof(VariableSegmentTargetSubType), parameterPath[1]) is VariableSegmentTargetSubType tt
                                    && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                                {
                                    type = VariableType.CurrentSegment;
                                }
                            }
                            catch { }
                        }
                        break;
                    case VariableType.CityData:
                        if (parameterPath.Length >= 2)
                        {
                            try
                            {
                                if (Enum.Parse(typeof(VariableCitySubType), parameterPath[1]) is VariableCitySubType tt
                                    && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                                {
                                    type = VariableType.CityData;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                    case VariableType.CurrentBuilding:
                        if (parameterPath.Length >= 2)
                        {
                            try
                            {
                                if (Enum.Parse(typeof(VariableBuildingSubType), parameterPath[1]) is VariableBuildingSubType tt
                                    && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                                {
                                    type = VariableType.CurrentBuilding;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                    case VariableType.CurrentVehicle:
                        if (parameterPath.Length >= 2)
                        {
                            try
                            {
                                if (Enum.Parse(typeof(VariableVehicleSubType), parameterPath[1]) is VariableVehicleSubType tt
                                    && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                                {
                                    type = VariableType.CurrentVehicle;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                    case VariableType.Parameter:
                        if (parameterPath.Length >= 2)
                        {
                            try
                            {
                                if (int.TryParse(parameterPath[1], out var idx))
                                {
                                    paramContainer.paramIdx = idx;
                                    type = VariableType.Parameter;
                                    break;
                                }
                            }
                            catch { }
                        }
                        break;
                }
            }
        }


        private VariableType type = VariableType.Invalid;
        private byte index = 0;
        private Enum subtype = VariableSegmentTargetSubType.None;
        public readonly VariableExtraParameterContainer paramContainer = default;

        public struct VariableExtraParameterContainer
        {
            public string numberFormat;
            public string stringFormat;
            public string prefix;
            public string suffix;
            public int paramIdx;
        }


        internal BasicRenderInformation GetTargetText(WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml instance, TextToWriteOnXml textDescriptor, DynamicSpriteFont targetFont, ushort refId, int secRefId, int tercRefId, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            string targetStr = m_originalCommand;
            switch (instance)
            {
                case OnNetInstanceCacheContainerXml cc:
                    targetStr = GetTargetTextForNet(cc, refId, textDescriptor, out multipleOutput);
                    break;
                case WriteOnBuildingPropXml bd:
                    targetStr = GetTargetTextForBuilding(propGroupDescriptor, bd, refId, textDescriptor, out multipleOutput);
                    break;
                case LayoutDescriptorVehicleXml ve:
                    targetStr = GetTargetTextForVehicle(refId, textDescriptor, out multipleOutput);
                    break;
                default:
                    multipleOutput = null;
                    break;
            }
            return multipleOutput is null ? targetFont.DrawString(ModInstance.Controller, targetStr, default, FontServer.instance.ScaleEffective) : null;
        }



        public string GetTargetTextForBuilding(WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (type)
            {
                case VariableType.CurrentBuilding:
                    return buildingId == 0 || buildingDescriptor is null || !(subtype is VariableBuildingSubType targetSubtype2) || targetSubtype2 == VariableBuildingSubType.None
                        ? $"{subtype}@currBuilding"
                        : $"{targetSubtype2.GetFormattedString(buildingDescriptor.m_platforms, buildingId, this) ?? m_originalCommand}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{targetCitySubtype.GetFormattedString(this) ?? m_originalCommand}";
                    }
                    break;
                case VariableType.Parameter:
                    var buildingParam = WTSBuildingData.Instance.Parameters.TryGetValue(WTSBuildingPropsSingleton.FindParentBuilding(buildingId), out var parameter) ? parameter : null;
                    if (buildingParam == null || buildingParam.TextParameters.Count == 0)
                    {
                        return "<NO PARAMS SET FOR BUILDING!>";
                    }
                    var paramIdx = paramContainer.paramIdx;
                    switch (textDescriptor.textContent)
                    {
                        case TextContent.None:
                            LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                            return null;
                        case TextContent.ParameterizedText:
                        Text:
                            if (buildingParam.GetParameter(paramIdx) is TextParameterWrapper tpw)
                            {
                                var result = tpw.GetTargetText(propGroupDescriptor, buildingDescriptor, textDescriptor, TextParameterWrapper.GetTargetFont(propGroupDescriptor, buildingDescriptor, textDescriptor), buildingId, 0, 0, out multipleOutput);
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

            return m_originalCommand;
        }

        public string GetTargetTextForVehicle(ushort vehicleId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (type)
            {
                case VariableType.CurrentBuilding:
                    var buildingId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_sourceBuilding;
                    return buildingId == 0 || !(subtype is VariableBuildingSubType targetSubtype) || targetSubtype == VariableBuildingSubType.None
                        ? $"{subtype}@vehicleSrcBuilding"
                        : $"{targetSubtype.GetFormattedString(null, buildingId, this) ?? m_originalCommand}";
                case VariableType.CurrentVehicle:
                    return vehicleId == 0 || !(subtype is VariableVehicleSubType targetSubtype2) || targetSubtype2 == VariableVehicleSubType.None
                        ? $"{subtype}@currVehicle"
                        : $"{targetSubtype2.GetFormattedString(vehicleId, this) ?? m_originalCommand}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{targetCitySubtype.GetFormattedString(this) ?? m_originalCommand}";
                    }
                    break;
            }
            return m_originalCommand;
        }

        internal string GetTargetTextForNet(BaseWriteOnXml descriptor, ushort segmentId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var propDescriptor = descriptor as OnNetInstanceCacheContainerXml;
            switch (type)
            {
                case VariableType.SegmentTarget:
                    var targId = propDescriptor?.GetTargetSegment(index) ?? 0;
                    return targId == 0 || !(subtype is VariableSegmentTargetSubType targetSubtype) || targetSubtype == VariableSegmentTargetSubType.None
                        ? $"{paramContainer.prefix}{subtype}@targ{index}{paramContainer.suffix}"
                        : $"{paramContainer.prefix}{targetSubtype.GetFormattedString(propDescriptor, targId, this) ?? m_originalCommand}{paramContainer.suffix}";
                case VariableType.CurrentSegment:
                    return segmentId == 0 || !(subtype is VariableSegmentTargetSubType targetSubtype2) || targetSubtype2 == VariableSegmentTargetSubType.None
                        ? $"{paramContainer.prefix}{subtype}@currSeg"
                        : $"{paramContainer.prefix}{targetSubtype2.GetFormattedString(propDescriptor, segmentId, this) ?? m_originalCommand}{paramContainer.suffix}";
                case VariableType.CityData:
                    if ((subtype is VariableCitySubType targetCitySubtype))
                    {
                        return $"{paramContainer.prefix}{targetCitySubtype.GetFormattedString(this) ?? m_originalCommand}{paramContainer.suffix}";
                    }
                    break;
                case VariableType.Invalid:
                    return $"<UNSUPPORTED PATH: {m_originalCommand}>";
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
                                var result = tpw.GetTargetText(null, descriptor, textDescriptor, TextParameterWrapper.GetTargetFont(null, propDesc: descriptor, textDesc: textDescriptor), segmentId, 0, 0, out multipleOutput);
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
                                ? (new[] { tpw2.GetSpriteFromCycle(textDescriptor, descriptor.TargetAssetParameter, segmentId, 0, 0) })
                                : (IEnumerable<BasicRenderInformation>)(new[] { ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet") });
                            return null;
                        case TextContent.ParameterizedSpriteSingle:
                        ImageSingle:
                            multipleOutput = new[] {propDescriptor.GetParameter(paramIdx)  is TextParameterWrapper tpw3
                                ? tpw3.GetSpriteFromParameter(descriptor.TargetAssetParameter)
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
            return m_originalCommand;
        }

        public string TryFormat(float value, float multiplier) => (value * multiplier).ToString(paramContainer.numberFormat);
        public string TryFormat(long value) => value.ToString(paramContainer.numberFormat);
        public string TryFormat(FormatableString value) => value.GetFormatted(paramContainer.stringFormat);
    }
}
