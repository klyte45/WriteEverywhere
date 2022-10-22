

using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Data;
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

    public sealed class CurrentVehicleVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.CurrentVehicle;

        public override string RootMenuDescription => VariableType.CurrentVehicle.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableVehicleSubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = Enum.GetValues(typeof(VariableVehicleSubType)).Cast<Enum>().Where(x => (VariableVehicleSubType)x != VariableVehicleSubType.None).ToArray();
        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private static Dictionary<Enum, CommandLevel> ReadCommandTree()
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

        public override string GetTargetTextForVehicle(TextParameterVariableWrapper wrapper, ushort vehicleId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            preLoad = null;
            multipleOutput = null;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            return vehicleId == 0 || !(subtype is VariableVehicleSubType targetSubtype) || targetSubtype == VariableVehicleSubType.None
                ? $"{subtype}@currVehicle"
                : $"{GetFormattedString(targetSubtype, vehicleId, wrapper) ?? originalCommand}";
        }
        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.Vehicle;
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableVehicleSubType), parameterPath[1]) is VariableVehicleSubType tt
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.CurrentVehicle;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }
        private static string GetFormattedString(VariableVehicleSubType var, ushort vehicleId, TextParameterVariableWrapper varWrapper)
        {
            if (vehicleId == 0)
            {
                return null;
            }
            switch (var)
            {
                case VariableVehicleSubType.NextStopLine:
                    ref Vehicle[] buffer7 = ref VehicleManager.instance.m_vehicles.m_buffer;
                    ref Vehicle targetVehicle7 = ref buffer7[buffer7[vehicleId].GetFirstVehicle(vehicleId)];
                    var regLine2 = ModInstance.Controller.ConnectorTLM.GetVehicleLine(vehicleId);
                    return varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(targetVehicle7.m_targetBuilding, regLine2).AsFormattable());
                case VariableVehicleSubType.PrevStopLine:
                    ref Vehicle[] buffer5 = ref VehicleManager.instance.m_vehicles.m_buffer;
                    ref Vehicle targetVehicle5 = ref buffer5[buffer5[vehicleId].GetFirstVehicle(vehicleId)];
                    var regLine3 = ModInstance.Controller.ConnectorTLM.GetVehicleLine(vehicleId);
                    return varWrapper.TryFormat(ModInstance.Controller.ConnectorTLM.GetStopName(TransportLine.GetPrevStop(targetVehicle5.m_targetBuilding), regLine3).AsFormattable());
                case VariableVehicleSubType.LastStopLine:
                    ref Vehicle[] buffer2 = ref VehicleManager.instance.m_vehicles.m_buffer;
                    ref Vehicle targetVehicle = ref buffer2[buffer2[vehicleId].GetFirstVehicle(vehicleId)];
                    var regLine4 = ModInstance.Controller.ConnectorTLM.GetVehicleLine(vehicleId);
                    if (regLine4.ZeroLine)
                    {
                        return varWrapper.TryFormat(targetVehicle.m_targetBuilding == 0 || (targetVehicle.m_flags & Vehicle.Flags.GoingBack) != 0
                            ? WTSCacheSingleton.instance.GetBuilding(targetVehicle.m_sourceBuilding).Name
                            : WTSCacheSingleton.instance.GetBuilding(WTSBuildingData.Instance.CacheData.GetStopBuilding(targetVehicle.m_targetBuilding, regLine4)).Name);
                    }
                    else
                    {
                        var target = targetVehicle.m_targetBuilding;
                        var lastTarget = TransportLine.GetPrevStop(target);
                        StopInformation stopInfo = WTSStopUtils.GetStopDestinationData(lastTarget);
                        var result =
                              stopInfo.m_destinationString.TrimToNull() ?? (
                              stopInfo.m_destinationId != 0
                                ? ModInstance.Controller.ConnectorTLM.GetStopName(stopInfo.m_destinationId, regLine4)
                                : ModInstance.Controller.ConnectorTLM.GetStopName(targetVehicle.m_targetBuilding, regLine4)
                            );
                        return varWrapper.TryFormat(result.AsFormattable());
                    }
                case VariableVehicleSubType.OwnNumber:
                    return WTSCacheSingleton.instance.GetVehicle(vehicleId).Identifier;
                case VariableVehicleSubType.LineIdentifier:
                    ref Vehicle[] buffer = ref VehicleManager.instance.m_vehicles.m_buffer;
                    var targetVehicleId = buffer[vehicleId].GetFirstVehicle(vehicleId);
                    var transportLine = ModInstance.Controller.ConnectorTLM.GetVehicleLine(targetVehicleId);
                    if (!transportLine.ZeroLine)
                    {
                        return WTSCacheSingleton.instance.GetATransportLine(transportLine).Identifier;
                    }
                    else
                    {
                        ref Vehicle vehicle = ref buffer[targetVehicleId];
                        return vehicle.m_targetBuilding == 0 || (vehicle.m_flags & Vehicle.Flags.GoingBack) != 0
                            ? vehicle.m_sourceBuilding.ToString("D5")
                            : $"R{vehicle.m_targetBuilding:X4}";
                    }
                case VariableVehicleSubType.LineFullName:
                    var regLine = ModInstance.Controller.ConnectorTLM.GetVehicleLine(vehicleId);
                    return regLine.regional ? null : varWrapper.TryFormat(WTSCacheSingleton.instance.GetATransportLine(regLine).Name);
                default:
                    return null;
            }
        }
        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
