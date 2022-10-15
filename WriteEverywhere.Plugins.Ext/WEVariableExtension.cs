using System;
using System.Collections.Generic;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins.Ext
{
    public abstract class WEVariableExtension
    {
        public WEVariableExtension() { }
        public abstract Enum RootMenuEnumValueWithPrefix { get; }
        public abstract string RootMenuDescription { get; }
        public abstract Enum DefaultValue { get; }
        public abstract Enum[] AccessibleSubmenusEnum { get; }
        public abstract CommandLevel GetCommandTree(Enum var);
        public abstract void Validate(TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer);
        public abstract void GetTargetTextForBuilding(TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput);
        public abstract string GetTargetTextForVehicle(TextParameterVariableWrapper wrapper, ushort vehicleId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput);
        public abstract string GetTargetTextForNet(TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput);
    }
}
