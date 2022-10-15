extern alias TLM;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using WriteEverywhere.Data;
using WriteEverywhere.Font;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{
    internal static class TextParameterWrapperRendering
    {
        #region renderingInfo

        internal static DynamicSpriteFont GetTargetFont(WriteOnBuildingXml propGroup, BaseWriteOnXml propDesc, TextToWriteOnXml textDesc)
            => FontServer.instance.FirstOf(new Func<string>[]
            {
                    ()=>   textDesc.m_overrideFont,
                    ()=>   WTSEtcData.Instance.FontSettings.GetTargetFont(textDesc.m_fontClass),
                    ()=>   propDesc.DescriptorOverrideFont,
                    ()=>   propGroup?.FontName,
                    ()=>   WTSEtcData.Instance.FontSettings.GetTargetFont(propDesc.RenderingClass),
                    ()=>   WEMainController.DEFAULT_FONT_KEY,
            });

        internal static BasicRenderInformation GetRenderInfo(WriteOnBuildingXml propGroup, BaseWriteOnXml propDesc, TextToWriteOnXml textDescriptor, ushort refId, int secIdx, int tercIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                    return null;
                case TextContent.ParameterizedText:
                    return textDescriptor.Value?.GetTargetText(propGroup, propDesc, textDescriptor, GetTargetFont(propGroup, propDesc, textDescriptor), refId, secIdx, tercIdx, out multipleOutput);
                case TextContent.ParameterizedSpriteFolder:
                    return textDescriptor.Value?.GetSpriteFromCycle(textDescriptor, propDesc.TargetAssetParameter, refId, secIdx, tercIdx);
                case TextContent.ParameterizedSpriteSingle:
                    return textDescriptor.Value?.GetSpriteFromParameter(propDesc.TargetAssetParameter);
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


        internal static BasicRenderInformation GetRenderInfo(WriteOnBuildingXml propGroupDescriptor, TextParameterWrapper tpw, BaseWriteOnXml instance, TextToWriteOnXml textDescriptor, ushort refId, int secIdx, int tercIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                    return null;
                case TextContent.ParameterizedText:
                Text:
                    return !(tpw is null)
                        ? tpw.GetTargetText(propGroupDescriptor, instance, textDescriptor, GetTargetFont(propGroupDescriptor, instance, textDescriptor), refId, secIdx, tercIdx, out multipleOutput)
                        : GetTargetFont(propGroupDescriptor, instance, textDescriptor).DrawString(ModInstance.Controller, $"<PARAM IS NOT SET>", default, FontServer.instance.ScaleEffective);
                case TextContent.ParameterizedSpriteFolder:
                ImageFolder:
                    return !(tpw is null)
                        ? tpw.GetSpriteFromCycle(textDescriptor, instance.TargetAssetParameter, refId, secIdx, tercIdx)
                        : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet");
                case TextContent.ParameterizedSpriteSingle:
                ImageSingle:
                    return !(tpw is null)
                        ? tpw.GetSpriteFromParameter(instance.TargetAssetParameter)
                        : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet");
                case TextContent.Any:
                case TextContent.TextParameterSequence:
                    if (!(tpw is null))
                    {
                        switch (tpw.ParamType)
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
                    return GetTargetFont(propGroupDescriptor, instance, textDescriptor).DrawString(ModInstance.Controller, $"<SEQ PARAM IS NOT SET>", default, FontServer.instance.ScaleEffective); ;
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

        public static BasicRenderInformation GetSpriteFromParameter(this TextParameterWrapper wrapper, PrefabInfo prop) => wrapper.IsEmpty ? null : wrapper.GetImageBRI(prop);
        public static BasicRenderInformation GetImageBRI(this TextParameterWrapper wrapper, PrefabInfo prefab)
        {
            if (wrapper.ParamType == ParameterType.IMAGE)
            {
                if (wrapper.IsLocal)
                {
                    return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(wrapper.AtlasName, wrapper.TextOrSpriteValue, true);
                }
                else
                {
                    var cachedId = wrapper.GetCachedPrefab(prefab);
                    return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(cachedId.ToString(), wrapper.TextOrSpriteValue, cachedId == default) ?? ModInstance.Controller.AtlasesLibrary.GetFromAssetAtlases(cachedId, wrapper.TextOrSpriteValue, true);
                }
            }
            else
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsImageRequired");
            }

        }

        internal static BasicRenderInformation GetTargetText(this TextParameterWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, BaseWriteOnXml descriptorBuilding, TextToWriteOnXml textDescriptor, DynamicSpriteFont targetFont, ushort refId, int secId, int tercId, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            if (wrapper.ParamType != ParameterType.VARIABLE)
            {
                multipleOutput = null;
                return targetFont?.DrawString(ModInstance.Controller, wrapper.ToString(), default, FontServer.instance.ScaleEffective);
            }
            else
            {
                return wrapper.VariableValue.GetTargetText(propGroupDescriptor, descriptorBuilding, textDescriptor, targetFont, refId, secId, tercId, out multipleOutput);
            }
        }

        public static string GetOriginalVariableParam(this TextParameterWrapper wrapper) => wrapper.ParamType != ParameterType.VARIABLE ? null : wrapper.OriginalCommand;


        internal static BasicRenderInformation GetSpriteFromCycle(this TextParameterWrapper wrapper, TextToWriteOnXml textDescriptor, PrefabInfo cachedPrefab, ushort refId, int boardIdx, int secIdx)
        {
            if (wrapper.IsEmpty)
            {
                return null;
            }
            if (wrapper.ParamType != ParameterType.FOLDER)
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsFolderRequired");
            }
            if (textDescriptor.AnimationSettings.m_itemCycleFramesDuration < 1)
            {
                textDescriptor.AnimationSettings.m_itemCycleFramesDuration = 100;
            }
            return wrapper.GetCurrentSprite(cachedPrefab, (int length) => (int)(((SimulationManager.instance.m_currentFrameIndex + textDescriptor.AnimationSettings.m_extraDelayCycleFrames + (refId * (1 + boardIdx) + (11345476 * secIdx))) % (length * textDescriptor.AnimationSettings.m_itemCycleFramesDuration) / textDescriptor.AnimationSettings.m_itemCycleFramesDuration)));
        }
        internal static BasicRenderInformation GetCurrentSprite(this TextParameterWrapper wrapper, PrefabInfo prefab, Func<int, int> p)
        {
            if (wrapper.ParamType == ParameterType.FOLDER)
            {
                if (wrapper.IsLocal)
                {
                    return ModInstance.Controller.AtlasesLibrary.GetSlideFromLocal(wrapper.AtlasName, p, true);
                }
                else
                {
                    var cachedId = wrapper.GetCachedPrefab(prefab);
                    return ModInstance.Controller.AtlasesLibrary.GetSlideFromLocal(cachedId.ToString(), p, cachedId == default) ?? ModInstance.Controller.AtlasesLibrary.GetSlideFromAsset(cachedId, p, true);
                }
            }
            else
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsFolderRequired");
            }

        }
        #endregion
    }
}
