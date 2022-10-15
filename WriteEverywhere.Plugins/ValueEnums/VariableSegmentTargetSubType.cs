using System;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Plugins
{
    public enum VariableSegmentTargetSubType
    {
        None,
        StreetSuffix,
        StreetNameComplete,
        StreetPrefix,
        District,
        Park,
        PostalCode,
        ParkOrDistrict,
        DistrictOrPark,
        MileageMeters,
        MileageKilometers,
        MileageMiles,
        DistrictAreaM2,
        DistrictAreaKm2,
        DistrictAreaMi2,
        ParkAreaM2,
        ParkAreaKm2,
        ParkAreaMi2,
        DistrictPopulation,
        Direction,
        HwCodeShort,
        HwCodeLong,
        HwDettachedPrefix,
        HwIdentifierSuffix,
        DistanceFromReferenceKilometers,
        DistanceFromReferenceMeters,
        DistanceFromReferenceMiles,
    }

    public static class VariableSegmentTargetSubTypeExtensions
    {
        public static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableSegmentTargetSubType)).Cast<VariableSegmentTargetSubType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandLevel();
            }
            return result;
        }
        public static CommandLevel GetCommandLevel(this VariableSegmentTargetSubType var)
        {
            switch (var)
            {
                case VariableSegmentTargetSubType.MileageMeters:
                case VariableSegmentTargetSubType.MileageKilometers:
                case VariableSegmentTargetSubType.MileageMiles:
                case VariableSegmentTargetSubType.DistrictAreaM2:
                case VariableSegmentTargetSubType.DistrictAreaKm2:
                case VariableSegmentTargetSubType.DistrictAreaMi2:
                case VariableSegmentTargetSubType.ParkAreaM2:
                case VariableSegmentTargetSubType.ParkAreaKm2:
                case VariableSegmentTargetSubType.ParkAreaMi2:
                case VariableSegmentTargetSubType.DistanceFromReferenceKilometers:
                case VariableSegmentTargetSubType.DistanceFromReferenceMeters:
                case VariableSegmentTargetSubType.DistanceFromReferenceMiles:
                    return CommandLevel.m_numberFormatFloat;
                case VariableSegmentTargetSubType.DistrictPopulation:
                    return CommandLevel.m_numberFormatInt;
                case VariableSegmentTargetSubType.StreetSuffix:
                case VariableSegmentTargetSubType.StreetNameComplete:
                case VariableSegmentTargetSubType.StreetPrefix:
                case VariableSegmentTargetSubType.District:
                case VariableSegmentTargetSubType.Park:
                case VariableSegmentTargetSubType.ParkOrDistrict:
                case VariableSegmentTargetSubType.DistrictOrPark:
                    return CommandLevel.m_stringFormat;
                case VariableSegmentTargetSubType.PostalCode:
                case VariableSegmentTargetSubType.Direction:
                case VariableSegmentTargetSubType.HwCodeShort:
                case VariableSegmentTargetSubType.HwCodeLong:
                case VariableSegmentTargetSubType.HwDettachedPrefix:
                case VariableSegmentTargetSubType.HwIdentifierSuffix:
                    return CommandLevel.m_appendPrefix;
                default:
                    return null;
            }
        }

        public static bool ReadData(this VariableSegmentTargetSubType var, string[] relativeParams, ref Enum subtype, out VariableExtraParameterContainer extraParams)
        {
            var cmdLevel = var.GetCommandLevel();
            if (cmdLevel is null)
            {
                extraParams = default;
                return false;
            }

            cmdLevel.ParseFormatting(relativeParams, out extraParams);
            subtype = var;
            return true;
        }
    }
}
