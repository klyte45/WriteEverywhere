#if  UNITY_EDITOR
#else
using ColossalFramework;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WriteEverywhere
{
    public class WEAssetLibrary : SingletonLite<WEAssetLibrary>
    {
        private Dictionary<string, Shader> m_loadedShaders = null;

        public WEAssetLibrary()
        {
            GetShaders();
        }

        public Dictionary<string, Shader> GetShaders()
        {
            if (m_loadedShaders is null)
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    m_loadedShaders = LoadAllShaders("Shader.ShaderTest.unity3d");
                    LogUtils.DoLog($"Shaders loaded for {Application.platform}!");
                }
                else if (Application.platform == RuntimePlatform.LinuxPlayer)
                {
                    m_loadedShaders = LoadAllShaders("Shader.ShaderTest-linux.unity3d");
                    LogUtils.DoLog($"Shaders loaded for {Application.platform}!");
                }
                else if (Application.platform == RuntimePlatform.OSXPlayer)
                {

                    m_loadedShaders = LoadAllShaders("Shader.ShaderTest-macosx.unity3d");
                    LogUtils.DoLog($"Shaders loaded for {Application.platform}!");
                }
                else
                {
                    m_loadedShaders = new Dictionary<string, Shader>();
                    LogUtils.DoErrorLog($"WARNING: Shaders not found for {Application.platform}!");
                }
                LogUtils.DoLog($"Shaders loaded:\n\t- {string.Join("\n\t- ", m_loadedShaders.Keys?.ToArray() ?? new[] { "ERRRRRR" })}");
            }
            return m_loadedShaders;
        }

        private AssetBundle m_memoryLoaded;
        public bool ReloadFromDisk()
        {
            LogUtils.DoWarnLog("LOADING Shaders");
            m_memoryLoaded?.Unload(true);
            var addr = System.Environment.GetEnvironmentVariable("K45_WE_PROJECTROOT") + "/Shader/ShaderTest.unity3d";
            LogUtils.DoLog($"Loading from: {addr}");
            m_memoryLoaded = AssetBundle.LoadFromFile(addr);
            if (m_memoryLoaded != null)
            {
                LogUtils.DoWarnLog("FOUND Shaders");
                ReadShaders(m_memoryLoaded, out m_loadedShaders);
                return true;
            }
            else
            {
                LogUtils.DoErrorLog("NOT FOUND Shaders");
                return false;
            }
        }

        public Shader GetLoadedShader(string shaderName)
        {
            GetShaders().TryGetValue(shaderName, out Shader result);
            return result;
        }
        private Dictionary<string, Shader> LoadAllShaders(string assetBundleName)
        {
            var bundle = KResourceLoader.LoadBundle(assetBundleName);
            if (bundle != null)
            {
                ReadShaders(bundle, out Dictionary<string, Shader> m_loadedShaders);
                bundle.Unload(false);
                return m_loadedShaders;
            }
            return null;
        }
        public Mesh FrameMesh { get; private set; }

        private void ReadShaders(AssetBundle bundle, out Dictionary<string, Shader> m_loadedShaders)
        {
            m_loadedShaders = new Dictionary<string, Shader>();
            string[] files = bundle.GetAllAssetNames();
            foreach (string filename in files)
            {
                LogUtils.DoLog($"Reading file {filename} inside the bundle {bundle}");
                if (filename.EndsWith(".shader"))
                {
                    Shader shader = bundle.LoadAsset<Shader>(filename);
                    string effectiveName = filename.Split('.')[0].Split('/').Last();
                    shader.name = $"klyte/wts/{effectiveName}";
                    m_loadedShaders[shader.name] = (shader);
                }
                if (filename.EndsWith(".fbx"))
                {
                    Mesh mesh = bundle.LoadAsset<Mesh>(filename);
                    var vertices = mesh.vertices.ToArray();
                    mesh.vertices = vertices;
                    FrameMesh = mesh;
                }
            }
        }
    }
}
#endif