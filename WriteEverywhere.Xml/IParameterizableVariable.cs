namespace WriteEverywhere.Xml
{
    public interface IParameterizableVariable
    {
        string GetParameterDisplayName();
        int GetParamIdx();
        TextContent GetTextContent();
        string GetValueAsUri();
    }
}