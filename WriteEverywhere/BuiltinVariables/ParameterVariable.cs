using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class ParameterVariable : WEVariableExtensionRegex
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.Parameter;
        public override string RegexValidValues { get; } = "^[0-9]{1,2}$";

        public override CommandLevel NextLevelByRegex { get; } = CommandLevel.m_endLevel;

        public override string RootMenuDescription { get; } = Str.WTS_PARAMVARS_DESC__COMMON_PARAMNUM;

        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Buildings || renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.PlaceOnNet;

        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (int.TryParse(parameterPath[1], out var idx))
                    {
                        paramContainer.paramIdx = idx;
                        type = VariableType.Parameter;
                        paramContainer.contentType = TextContent.Any;
                    }
                }
                catch { }
            }
        }

        public override string GetTargetTextForNet(TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var paramContainer = wrapper.paramContainer;
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
            return null;
        }

        public override string GetTargetTextForBuilding(TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
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
            return null;
        }
    }
}
