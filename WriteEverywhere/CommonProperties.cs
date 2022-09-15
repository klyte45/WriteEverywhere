using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere;

namespace Kwytto
{
    public static class CommonProperties
    {
        public static bool DebugMode => ModInstance.DebugMode;
        public static string Version => ModInstance.Version;
        public static string ModName => ModInstance.Instance.SimpleName;
        public static string Acronym { get; } = "WE";
        public static string ModRootFolder => MainController.FOLDER_PATH;
        public static string ModIcon => ModInstance.Instance.IconName;
        public static string ModDllRootFolder => ModInstance.RootFolder;

        public static string GitHubRepoPath => "klyte45/WriteEverywhere";


        internal static readonly string[] AssetExtraDirectoryNames = new string[0];
        internal static readonly string[] AssetExtraFileNames = new string[] { };

        public static Color ModColor { get; } = ColorExtensions.FromRGB("44aadd");
        public static float UIScale { get; } = 1f;
    }
}