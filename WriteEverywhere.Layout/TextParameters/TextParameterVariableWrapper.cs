
using System;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{
    public class TextParameterVariableWrapper
    {
        public readonly string m_originalCommand;
        public readonly Enum m_varType;
        public readonly Enum type = VariableType.Invalid;
        public readonly byte index = 0;
        public readonly Enum subtype = VariableSegmentSubType.None;
        public VariableExtraParameterContainer paramContainer = default;

        public TextParameterVariableWrapper(string input, TextRenderingClass renderingClass = TextRenderingClass.Any)
        {
            m_originalCommand = input;
            CommandLevelSingletonBase.Instance.ReadVariableData(renderingClass, CommandLevelSingletonBase.GetParameterPath(input), out m_varType, ref type, ref subtype, ref index, out paramContainer);
        }
    }
}
