using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{
    public class TextParameterSequenceItem
    {
        public long m_length;

        public TextParameterWrapper Value { get; private set; }

        public TextParameterSequenceItem(string value, TextRenderingClass clazz, long length = 500)
        {
            m_length = length;
            Value = new TextParameterWrapper(value, clazz);
        }
    }
}
