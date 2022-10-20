using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class EnvironmentVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.Environment;

        public override string RootMenuDescription => VariableType.Environment.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableEnvironmentSubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = Enum.GetValues(typeof(VariableEnvironmentSubType)).Cast<Enum>().Where(x => (VariableEnvironmentSubType)x != VariableEnvironmentSubType.None).ToArray();

        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableEnvironmentSubType)).Cast<VariableEnvironmentSubType>())
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
            multipleOutput = null;
            return $"{wrapper.paramContainer.prefix}{GetFormattedString(wrapper.subtype, wrapper, out preLoad) ?? wrapper.m_originalCommand}{wrapper.paramContainer.suffix}";
        }
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            paramContainer = default;
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableEnvironmentSubType), parameterPath[1]) is VariableEnvironmentSubType tt
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.Environment;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }

        private static string GetFormattedString(Enum var, TextParameterVariableWrapper varWrapper, out string[] preLoad)
        {
            switch (var)
            {
                case VariableEnvironmentSubType.Clock:
                    var time = SimulationManager.instance.m_currentDayTimeHour;
                    if (ModInstance.Clock12hFormat)
                    {
                        time = ((time + 11) % 12) + 1;
                    }
                    var step = (ModInstance.ClockPrecision.value / 60);
                    preLoad = new[]
                    {
                        GetFormattedTime((time + step) % 24),
                        GetFormattedTime((time + (step*2)) % 24)
                    };
                    return GetFormattedTime(time);
                case VariableEnvironmentSubType.Temperature:
                    preLoad = new[]
                    {
                        WTSEtcData.FormatTemp(WeatherManager.instance.m_currentTemperature+1, "0"),
                        WTSEtcData.FormatTemp(WeatherManager.instance.m_currentTemperature-1, "0"),
                    };
                    return WTSEtcData.FormatTemp(WeatherManager.instance.m_currentTemperature, "0");
                case VariableEnvironmentSubType.CustomFormattedDate:
                    preLoad = new[]
                    {
                        SimulationManager.instance.m_metaData.m_currentDateTime.AddDays(1).ToString(varWrapper.paramContainer.numberFormat.TrimToNull() ?? "dd/MM/yy", WTSEtcData.FormatCulture)
                    };
                    return SimulationManager.instance.m_metaData.m_currentDateTime.ToString(varWrapper.paramContainer.numberFormat.TrimToNull() ?? "dd/MM/yy", WTSEtcData.FormatCulture);
                default:
                    preLoad = null;
                    return null;
            }
        }

        private static string GetFormattedTime(float time)
        {
            var precision = ModInstance.ClockPrecision.value;
            return $"{((int)time).ToString($"D{(ModInstance.ClockShowLeadingZero ? "2" : "1")}")}:{(int)(((int)(time % 1 * 60 / precision)) * precision):D2}";
        }

        public override bool Supports(TextRenderingClass renderingClass) => true;
        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
