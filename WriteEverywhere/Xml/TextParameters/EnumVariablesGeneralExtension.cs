using Klyte.Localization;
using System;

namespace WriteEverywhere.Xml
{
    internal static class EnumVariablesGeneralExtension
    {
        public static string VariableToI18n(this Enum variable)
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
                        default: return $"{tp.GetType()}|{tp}";
                    }
                case VariableCitySubType tp:
                    switch (tp)
                    {
                        case VariableCitySubType.None: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_None;
                        case VariableCitySubType.CityName: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_CityName;
                        case VariableCitySubType.CityPopulation: return Str.WTS_PARAMVARS_DESC__VariableCitySubType_CityPopulation;
                        default: return $"{tp.GetType()}|{tp}";
                    }
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
                        default: return $"{tp.GetType()}|{tp}";
                    }
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
                        default: return $"{tp.GetType()}|{tp}";
                    }
                default: return $"{variable?.GetType()}|{variable}";

            }
        }
    }
}
