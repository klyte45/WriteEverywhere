using ColossalFramework.UI;
using Kwytto.Localization;
using System;
using System.Linq;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Variables;
using WriteEverywhere.Xml;

namespace WriteEverywhere
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
                        case VariableType.VehicleBuilding: return Str.WTS_PARAMVARS_DESC__VariableType_VehicleBuilding;
                        case VariableType.Invalid: return Str.WTS_PARAMVARS_DESC__VariableType_Invalid;
                        case VariableType.Parameter: return Str.WTS_PARAMVARS_DESC__VariableType_Parameter;
                        case VariableType.Environment: return Str.WTS_PARAMVARS_DESC__VariableType_Environment;
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
                case VariableSegmentSubType tp:
                    switch (tp)
                    {
                        case VariableSegmentSubType.None: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_None;
                        case VariableSegmentSubType.StreetSuffix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetSuffix;
                        case VariableSegmentSubType.StreetNameComplete: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetNameComplete;
                        case VariableSegmentSubType.StreetPrefix: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_StreetPrefix;
                        case VariableSegmentSubType.District: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_District;
                        case VariableSegmentSubType.Park: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_Park;
                        case VariableSegmentSubType.ParkOrDistrict: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkOrDistrict;
                        case VariableSegmentSubType.DistrictOrPark: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictOrPark;
                        case VariableSegmentSubType.MileageMeters: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageMeters;
                        case VariableSegmentSubType.MileageKilometers: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageKilometers;
                        case VariableSegmentSubType.MileageMiles: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_MileageMiles;
                        case VariableSegmentSubType.DistrictAreaM2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaM2;
                        case VariableSegmentSubType.DistrictAreaKm2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaKm2;
                        case VariableSegmentSubType.DistrictAreaMi2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictAreaMi2;
                        case VariableSegmentSubType.ParkAreaM2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaM2;
                        case VariableSegmentSubType.ParkAreaKm2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaKm2;
                        case VariableSegmentSubType.ParkAreaMi2: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_ParkAreaMi2;
                        case VariableSegmentSubType.DistrictPopulation: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_DistrictPopulation;
                        case VariableSegmentSubType.Direction: return Str.WTS_PARAMVARS_DESC__VariableSegmentTargetSubType_Direction;
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
                        case TextContent.TextParameterSequence: return Str.WTS_BOARD_TEXT_TYPE_DESC__TextParameterSequence;
                        case TextContent.Any: return Str.WTS_BOARD_TEXT_TYPE_DESC__Any;
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
                case ConfigurationSource.NONE: return Str.we_Enum__ConfigurationSource_NONE;
                case ConfigurationSource.ASSET: return Str.we_Enum__ConfigurationSource_ASSET;
                case ConfigurationSource.GLOBAL: return Str.we_Enum__ConfigurationSource_GLOBAL;
                case ConfigurationSource.CITY: return Str.we_Enum__ConfigurationSource_CITY;
                case ConfigurationSource.SKIN: return Str.we_Enum__ConfigurationSource_SKIN;
                case YCloneType.None: return Str.we_Enum__YCloneType_None;
                case YCloneType.OnX: return Str.we_Enum__YCloneType_OnX;
                case YCloneType.OnZ: return Str.we_Enum__YCloneType_OnZ;
                case ColoringSource.Fixed: return Str.we_Enum__ColoringSource_Fixed;
                case ColoringSource.ContrastProp: return Str.we_Enum__ColoringSource_ContrastProp;
                case ColoringSource.PlatformLine: return Str.we_Enum__ColoringSource_PlatformLine;
                case ColoringSource.ContrastPlatformLine: return Str.we_Enum__ColoringSource_ContrastPlatformLine;
                case TextRenderingClass.None: return Str.we_Enum__TextRenderingClass_None;
                case TextRenderingClass.RoadNodes: return Str.we_Enum__TextRenderingClass_RoadNodes;
                case TextRenderingClass.Buildings: return Str.we_Enum__TextRenderingClass_Buildings;
                case TextRenderingClass.PlaceOnNet: return Str.we_Enum__TextRenderingClass_PlaceOnNet;
                case TextRenderingClass.Vehicle: return Str.we_Enum__TextRenderingClass_Vehicle;
                case TextRenderingClass.Any: return Str.we_Enum__TextRenderingClass_Any;
                case TextRenderingClass.BgMesh: return Str.we_Enum__TextRenderingClass_BgMesh;

                case PivotPosition.Left: return Str.we_PivotPosition__Left;
                case PivotPosition.Right: return Str.we_PivotPosition__Right;
                case PivotPosition.Center: return Str.we_PivotPosition__Center;
                case PivotPosition.LeftInvert: return Str.we_PivotPosition__LeftInvert;
                case PivotPosition.RightInvert: return Str.we_PivotPosition__RightInvert;
                case PivotPosition.CenterInvert: return Str.we_PivotPosition__CenterInvert;
                case PivotPosition.CenterLookingLeft: return Str.we_PivotPosition__CenterLookingLeft;
                case PivotPosition.CenterLookingRight: return Str.we_PivotPosition__CenterLookingRight;

                case VariableEnvironmentSubType.Clock: return Str.we_Enum__VariableEnvironmentSubType_Clock;
                case VariableEnvironmentSubType.Temperature: return Str.we_Enum__VariableEnvironmentSubType_Temperature;
                case VariableEnvironmentSubType.CustomFormattedDate: return Str.we_Enum__VariableEnvironmentSubType_CustomFormattedDate;


            }
            return variable.ValueToI18nKwytto();
        }

        public static string[] GetAllValuesI18n<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<Enum>().Select(x => x.ValueToI18n()).ToArray();
    }
}
