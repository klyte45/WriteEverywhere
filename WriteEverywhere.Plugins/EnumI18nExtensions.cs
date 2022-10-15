using Kwytto.Localization;
using System;
using WriteEverywhere.Localization;

namespace WriteEverywhere.Plugins
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
                        case VariableType.Parameter: return Str.WTS_PARAMVARS_DESC__VariableType_Parameter;
                    }
                    break;
            }
            return variable.ValueToI18nKwytto();
        }
    }
}
