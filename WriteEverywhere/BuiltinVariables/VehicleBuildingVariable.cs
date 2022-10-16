extern alias TLM;
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class VehicleBuildingVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.VehicleBuilding;

        public override string RootMenuDescription => VariableType.VehicleBuilding.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableBuildingSubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = new Enum[] { VariableBuildingSubType.OwnName };

        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in AccessibleSubmenusEnum)
            {
                result[value] = ((VariableBuildingSubType)value).GetCommandLevel();
            }
            return result;
        }

        public override string GetTargetTextForVehicle(TextParameterVariableWrapper wrapper, ushort vehicleId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            var buildingId = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_sourceBuilding;
            return buildingId == 0 || !(wrapper.subtype is VariableBuildingSubType targetSubtype) || targetSubtype == VariableBuildingSubType.None
                ? $"{wrapper.subtype}@vehicleSrcBuilding"
                : $"{CurrentBuildingVariable.GetFormattedString(targetSubtype, null, buildingId, wrapper) ?? wrapper.m_originalCommand}";

        }
        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.Vehicle;
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableBuildingSubType), parameterPath[1]) is VariableBuildingSubType tt
                        && AccessibleSubmenusEnum.Contains(tt)
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.VehicleBuilding;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }

    }
}
