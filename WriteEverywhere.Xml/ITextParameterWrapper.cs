namespace WriteEverywhere.Xml
{
    public interface ITextParameterWrapper
    {
        string AtlasName { get; }
        bool IsEmpty { get; }
        bool IsLocal { get; }
        bool IsParameter { get; }
        ParameterType ParamType { get; }
        string TextOrSpriteValue { get; set; }

        void SetVariableFromString(string stringNoProtocol, TextRenderingClass clazz = TextRenderingClass.Any);
        string ToString();

        int GetParamIdx { get; }
    }
}