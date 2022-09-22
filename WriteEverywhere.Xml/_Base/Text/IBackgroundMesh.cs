using Kwytto.Utils;

namespace WriteEverywhere.Xml
{
    public interface IBackgroundMesh
    {
        string BackColorStr { get; set; }
        string BgColorStr { get; set; }
        ITextParameterWrapper BgImage { get; }
        string BgImageAsUri { get; set; }
        FrameMesh FrameMeshSettings { get; set; }
        Vector2Xml Size { get; set; }

        string SetBgImage(string value);
    }
}