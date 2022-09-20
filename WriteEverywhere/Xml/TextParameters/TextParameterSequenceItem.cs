﻿using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{
    public class TextParameterSequenceItem : IParameterizableVariable
    {
        public long m_length;

        public TextParameterWrapper Value { get; private set; }
        public string GetParameterDisplayName() => null;

        public TextContent GetTextContent() => Value.VariableValueTextContent;

        public object GetValueAsUri() => Value.ToString();

        public int GetParamIdx() => Value.GetParamIdx;
        public TextParameterSequenceItem(string value, TextRenderingClass clazz, long length = 500)
        {
            m_length = length;
            Value = new TextParameterWrapper(value, clazz);
        }
    }
}
