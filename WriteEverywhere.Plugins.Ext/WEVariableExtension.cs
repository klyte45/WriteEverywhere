using System;
using System.Collections.Generic;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins.Ext
{
    public abstract class WEVariableExtensionRegex : WEVariableExtension
    {
        public abstract string RegexValidValues { get; }
        public abstract CommandLevel NextLevelByRegex { get; }
    }

    public abstract class WEVariableExtensionEnum : WEVariableExtension
    {
        public abstract Enum[] AccessibleSubmenusEnum { get; }
        public abstract Dictionary<Enum, CommandLevel> CommandTree { get; }
        public abstract Enum DefaultValue { get; }
    }

    public abstract class WEVariableExtension
    {
        public WEVariableExtension() { }
        public abstract Enum RootMenuEnumValueWithPrefix { get; }
        public abstract string RootMenuDescription { get; }
        public abstract string GetSubvalueDescription(Enum subRef);

        protected abstract void Validate_Internal(string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, ref VariableExtraParameterContainer paramContainer);
        public virtual string GetTargetTextForBuilding(TextParameterVariableWrapper wrapper, WriteOnBuildingXml propGroupDescriptor, WriteOnBuildingPropXml buildingDescriptor, ushort buildingId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            multipleOutput = null;
            preLoad = null;
            return wrapper.m_originalCommand;
        }
        public virtual string GetTargetTextForVehicle(TextParameterVariableWrapper wrapper, ushort vehicleId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            multipleOutput = null;
            preLoad = null;
            return wrapper.m_originalCommand;
        }
        public virtual string GetTargetTextForNet(TextParameterVariableWrapper wrapper, OnNetInstanceCacheContainerXml propDescriptor, ushort segmentId, int secRefId, int tercRefId, TextToWriteOnXml textDescriptor, out IEnumerable<BasicRenderInformation> multipleOutput, out string[] preLoad)
        {
            multipleOutput = null;
            preLoad = null;
            return wrapper.m_originalCommand;
        }

        public abstract bool Supports(TextRenderingClass renderingClass);
        public void Validate(TextRenderingClass renderingClass, string[] parameterPath, ref Enum type, ref Enum subtype, ref byte index, out VariableExtraParameterContainer paramContainer)
        {
            paramContainer = default;
            if (renderingClass != TextRenderingClass.Any && !Supports(renderingClass))
            {
                return;
            }
            Validate_Internal(parameterPath, ref type, ref subtype, ref index, ref paramContainer);
        }
    }
}
