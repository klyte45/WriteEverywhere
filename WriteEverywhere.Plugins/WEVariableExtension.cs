using System;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins
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
        //public abstract void GetTargetTextForBuilding(this TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput);
    }
}
