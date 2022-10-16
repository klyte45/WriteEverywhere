using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Plugins;

namespace WriteEverywhere.Variables
{
    public enum VariableCitySubType
    {
        None,
        CityName,
        CityPopulation,
    }

    public static class VariableCitySubTypeExtensions
    {
        public static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableCitySubType)).Cast<VariableCitySubType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandLevel();
            }
            return result;
        }
        public static CommandLevel GetCommandLevel(this VariableCitySubType var)
        {
            switch (var)
            {
                case VariableCitySubType.CityPopulation:
                    return CommandLevel.m_numberFormatInt;
                case VariableCitySubType.CityName:
                    return CommandLevel.m_appendPrefix;
                default:
                    return null;
            }
        }
        public static bool ReadData(this VariableCitySubType var, string[] relativeParams, ref Enum subtype, out VariableExtraParameterContainer extraParams)
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
