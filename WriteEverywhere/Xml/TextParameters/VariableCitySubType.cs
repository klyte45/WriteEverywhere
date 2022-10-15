using WriteEverywhere.Plugins;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{

    internal static class VariableCitySubTypeExtensions
    {
        public static string GetFormattedString(this VariableCitySubType var, TextParameterVariableWrapper varWrapper)
        {
            switch (var)
            {
                case VariableCitySubType.CityName:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetDistrict(0).Name);
                case VariableCitySubType.CityPopulation:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetDistrict(0).Population);
                default:
                    return null;
            }
        }

    }
}
