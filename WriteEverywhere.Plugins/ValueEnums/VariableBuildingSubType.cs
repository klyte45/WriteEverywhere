
using System;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Plugins
{
    public enum VariableBuildingSubType
    {
        None,
        OwnName,
        NextStopLine,
        PrevStopLine,
        LastStopLine,
        PlatformNumber
    }

    public static class VariableBuildingSubTypeExtensions
    {
        public static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableBuildingSubType)).Cast<VariableBuildingSubType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandLevel();
            }
            return result;
        }
        public static CommandLevel GetCommandLevel(this VariableBuildingSubType var)
        {
            switch (var)
            {
                case VariableBuildingSubType.OwnName:
                case VariableBuildingSubType.NextStopLine:
                case VariableBuildingSubType.PrevStopLine:
                case VariableBuildingSubType.LastStopLine:
                case VariableBuildingSubType.PlatformNumber:
                    return CommandLevel.m_stringFormat;
                default:
                    return null;
            }
        }
        public static bool ReadData(this VariableBuildingSubType var, string[] relativeParams, ref Enum subtype, out VariableExtraParameterContainer extraParams)
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
