extern alias TLM;
extern alias VS;

using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System.Collections.Generic;
using System.Linq;
using VS::Bridge_WE2VS;
using WriteEverywhere.Data;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Rendering
{

    public static class WTSTextMeshProcess
    {
        internal static BasicRenderInformation GetTextMesh(BoardTextDescriptorGeneralXml textDescriptor, ushort refID, int boardIdx, int secIdx, BaseWriteOnXml instance)
        {
            DynamicSpriteFont baseFont = FontServer.instance[WTSEtcData.Instance.FontSettings.GetTargetFont(textDescriptor.m_fontClass)] ?? FontServer.instance[instance.DescriptorOverrideFont];

            //if (instance is BoardPreviewInstanceXml preview)
            //{
            //    return GetTextForPreview(textDescriptor, propLayout, ref multipleOutput, ref baseFont, preview, refPrefab);
            //}
            //else
            if (instance is LayoutDescriptorVehicleXml vehicleDescriptor)
            {
                return GetTextForVehicle(textDescriptor, refID, boardIdx, secIdx, ref baseFont, vehicleDescriptor);
            }
            else
            //if (instance is BoardInstanceBuildingXml buildingDescritpor)
            //{
            //    return GetTextForBuilding(textDescriptor, refID, boardIdx, secIdx, ref multipleOutput, ref baseFont, buildingDescritpor);
            //}
            //else if (instance is BoardInstanceRoadNodeXml)
            //{
            //    return GetTextForRoadNode(textDescriptor, refID, boardIdx, secIdx, ref baseFont);
            //}
            //else 
            if (instance is OnNetInstanceCacheContainerXml onNet)
            {
                return GetTextForOnNet(textDescriptor, refID, boardIdx, secIdx, onNet, ref baseFont, out var multipleOutput) ?? multipleOutput?.FirstOrDefault();
            }
            return WTSCacheSingleton.GetTextData(textDescriptor.Value?.ToString() ?? "", textDescriptor.m_prefix, textDescriptor.m_suffix, null, textDescriptor.m_overrideFont);
        }

        //private static BasicRenderInformation GetTextForRoadNode(BoardTextDescriptorGeneralXml textDescriptor, ushort refID, int boardIdx, int secIdx, ref DynamicSpriteFont baseFont)
        //{
        //    CacheRoadNodeItem data = WTSRoadNodesData.Instance.BoardsContainers[refID, boardIdx, secIdx];
        //    if (data == null)
        //    {
        //        return null;
        //    }

        //    if (baseFont == null)
        //    {
        //        baseFont = FontServer.instance[WTSRoadNodesData.Instance.DefaultFont];
        //    }

        //    var targetType = textDescriptor.OLD__TextType;
        //    switch (targetType)
        //    {
        //        case TextType.ParkOrDistrict: targetType = data.m_districtParkId > 0 ? TextType.Park : TextType.District; break;
        //        case TextType.DistrictOrPark: targetType = data.m_districtId == 0 && data.m_districtParkId > 0 ? TextType.Park : TextType.District; break;
        //        case TextType.Park:
        //            if (data.m_districtParkId == 0)
        //            {
        //                return null;
        //            }
        //            break;
        //    }
        //    switch (targetType)
        //    {
        //        case TextType.GameSprite: return GetSpriteFromParameter(data.m_currentDescriptor.Descriptor.CachedProp, textDescriptor.m_spriteParam);
        //        case TextType.DistanceFromReference: return WTSCacheSingleton.GetTextData($"{data.m_distanceRefKm}", textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        //        case TextType.Fixed: return WTSCacheSingleton.GetTextData(textDescriptor.m_fixedText ?? "", textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        //        case TextType.StreetSuffix: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(data.m_segmentId).StreetName, baseFont);
        //        case TextType.StreetNameComplete: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(data.m_segmentId).FullStreetName, baseFont);
        //        case TextType.StreetPrefix: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(data.m_segmentId).StreetQualifier, baseFont);
        //        case TextType.District: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetDistrict(data.m_districtId).Name, baseFont);
        //        case TextType.Park: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetPark(data.m_districtParkId).Name, baseFont);
        //        case TextType.PostalCode: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(data.m_segmentId).PostalCode, baseFont);
        //        case TextType.CityName: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetDistrict(0).Name, baseFont);
        //        default: return null;
        //    };
        //}


        private static BasicRenderInformation GetTextForVehicle(BoardTextDescriptorGeneralXml textDescriptor, ushort refID, int boardIdx, int secIdx, ref DynamicSpriteFont baseFont, LayoutDescriptorVehicleXml vehicleDescriptor)
        {
            if (baseFont is null)
            {
                baseFont = FontServer.instance[vehicleDescriptor.FontName] ?? FontServer.instance[WTSVehicleData.Instance.DefaultFont];
            }
            switch (textDescriptor.textContent)
            {
                case TextContent.ParameterizedText:
                case TextContent.ParameterizedSpriteFolder:
                case TextContent.ParameterizedSpriteSingle:
                    return TextParameterWrapper.GetRenderInfo(vehicleDescriptor, textDescriptor, refID, boardIdx, secIdx, out _);
                case TextContent.LinesSymbols:
                    break;
                case TextContent.LinesNameList:
                    break;
                case TextContent.HwShield:
                    break;
                case TextContent.TimeTemperature: return GetTimeTemperatureText(textDescriptor, ref baseFont, refID, boardIdx, secIdx);
                case TextContent.TextParameterSequence:
                    return TextParameterWrapper.GetRenderInfo(textDescriptor.ParameterSequence?.GetAt(SimulationManager.instance.m_referenceFrameIndex, refID), vehicleDescriptor, textDescriptor, refID, boardIdx, secIdx, out _);
            }

            return null;
        }



        //private static BasicRenderInformation GetTextForPreview(BoardTextDescriptorGeneralXml textDescriptor, BoardDescriptorGeneralXml propLayout, ref IEnumerable<BasicRenderInformation> multipleOutput, ref DynamicSpriteFont baseFont, BoardPreviewInstanceXml preview, PrefabInfo refInfo)
        //{
        //    if (baseFont is null)
        //    {
        //        switch (propLayout?.m_allowedRenderClass)
        //        {
        //            case TextRenderingClass.RoadNodes:
        //                baseFont = FontServer.instance[WTSRoadNodesData.Instance.DefaultFont];
        //                break;
        //            case TextRenderingClass.Buildings:
        //                baseFont = FontServer.instance[WTSBuildingsData.Instance.DefaultFont];
        //                break;
        //            case TextRenderingClass.PlaceOnNet:
        //                baseFont = FontServer.instance[WTSOnNetData.Instance.DefaultFont];
        //                break;
        //            case null:
        //            case TextRenderingClass.Vehicle:
        //                baseFont = FontServer.instance[WTSVehicleData.Instance.DefaultFont];
        //                break;
        //        }
        //    }

        //    if (!preview.m_overrideText.IsNullOrWhiteSpace() && !textDescriptor.IsSpriteText())
        //    {
        //        return WTSCacheSingleton.GetTextData(preview.m_overrideText, "", "", baseFont, textDescriptor.m_overrideFont);
        //    }

        //    string otherText = "";
        //    if (textDescriptor.IsTextRelativeToSegment())
        //    {
        //        otherText = $"({textDescriptor.m_destinationRelative}) ";
        //    }

        //    switch (textDescriptor.textContent)
        //    {
        //        case TextContent.None:
        //            return LegacyInfoPreview(textDescriptor, ref multipleOutput, baseFont, preview, otherText);
        //        case TextContent.ParameterizedText:
        //        case TextContent.ParameterizedSpriteFolder:
        //        case TextContent.ParameterizedSpriteSingle:
        //            return TextParameterWrapper.GetRenderInfo(preview, textDescriptor, 0, 0, 0, out multipleOutput);
        //        case TextContent.LinesSymbols:
        //            multipleOutput = ModInstance.Controller.AtlasesLibrary.DrawLineFormats(new WTSLine[textDescriptor.MultiItemSettings.SubItemsPerColumn * textDescriptor.MultiItemSettings.SubItemsPerRow].Select((x, y) => new WTSLine((ushort)y, false, true)));
        //            return null;
        //        case TextContent.HwShield:
        //            return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "K45WTS FrameBorder");
        //        case TextContent.TimeTemperature:
        //            return WTSCacheSingleton.GetTextData(ModInstance.Clock12hFormat ? "12:60AM" : "24:60", textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        //        case TextContent.TextParameterSequence:
        //            return TextParameterWrapper.GetRenderInfo(textDescriptor.ParameterSequence?.GetAt(SimulationManager.instance.m_referenceFrameIndex, 0), preview, textDescriptor, 0, 0, 0, out _);

        //    }
        //    return null;
        //}

        private static BasicRenderInformation GetTextForOnNet(BoardTextDescriptorGeneralXml textDescriptor, ushort segmentId, int boardIdx, int secIdx, OnNetInstanceCacheContainerXml propDescriptor, ref DynamicSpriteFont baseFont, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var data = WTSOnNetData.Instance.m_boardsContainers[segmentId];
            if (data == null)
            {
                return null;
            }
            if (baseFont is null)
            {
                baseFont = FontServer.instance[WTSOnNetData.Instance.DefaultFont];
            }
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    break;
                case TextContent.ParameterizedText:
                case TextContent.ParameterizedSpriteFolder:
                case TextContent.ParameterizedSpriteSingle:
                    return TextParameterWrapper.GetRenderInfo(propDescriptor, textDescriptor, segmentId, boardIdx, secIdx, out multipleOutput);
                case TextContent.LinesSymbols:
                    break;
                case TextContent.LinesNameList:
                    break;
                case TextContent.HwShield:
                    break;
                case TextContent.TimeTemperature: return GetTimeTemperatureText(textDescriptor, ref baseFont, segmentId, boardIdx, secIdx);
                case TextContent.TextParameterSequence:
                    return TextParameterWrapper.GetRenderInfo(textDescriptor.ParameterSequence?.GetAt(SimulationManager.instance.m_referenceFrameIndex, segmentId), propDescriptor, textDescriptor, segmentId, boardIdx, secIdx, out multipleOutput);
            }
            return null;
        }


        private static BasicRenderInformation GetTimeTemperatureText(BoardTextDescriptorGeneralXml textDescriptor, ref DynamicSpriteFont baseFont, ushort refId, int boardIdx, int secIdx)
        {
            if ((SimulationManager.instance.m_currentFrameIndex + (refId * (1 + boardIdx) + (11345476 * secIdx))) % 760 < 380)
            {
                var time = SimulationManager.instance.m_currentDayTimeHour;
                if (ModInstance.Clock12hFormat)
                {
                    time = ((time + 11) % 12) + 1;
                }
                var precision = ModInstance.ClockPrecision.value;
                return WTSCacheSingleton.GetTextData($"{((int)time).ToString($"D{(ModInstance.ClockShowLeadingZero ? "2" : "1")}")}:{((int)(((int)(time % 1 * 60 / precision)) * precision)).ToString("D2")}", textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
            }
            else
            {
                return WTSCacheSingleton.GetTextData(WTSEtcData.FormatTemp(WeatherManager.instance.m_currentTemperature), textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
            }
        }
        private static BasicRenderInformation GetSpriteFromParameter(PrefabInfo prop, TextParameterWrapper param)
            => param is null
                ? ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "K45WTS FrameParamsNotSet")
                : param.IsEmpty
                    ? null
                    : param.GetImageBRI(prop);

        internal static BasicRenderInformation GetFromCacheArray(BoardTextDescriptorGeneralXml textDescriptor, string text, DynamicSpriteFont baseFont) => text is null ? null : WTSCacheSingleton.GetTextData(text, textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        internal static BasicRenderInformation GetFromCacheArray(BoardTextDescriptorGeneralXml textDescriptor, FormatableString text, DynamicSpriteFont baseFont) => text is null ? null : WTSCacheSingleton.GetTextData(text.Get(textDescriptor.m_allCaps, textDescriptor.m_applyAbbreviations), textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        internal static BasicRenderInformation GetFromCacheArray(BoardTextDescriptorGeneralXml textDescriptor, int value, string mask, DynamicSpriteFont baseFont) => WTSCacheSingleton.GetTextData(value.ToString(mask), textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        internal static BasicRenderInformation GetFromCacheArray(BoardTextDescriptorGeneralXml textDescriptor, float value, string mask, DynamicSpriteFont baseFont) => WTSCacheSingleton.GetTextData(value.ToString(mask), textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);

        #region Legacy

        //private static BasicRenderInformation GetTextForBuilding(BoardTextDescriptorGeneralXml textDescriptor, ushort refID, int boardIdx, int secIdx, ref IEnumerable<BasicRenderInformation> multipleOutput, ref DynamicSpriteFont baseFontRef, BoardInstanceBuildingXml buildingDescritpor)
        //{
        //    ref BoardBunchContainerBuilding data = ref WTSBuildingsData.Instance.BoardsContainers[refID, 0, 0][boardIdx];
        //    if (data == null)
        //    {
        //        return null;
        //    }
        //    var baseFont = baseFontRef ?? FontServer.instance[WTSBuildingsData.Instance.DefaultFont];
        //    var targetType = textDescriptor.OLD__TextType;
        //    switch (targetType)
        //    {
        //        case TextType.GameSprite: return GetSpriteFromParameter(buildingDescritpor.Descriptor.CachedProp, textDescriptor.m_spriteParam);
        //        case TextType.Fixed: return WTSCacheSingleton.GetTextData(textDescriptor.m_fixedText ?? "", textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        //        case TextType.OwnName: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetBuilding(refID).Name, baseFont);
        //        case TextType.NextStopLine: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetBuilding(WTSStopUtils.GetTargetStopInfo(buildingDescritpor, refID).FirstOrDefault().NextStopBuildingId).Name, baseFont);
        //        case TextType.PrevStopLine: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetBuilding(WTSStopUtils.GetTargetStopInfo(buildingDescritpor, refID).FirstOrDefault().PrevStopBuildingId).Name, baseFont);
        //        case TextType.LastStopLine: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetBuilding(WTSStopUtils.GetTargetStopInfo(buildingDescritpor, refID).FirstOrDefault().DestinationBuildingId).Name, baseFont);
        //        case TextType.StreetPrefix: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(WTSCacheSingleton.instance.GetBuilding(refID).SegmentId).StreetQualifier, baseFont);
        //        case TextType.StreetSuffix: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(WTSCacheSingleton.instance.GetBuilding(refID).SegmentId).StreetName, baseFont);
        //        case TextType.StreetNameComplete: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetSegment(WTSCacheSingleton.instance.GetBuilding(refID).SegmentId).FullStreetName, baseFont);
        //        case TextType.PlatformNumber: return WTSCacheSingleton.GetTextData((buildingDescritpor.m_platforms.FirstOrDefault() + 1).ToString(), textDescriptor.m_prefix, textDescriptor.m_suffix, baseFont, textDescriptor.m_overrideFont);
        //        case TextType.TimeTemperature: return GetTimeTemperatureText(textDescriptor, ref baseFont, refID, boardIdx, 0);
        //        case TextType.CityName: return GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetDistrict(0).Name, baseFont);
        //        case TextType.LinesSymbols:
        //            multipleOutput = ModInstance.Controller.AtlasesLibrary.DrawLineFormats(WTSStopUtils.GetAllTargetStopInfo(buildingDescritpor, refID).GroupBy(x => x.m_lineId).Select(x => x.First()).Select(x => new WTSLine(x.m_lineId, x.m_regionalLine)));
        //            return null;
        //        case TextType.LineFullName:
        //            multipleOutput = WTSStopUtils.GetAllTargetStopInfo(buildingDescritpor, refID).GroupBy(x => x.m_lineId).Select(x => x.First()).Select(x => GetFromCacheArray(textDescriptor, WTSCacheSingleton.instance.GetCityTransportLine(x.m_lineId).Name, baseFont));
        //            return null;
        //        case TextType.ParameterizedText:
        //        case TextType.ParameterizedGameSprite:
        //        case TextType.ParameterizedGameSpriteIndexed:
        //            return TextParameterWrapper.GetRenderInfo(buildingDescritpor, textDescriptor, refID, boardIdx, secIdx, out multipleOutput);
        //        default:
        //            return null;
        //    }
        //}


        #endregion


    }


}
