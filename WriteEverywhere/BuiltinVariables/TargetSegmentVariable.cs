
using System;
using System.Collections.Generic;
using System.Linq;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class TargetSegmentVariable : WEVariableExtensionRegex
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.SegmentTarget;

        public override string RootMenuDescription => Str.WTS_PARAMVARS_DESC__SegmentTarget__SelectReference;

        public override string RegexValidValues { get; } = "^[0-9]$";
        public override CommandLevel NextLevelByRegex { get; } = new CommandLevel
        {
            defaultValue = VariableSegmentSubType.None,
            nextLevelOptions = VariableSegmentTargetSubTypeExtensions.ReadCommandTree()
        };

        public override string GetTargetTextForNet(TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            preLoad = null;
            multipleOutput = null;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            var paramContainer = wrapper.paramContainer;
            var index = wrapper.index;
            var targId = propDescriptor?.GetTargetSegment(index) ?? 0;
            return targId == 0 || !(subtype is VariableSegmentSubType targetSubtype) || targetSubtype == VariableSegmentSubType.None
                ? $"{paramContainer.prefix}{subtype}@targ{index}{paramContainer.suffix}"
                : $"{paramContainer.prefix}{CurrentSegmentVariable.GetFormattedString(targetSubtype, propDescriptor, targId, wrapper) ?? originalCommand}{paramContainer.suffix}";
        }
        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.PlaceOnNet;
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {

            if (parameterPath.Length >= 3 && byte.TryParse(parameterPath[1], out byte targIdx))
            {
                try
                {
                    if (Enum.Parse(typeof(VariableSegmentSubType), parameterPath[2]) is VariableSegmentSubType tt
                        && tt.ReadData(parameterPath.Skip(3).ToArray(), ref subtype, out paramContainer))
                    {
                        index = targIdx;
                        type = VariableType.SegmentTarget;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }
        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
