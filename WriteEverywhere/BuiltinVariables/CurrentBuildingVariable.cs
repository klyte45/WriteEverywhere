using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Singleton;
using WriteEverywhere.TransportLines;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{

    public sealed class CurrentBuildingVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.CurrentBuilding;

        public override string RootMenuDescription => VariableType.CurrentBuilding.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableBuildingSubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = Enum.GetValues(typeof(VariableBuildingSubType)).Cast<Enum>().Where(x => (VariableBuildingSubType)x != VariableBuildingSubType.None).ToArray();

        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private static Dictionary<Enum, CommandLevel> ReadCommandTree()
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
        public override string GetTargetTextForBuilding(TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            preLoad = null;
            multipleOutput = null;
            var subtype = wrapper.subtype;
            return buildingId == 0 || buildingDescriptor is null || !(subtype is VariableBuildingSubType targetSubtype2) || targetSubtype2 == VariableBuildingSubType.None
                ? $"{subtype}@currBuilding"
                : $"{GetFormattedString(targetSubtype2, buildingDescriptor.m_platforms, buildingId, wrapper) ?? wrapper.m_originalCommand}";
        }
        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.Buildings;
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableBuildingSubType), parameterPath[1]) is VariableBuildingSubType tt
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.CurrentBuilding;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }

        public static string GetFormattedString(VariableBuildingSubType var, IEnumerable<int> platforms, ushort buildingId, TextParameterVariableWrapper varWrapper)
        {
            if (buildingId == 0)
            {
                return null;
            }
            StopInformation targetStop;
            switch (var)
            {
                case VariableBuildingSubType.OwnName:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetBuilding(buildingId).Name);
                case VariableBuildingSubType.NextStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return targetStop.m_lineId == 0 ? "" : varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_nextStopId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.PrevStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return targetStop.m_lineId == 0 ? "" : varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_previousStopId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.LastStopLine:
                    targetStop = WTSStopUtils.GetTargetStopInfo(platforms, buildingId).FirstOrDefault();
                    return targetStop.m_lineId == 0 ? "" : varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetStop.m_destinationId, new WTSLine(targetStop.m_lineId, targetStop.m_regionalLine)).AsFormattable());
                case VariableBuildingSubType.PlatformNumber:
                    return varWrapper.TryFormat(platforms.FirstOrDefault() + 1);
                default:
                    return null;
            }
        }
        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
