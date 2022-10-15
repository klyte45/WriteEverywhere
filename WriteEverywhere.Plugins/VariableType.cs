using System;
using System.Linq;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins
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

        public static RootCommandLevel GetCommandTree(this VariableType var)
        {
            switch (var)
            {
                case VariableType.SegmentTarget:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.SegmentTarget, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        descriptionKey = () => Str.WTS_PARAMVARS_DESC__SegmentTarget__SelectReference,
                        regexValidValues = "^[0-9]$",
                        nextLevelByRegex = new CommandLevel
                        {
                            defaultValue = VariableSegmentTargetSubType.None,
                            nextLevelOptions = VariableSegmentTargetSubTypeExtensions.ReadCommandTree()
                        }
                    };
                case VariableType.CityData:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.CityData, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        defaultValue = VariableCitySubType.None,
                        nextLevelOptions = VariableCitySubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentBuilding:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.CurrentBuilding, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        defaultValue = VariableBuildingSubType.None,
                        nextLevelOptions = VariableBuildingSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentSegment:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.CurrentSegment, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        defaultValue = VariableSegmentTargetSubType.None,
                        nextLevelOptions = VariableSegmentTargetSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.CurrentVehicle:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.CurrentVehicle, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        defaultValue = VariableVehicleSubType.None,
                        nextLevelOptions = VariableVehicleSubTypeExtensions.ReadCommandTree()
                    };
                case VariableType.Parameter:
                    return new RootCommandLevel
                    {
                        Validate = (TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer) => Validate(VariableType.Parameter, renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer),
                        descriptionKey = () => Str.WTS_PARAMVARS_DESC__COMMON_PARAMNUM,
                        regexValidValues = "^[0-9]{1,2}$",
                        nextLevelByRegex = CommandLevel.m_endLevel
                    };
                default:
                    return null;
            }
        }

        public static void Validate(this VariableType varTypeParsed, TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer)
        {
            paramContainer = default;
            if (!varTypeParsed.Supports(renderingClass))
            {
                return;
            }
            switch (varTypeParsed)
            {
                case VariableType.SegmentTarget:
                    if (parameterPath.Length >= 3 && byte.TryParse(parameterPath[1], out byte targIdx))
                    {
                        try
                        {
                            if (Enum.Parse(typeof(VariableSegmentTargetSubType), parameterPath[2]) is VariableSegmentTargetSubType tt
                                && tt.ReadData(parameterPath.Skip(3).ToArray(), ref subtype, out paramContainer))
                            {
                                index = targIdx;
                                type = VariableType.SegmentTarget;
                            }
                        }
                        catch { }
                    }
                    break;
                case VariableType.CurrentSegment:
                    if (parameterPath.Length >= 2)
                    {
                        try
                        {
                            if (Enum.Parse(typeof(VariableSegmentTargetSubType), parameterPath[1]) is VariableSegmentTargetSubType tt
                                && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                            {
                                type = VariableType.CurrentSegment;
                            }
                        }
                        catch { }
                    }
                    break;
                case VariableType.CityData:
                    if (parameterPath.Length >= 2)
                    {
                        try
                        {
                            if (Enum.Parse(typeof(VariableCitySubType), parameterPath[1]) is VariableCitySubType tt
                                && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                            {
                                type = VariableType.CityData;
                                break;
                            }
                        }
                        catch { }
                    }
                    break;
                case VariableType.CurrentBuilding:
                    if (parameterPath.Length >= 2)
                    {
                        try
                        {
                            if (Enum.Parse(typeof(VariableBuildingSubType), parameterPath[1]) is VariableBuildingSubType tt
                                && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                            {
                                type = VariableType.CurrentBuilding;
                                break;
                            }
                        }
                        catch { }
                    }
                    break;
                case VariableType.CurrentVehicle:
                    if (parameterPath.Length >= 2)
                    {
                        try
                        {
                            if (Enum.Parse(typeof(VariableVehicleSubType), parameterPath[1]) is VariableVehicleSubType tt
                                && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                            {
                                type = VariableType.CurrentVehicle;
                                break;
                            }
                        }
                        catch { }
                    }
                    break;
                case VariableType.Parameter:
                    if (parameterPath.Length >= 2)
                    {
                        try
                        {
                            if (int.TryParse(parameterPath[1], out var idx))
                            {
                                paramContainer.paramIdx = idx;
                                type = VariableType.Parameter;
                                break;
                            }
                        }
                        catch { }
                    }
                    break;
            }
            paramContainer.contentType = varTypeParsed.ToContent();
        }
    }
}
