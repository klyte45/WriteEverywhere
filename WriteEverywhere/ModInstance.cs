using Klyte.Localization;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Globalization;
using System.Reflection;

[assembly: AssemblyVersion("0.0.0.*")]
namespace WriteEverywhere
{
    public class ModInstance : BasicIUserMod<ModInstance, MainController>
    {
        public override string SimpleName { get; } = "Write Everywhere";

        public override string Description { get; } = Str.root_modDescription;

        protected override void SetLocaleCulture(CultureInfo culture) => Str.Culture = culture;
    }

    public class MainController : BaseController<ModInstance, MainController>
    {
        public static readonly string FOLDER_PATH = KFileUtils.BASE_FOLDER_PATH + "WriteEverywhere";
        public WTSShaderLibrary ShaderLib => WTSShaderLibrary.instance;
    }
}
