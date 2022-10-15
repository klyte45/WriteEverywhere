
using System;
using WriteEverywhere.Plugins;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Xml
{
    public class TextParameterVariableWrapper
    {
        public readonly string m_originalCommand;
        public readonly Enum m_varType;

        public TextParameterVariableWrapper(string input, TextRenderingClass renderingClass = TextRenderingClass.Any)
        {
            m_originalCommand = input;
            var parameterPath = CommandLevelSingletonBase.GetParameterPath(input, out m_varType);
            if (parameterPath.Length > 0)
            {
                if (m_varType is VariableType varTypeParsed)
                {
                    varTypeParsed.Validate(renderingClass, parameterPath, ref type, ref subtype, ref index, out paramContainer);
                }
            }
        }


        public readonly Enum type = VariableType.Invalid;
        public readonly byte index = 0;
        public readonly Enum subtype = VariableSegmentTargetSubType.None;
        public VariableExtraParameterContainer paramContainer = default;

    }
}
