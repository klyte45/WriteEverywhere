extern alias TLM;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class CurrentCityVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.CityData;

        public override string RootMenuDescription => VariableType.CityData.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableCitySubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = Enum.GetValues(typeof(VariableCitySubType)).Cast<Enum>().Where(x => (VariableCitySubType)x != VariableCitySubType.None).ToArray();

        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private static Dictionary<Enum, CommandLevel> ReadCommandTree()
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
        public override string GetTargetTextForNet(TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
            => GetText(wrapper, out multipleOutput, out preLoad);
        public override string GetTargetTextForBuilding(TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
            => GetText(wrapper, out multipleOutput, out preLoad);
        public override string GetTargetTextForVehicle(TextParameterVariableWrapper wrapper, ushort vehicleId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
            => GetText(wrapper, out multipleOutput, out preLoad);

        private static string GetText(TextParameterVariableWrapper wrapper, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            preLoad = null;
            multipleOutput = null;
            return $"{wrapper.paramContainer.prefix}{GetFormattedString(wrapper.subtype, wrapper) ?? wrapper.m_originalCommand}{wrapper.paramContainer.suffix}";
        }
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            paramContainer = default;
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableCitySubType), parameterPath[1]) is VariableCitySubType tt
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.CityData;
                    }
                }
                catch { }
            }
            paramContainer.contentType = TextContent.ParameterizedText;
        }

        private static string GetFormattedString(Enum var, TextParameterVariableWrapper varWrapper)
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
        public override bool Supports(TextRenderingClass renderingClass) => true;

        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
