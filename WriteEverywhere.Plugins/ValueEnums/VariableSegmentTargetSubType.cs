using System;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Plugins
{
    public enum VariableSegmentSubType
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
            foreach (var value in Enum.GetValues(typeof(VariableSegmentSubType)).Cast<VariableSegmentSubType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandLevel();
            }
            return result;
        }
        public static CommandLevel GetCommandLevel(this VariableSegmentSubType var)
        {
            switch (var)
            {
                case VariableSegmentSubType.MileageMeters:
                case VariableSegmentSubType.MileageKilometers:
                case VariableSegmentSubType.MileageMiles:
                case VariableSegmentSubType.DistrictAreaM2:
                case VariableSegmentSubType.DistrictAreaKm2:
                case VariableSegmentSubType.DistrictAreaMi2:
                case VariableSegmentSubType.ParkAreaM2:
                case VariableSegmentSubType.ParkAreaKm2:
                case VariableSegmentSubType.ParkAreaMi2:
                case VariableSegmentSubType.DistanceFromReferenceKilometers:
                case VariableSegmentSubType.DistanceFromReferenceMeters:
                case VariableSegmentSubType.DistanceFromReferenceMiles:
                    return CommandLevel.m_numberFormatFloat;
                case VariableSegmentSubType.DistrictPopulation:
                    return CommandLevel.m_numberFormatInt;
                case VariableSegmentSubType.StreetSuffix:
                case VariableSegmentSubType.StreetNameComplete:
                case VariableSegmentSubType.StreetPrefix:
                case VariableSegmentSubType.District:
                case VariableSegmentSubType.Park:
                case VariableSegmentSubType.ParkOrDistrict:
                case VariableSegmentSubType.DistrictOrPark:
                    return CommandLevel.m_stringFormat;
                case VariableSegmentSubType.PostalCode:
                case VariableSegmentSubType.Direction:
                case VariableSegmentSubType.HwCodeShort:
                case VariableSegmentSubType.HwCodeLong:
                case VariableSegmentSubType.HwDettachedPrefix:
                case VariableSegmentSubType.HwIdentifierSuffix:
                    return CommandLevel.m_appendPrefix;
                default:
                    return null;
            }
        }

        public static bool ReadData(this VariableSegmentSubType var, string[] relativeParams, ref Enum subtype, out VariableExtraParameterContainer extraParams)
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
