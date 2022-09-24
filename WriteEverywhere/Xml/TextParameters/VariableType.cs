﻿using WriteEverywhere.Localization;

namespace WriteEverywhere.Xml
{
    public enum VariableType
    {
        Invalid,
        SegmentTarget,
        CityData,
        CurrentBuilding,
        CurrentVehicle,
        CurrentSegment,
        Parameter
    }

    public static class VariableTypeExtension
    {
        public static bool Supports(this VariableType var, TextRenderingClass renderingClass)
        {
            if (renderingClass == TextRenderingClass.Any)
            {
                return true;
            }
            switch (var)
            {
                case VariableType.CurrentBuilding:
                    return renderingClass == TextRenderingClass.Buildings || renderingClass == TextRenderingClass.Vehicle;
                case VariableType.SegmentTarget:
                    return renderingClass == TextRenderingClass.PlaceOnNet;
                case VariableType.Parameter:
                    return renderingClass == TextRenderingClass.Buildings || renderingClass == TextRenderingClass.PlaceOnNet;
                case VariableType.CurrentVehicle:
                    return renderingClass == TextRenderingClass.Vehicle;
                case VariableType.CurrentSegment:
                    return renderingClass == TextRenderingClass.RoadNodes || renderingClass == TextRenderingClass.PlaceOnNet;
                default:
                    return true;
            }
        }
        public static TextContent ToContent(this VariableType var)
        {
            switch (var)
            {
                case VariableType.CurrentBuilding:
                case VariableType.SegmentTarget:
                case VariableType.CurrentVehicle:
                case VariableType.CurrentSegment:
                case VariableType.CityData:
                    return TextContent.ParameterizedText;
                case VariableType.Parameter:
                    return TextContent.Any;
                case VariableType.Invalid:
                default:
                    return TextContent.None;
            }
        }

        public static CommandLevel GetCommandTree(this VariableType var)
        {
            switch (var)
            {
                case VariableType.SegmentTarget:
                    return new CommandLevel
                    {
                        descriptionKey = () => Str.WTS_PARAMVARS_DESC__SegmentTarget__SelectReference,
                        regexValidValues = "^[0-9]$",
                        nextLevelByRegex = new CommandLevel
                        {
                            defaultValue = VariableSegmentTargetSubType.None,
                            nextLevelOptions = VariableSegmentTargetSubTypeExtensions.ReadCommandTree()
                        }
                    };
                case VariableType.CityData:
                    return new CommandLevel
                    {
                        defaultValue = VariableCitySubType.None,
                        nextLevelOptions = VariableCitySubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentBuilding:
                    return new CommandLevel
                    {
                        defaultValue = VariableBuildingSubType.None,
                        nextLevelOptions = VariableBuildingSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentSegment:
                    return new CommandLevel
                    {
                        defaultValue = VariableSegmentTargetSubType.None,
                        nextLevelOptions = VariableSegmentTargetSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentVehicle:
                    return new CommandLevel
                    {
                        defaultValue = VariableVehicleSubType.None,
                        nextLevelOptions = VariableVehicleSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.Parameter:
                    return CommandLevel.m_numberSet;
                default:
                    return null;
            }
        }
    }
}
