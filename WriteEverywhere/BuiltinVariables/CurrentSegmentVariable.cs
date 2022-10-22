
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Plugins;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Singleton;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Variables
{
    public sealed class CurrentSegmentVariable : WEVariableExtensionEnum
    {
        public override Enum RootMenuEnumValueWithPrefix { get; } = VariableType.CurrentSegment;

        public override string RootMenuDescription => VariableType.CurrentSegment.ValueToI18n();

        public override Enum DefaultValue { get; } = VariableSegmentSubType.None;

        public override Enum[] AccessibleSubmenusEnum { get; } = Enum.GetValues(typeof(VariableSegmentSubType)).Cast<Enum>().Where(x => (VariableSegmentSubType)x != VariableSegmentSubType.None).ToArray();

        public override Dictionary<Enum, CommandLevel> CommandTree => ReadCommandTree();
        private static Dictionary<Enum, CommandLevel> ReadCommandTree()
        {
            Dictionary<Enum, CommandLevel> result = new Dictionary<Enum, CommandLevel>();
            foreach (var value in Enum.GetValues(typeof(VariableSegmentSubType)).Cast<VariableSegmentSubType>())
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
        {
            preLoad = null;
            multipleOutput = null;
            var subtype = wrapper.subtype;
            var originalCommand = wrapper.m_originalCommand;
            var paramContainer = wrapper.paramContainer;
            return segmentId == 0 || !(subtype is VariableSegmentSubType targetSubtype2) || targetSubtype2 == VariableSegmentSubType.None
                ? $"{paramContainer.prefix}{subtype}@currSeg"
                : $"{paramContainer.prefix}{GetFormattedString(targetSubtype2, propDescriptor, segmentId, wrapper) ?? originalCommand}{paramContainer.suffix}";
        }
        public override bool Supports(TextRenderingClass renderingClass) => renderingClass == TextRenderingClass.Any || renderingClass == TextRenderingClass.PlaceOnNet;
        protected override void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer)
        {
            if (parameterPath.Length >= 2)
            {
                try
                {
                    if (Enum.Parse(typeof(VariableSegmentSubType), parameterPath[1]) is VariableSegmentSubType tt
                        && tt.ReadData(parameterPath.Skip(2).ToArray(), ref subtype, out paramContainer))
                    {
                        type = VariableType.CurrentSegment;
                        paramContainer.contentType = TextContent.ParameterizedText;
                    }
                }
                catch { }
            }
        }
        public static string GetFormattedString(VariableSegmentSubType var, OnNetInstanceCacheContainerXml instance, ushort targId, TextParameterVariableWrapper varWrapper)
        {
            var multiplier = 1f;
            switch (var)
            {
                case VariableSegmentSubType.StreetSuffix: return varWrapper.TryFormat(WTSCacheSingleton.instance.GetSegment(targId).StreetName);
                case VariableSegmentSubType.StreetNameComplete: return varWrapper.TryFormat(WTSCacheSingleton.instance.GetSegment(targId).FullStreetName);
                case VariableSegmentSubType.StreetPrefix: return varWrapper.TryFormat(WTSCacheSingleton.instance.GetSegment(targId).StreetQualifier);
                case VariableSegmentSubType.ParkOrDistrict:
                case VariableSegmentSubType.DistrictOrPark:
                    var segmentData = WTSCacheSingleton.instance.GetSegment(targId);
                    if (segmentData.DistrictId == 0 && segmentData.ParkId > 0 && var == VariableSegmentSubType.ParkOrDistrict)
                    {
                        goto case VariableSegmentSubType.Park;
                    }
                    else
                    {
                        goto case VariableSegmentSubType.District;
                    }
                case VariableSegmentSubType.District:
                    var segmentData2 = WTSCacheSingleton.instance.GetSegment(targId);
                    return varWrapper.TryFormat(segmentData2.OutsideConnectionId != 0
                        ? WTSCacheSingleton.instance.GetBuilding(segmentData2.OutsideConnectionId).Name
                        : WTSCacheSingleton.instance.GetDistrict(segmentData2.DistrictId).Name);
                case VariableSegmentSubType.Park: return varWrapper.TryFormat(WTSCacheSingleton.instance.GetPark(WTSCacheSingleton.instance.GetSegment(targId).ParkId).Name);
                case VariableSegmentSubType.MileageKilometers:
                    multiplier = 0.001f;
                    goto case VariableSegmentSubType.MileageMeters;
                case VariableSegmentSubType.MileageMiles:
                    multiplier = 1 / 1609f;
                    goto case VariableSegmentSubType.MileageMeters;
                case VariableSegmentSubType.MileageMeters:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetSegment(targId).GetMetersAt(instance.SegmentPosition), multiplier);

                case VariableSegmentSubType.DistrictAreaKm2:
                    multiplier = 0.000001f;
                    goto case VariableSegmentSubType.DistrictAreaM2;
                case VariableSegmentSubType.DistrictAreaMi2:
                    multiplier = 1f / 1609f / 1609f;
                    goto case VariableSegmentSubType.DistrictAreaM2;
                case VariableSegmentSubType.DistrictAreaM2:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetDistrict(WTSCacheSingleton.instance.GetSegment(targId).DistrictId).AreaSqMeters, multiplier);

                case VariableSegmentSubType.ParkAreaKm2:
                    multiplier = 0.000001f;
                    goto case VariableSegmentSubType.ParkAreaM2;
                case VariableSegmentSubType.ParkAreaMi2:
                    multiplier = 1f / 1609f / 1609f;
                    goto case VariableSegmentSubType.ParkAreaM2;
                case VariableSegmentSubType.ParkAreaM2:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetPark(WTSCacheSingleton.instance.GetSegment(targId).ParkId).AreaSqMeters, multiplier);

                case VariableSegmentSubType.DistrictPopulation:
                    return varWrapper.TryFormat(WTSCacheSingleton.instance.GetDistrict(WTSCacheSingleton.instance.GetSegment(targId).DistrictId).Population);


                case VariableSegmentSubType.Direction: return WTSCacheSingleton.instance.GetSegment(targId).Direction;
                default:
                    return null;
            }
        }
        public override string GetSubvalueDescription(Enum subRef) => subRef.ValueToI18n();
    }
}
