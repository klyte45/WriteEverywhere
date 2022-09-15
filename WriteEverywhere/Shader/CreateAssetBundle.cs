#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build Shader Bundle for WE")]
    static void BuildAllAssetBundles()
    {
        // Bring up save panel
        string path = "Assets/WE/ShaderTest.unity3d";
        if (path == null) return;
        var opts = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.CollectDependencies;
        BuildTarget[] platforms = { BuildTarget.StandaloneWindows, BuildTarget.StandaloneOSXUniversal, BuildTarget.StandaloneLinux };
        string[] platformExts = { "", "-macosx", "-linux" };

        for (var i = 0; i < platforms.Length; ++i)
        {
            // Build the resource file from the active selection.
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);            
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path.Replace(".unity3d", platformExts[i] + ".unity3d"), opts, platforms[i]);
            Selection.objects = selection;
        }
    }
}
#endif