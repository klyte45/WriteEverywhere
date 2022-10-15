﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Plugins
{
    public enum VariableVehicleSubType
    {
        None,
        OwnNumber,
        LineIdentifier,
        NextStopLine,
        PrevStopLine,
        LastStopLine,
        LineFullName,
    }

    public static class VariableVehicleSubTypeExtensions
    {
        public static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableVehicleSubType)).Cast<VariableVehicleSubType>())
            {
                if (value == 0)
                {
                    continue;
                }

                result[value] = value.GetCommandLevel();
            }
            return result;
        }
        public static CommandLevel GetCommandLevel(this VariableVehicleSubType var)
        {
            switch (var)
            {
                case VariableVehicleSubType.LineFullName:
                case VariableVehicleSubType.LineIdentifier:
                case VariableVehicleSubType.OwnNumber:
                case VariableVehicleSubType.LastStopLine:
                case VariableVehicleSubType.NextStopLine:
                case VariableVehicleSubType.PrevStopLine:
                    return CommandLevel.m_appendPrefix;
                default:
                    return null;
            }
        }
        public static bool ReadData(this VariableVehicleSubType var, string[] relativeParams, ref Enum subtype, out VariableExtraParameterContainer paramContainer)
        {
            var cmdLevel = var.GetCommandLevel();
            if (cmdLevel is null)
            {
                paramContainer = default;
                return false;
            }

            cmdLevel.ParseFormatting(relativeParams, out paramContainer);
            subtype = var;
            return true;
        }
    }
}
