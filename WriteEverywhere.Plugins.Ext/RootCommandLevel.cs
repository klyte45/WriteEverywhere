using WriteEverywhere.Xml;

namespace WriteEverywhere.Plugins.Ext
{
    public class RootCommandLevel : CommandLevel
    {
        public WEVariableExtension SrcClass { get; set; }
        public override bool Supports(TextRenderingClass clazz) => SrcClass.Supports(clazz);
    }
}
