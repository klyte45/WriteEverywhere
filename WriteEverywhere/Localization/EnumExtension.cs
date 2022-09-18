using ColossalFramework.UI;
using FontStashSharp;
using System;
using System.Linq;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Localization
{
    internal static class EnumI18nExtensions
    {
        public static string ValueToI18n(this Enum variable)
        {
            switch (variable)
            {
                case VariableType tp:
                    switch (tp)
                    {
                        case VariableType.SegmentTarget: return Str.WTS_PARAMVARS_DESC__VariableType_SegmentTarget;
                        case VariableType.CityData: return Str.WTS_PARAMVARS_DESC__VariableType_CityData;
                        case VariableType.CurrentBuilding: return Str.WTS_PARAMVARS_DESC__VariableType_CurrentBuilding;
                        case VariableType.CurrentSegment: return Str.WTS_PARAMVARS_DESC__VariableType_CurrentSegment;
                        case VariableType.CurrentVehicle: return Str.WTS_PARAMVARS_DESC__VariableType_CurrentVehicle;
                        case VariableType.Invalid: return Str.WTS_PARAMVARS_DESC__VariableType_Invalid;
                    }
                    break;
                case VariableCitySubType tp:
                    switch (tp)
                    {
                        case VariableCitySubType.None: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_None;
                        case VariableCitySubType.CityName: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_CityName;
                        case VariableCitySubType.CityPopulation: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_CityPopulation;
                    }
                    break;
                case VariableSegmentTargetSubType tp:
                    switch (tp)
                    {
                        case VariableSegmentTargetSubType.None: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_None;
                        case VariableSegmentTargetSubType.StreetSuffix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetSuffix;
                        case VariableSegmentTargetSubType.StreetNameComplete: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetNameComplete;
                        case VariableSegmentTargetSubType.StreetPrefix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetPrefix;
                        case VariableSegmentTargetSubType.District: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_District;
                        case VariableSegmentTargetSubType.Park: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_Park;
                        case VariableSegmentTargetSubType.PostalCode: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_PostalCode;
                        case VariableSegmentTargetSubType.ParkOrDistrict: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkOrDistrict;
                        case VariableSegmentTargetSubType.DistrictOrPark: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictOrPark;
                        case VariableSegmentTargetSubType.MileageMeters: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageMeters;
                        case VariableSegmentTargetSubType.MileageKilometers: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageKilometers;
                        case VariableSegmentTargetSubType.MileageMiles: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageMiles;
                        case VariableSegmentTargetSubType.DistrictAreaM2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaM2;
                        case VariableSegmentTargetSubType.DistrictAreaKm2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaKm2;
                        case VariableSegmentTargetSubType.DistrictAreaMi2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaMi2;
                        case VariableSegmentTargetSubType.ParkAreaM2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaM2;
                        case VariableSegmentTargetSubType.ParkAreaKm2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaKm2;
                        case VariableSegmentTargetSubType.ParkAreaMi2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaMi2;
                        case VariableSegmentTargetSubType.DistrictPopulation: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictPopulation;
                        case VariableSegmentTargetSubType.Direction: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_Direction;
                        case VariableSegmentTargetSubType.HwCodeShort: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_HwCodeShort;
                        case VariableSegmentTargetSubType.HwCodeLong: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_HwCodeLong;
                        case VariableSegmentTargetSubType.HwDettachedPrefix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_HwDettachedPrefix;
                        case VariableSegmentTargetSubType.HwIdentifierSuffix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_HwIdentifierSuffix;
                        case VariableSegmentTargetSubType.DistanceFromReferenceKilometers: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistanceFromReferenceKilometers;
                        case VariableSegmentTargetSubType.DistanceFromReferenceMeters: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistanceFromReferenceMeters;
                        case VariableSegmentTargetSubType.DistanceFromReferenceMiles: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistanceFromReferenceMiles;
                    }
                    break;
                case VariableVehicleSubType tp:
                    switch (tp)
                    {
                        case VariableVehicleSubType.None: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_None;
                        case VariableVehicleSubType.OwnNumber: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_OwnNumber;
                        case VariableVehicleSubType.LineIdentifier: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_LineIdentifier;
                        case VariableVehicleSubType.NextStopLine: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_NextStopLine;
                        case VariableVehicleSubType.PrevStopLine: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_PrevStopLine;
                        case VariableVehicleSubType.LastStopLine: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_LastStopLine;
                        case VariableVehicleSubType.LineFullName: return Str.WTS_PARAMVARS_DESC__VariableVehicleSubType_LineFullName;
                    }
                    break;
                case UIHorizontalAlignment x:
                    switch (x)
                    {
                        case UIHorizontalAlignment.Center: return Str.we_Enum__UIHorizontalAlignment_Center;
                        case UIHorizontalAlignment.Right: return Str.we_Enum__UIHorizontalAlignment_Right;
                        case UIHorizontalAlignment.Left: return Str.we_Enum__UIHorizontalAlignment_Left;
                    }
                    break;
                case FontClass x:
                    switch (x)
                    {
                        case FontClass.Regular: return Str.we_Enum__FontClass_Regular;
                        case FontClass.PublicTransport: return Str.we_Enum__FontClass_PublicTransport;
                        case FontClass.ElectronicBoards: return Str.we_Enum__FontClass_ElectronicBoards;
                        case FontClass.Stencil: return Str.we_Enum__FontClass_Stencil;
                        case FontClass.HighwayShields: return Str.we_Enum__FontClass_HighwayShields;
                    }
                    break;
                case TextContent content:
                    switch (content)
                    {
                        case TextContent.None: return Str.WTS_BOARD_TEXT_TYPE_DESC__None;
                        case TextContent.ParameterizedText: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedText;
                        case TextContent.ParameterizedSpriteFolder: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedSpriteFolder;
                        case TextContent.ParameterizedSpriteSingle: return Str.WTS_BOARD_TEXT_TYPE_DESC__ParameterizedSpriteSingle;
                        case TextContent.LinesSymbols: return Str.WTS_BOARD_TEXT_TYPE_DESC__LinesSymbols;
                        case TextContent.LinesNameList: return Str.WTS_BOARD_TEXT_TYPE_DESC__LinesNameList;
                        case TextContent.HwShield: return Str.WTS_BOARD_TEXT_TYPE_DESC__HwShield;
                        case TextContent.TimeTemperature: return Str.WTS_BOARD_TEXT_TYPE_DESC__TimeTemperature;
                        case TextContent.TextParameterSequence: return Str.WTS_BOARD_TEXT_TYPE_DESC__TextParameterSequence;
                    }
                    break;
                case MaterialType x:
                    switch (x)
                    {
                        case MaterialType.OPAQUE: return Str.we_Enum__MaterialType_OPAQUE;
                        case MaterialType.DAYNIGHT: return Str.we_Enum__MaterialType_DAYNIGHT;
                        case MaterialType.FLAGS: return Str.we_Enum__MaterialType_FLAGS;
                        case MaterialType.BRIGHT: return Str.we_Enum__MaterialType_BRIGHT;
                    }
                    break;

                case BlinkType x:
                    switch (x)
                    {
                        case BlinkType.None: return Str.we_Enum__BlinkType_None;
                        case BlinkType.Blink_050_050: return Str.we_Enum__BlinkType_Blink_050_050;
                        case BlinkType.MildFade_0125_0125: return Str.we_Enum__BlinkType_MildFade_0125_0125;
                        case BlinkType.MediumFade_500_500: return Str.we_Enum__BlinkType_MediumFade_500_500;
                        case BlinkType.StrongBlaze_0125_0125: return Str.we_Enum__BlinkType_StrongBlaze_0125_0125;
                        case BlinkType.StrongFade_250_250: return Str.we_Enum__BlinkType_StrongFade_250_250;
                        case BlinkType.Blink_025_025: return Str.we_Enum__BlinkType_Blink_025_025;
                        case BlinkType.Custom: return Str.we_Enum__BlinkType_Custom;
                    }
                    break;
            }
            return $"{variable?.GetType()}|{variable}";
        }

        public static string[] GetAllValuesI18n<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<Enum>().Select(x => x.ValueToI18n()).ToArray();
    }
}
