using ColossalFramework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using WriteEverywhere.Plugins;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Layout
{
    public class TextParameterWrapper : ITextParameterWrapper
    {
        public string TextOrSpriteValue
        {
            get => textOrSpriteValue; set
            {
                IsEmpty = value is null;
                textOrSpriteValue = IsEmpty ? "" : value;
            }
        }
        public TextParameterVariableWrapper VariableValue { get; private set; }
        public string AtlasName
        {
            get => atlasName; private set
            {
                atlasName = value;
                m_isDirtyImage = true;
            }
        }
        public bool IsLocal
        {
            get => isLocal; private set
            {
                isLocal = value;
                m_isDirtyImage = true;
            }
        }
        public ParameterType ParamType { get; private set; }
        public TextContent VariableValueTextContent => ParamType == ParameterType.VARIABLE ? VariableValue.paramContainer.contentType : TextContent.None;
        public bool IsEmpty { get; private set; }
        public bool IsParameter => VariableValue?.m_varType is VariableType v && v == VariableType.Parameter;

        public int GetParamIdx => VariableValue?.paramContainer.paramIdx ?? -1;

        public string OriginalCommand => VariableValue.m_originalCommand;

        private Dictionary<string, WEImageInfo> m_cachedAtlas;
        private string atlasName;
        private bool isLocal;
        private bool m_isDirtyImage = true;
        private PrefabInfo cachedPrefab;
        private ulong cachedPrefabId;
        private string textOrSpriteValue;

        public TextParameterWrapper()
        {
            ParamType = ParameterType.TEXT;
            TextOrSpriteValue = string.Empty;
        }
        public TextParameterWrapper(string value, TextRenderingClass clazz = TextRenderingClass.Any, bool acceptLegacy = false) : this()
        {
            if (value is null)
            {
                ParamType = ParameterType.EMPTY;
                IsEmpty = true;
                return;
            }
            var inputMatches = Regex.Match(value, "^(folder|assetFolder|image|assetImage)://(([^/]+)/)?([^/]+)$|^var://(([a-zA-Z0-9_]+::)?[a-zA-Z0-9_]+/.*)?$");
            if (inputMatches.Success)
            {
                switch (value.Split(':')[0])
                {
                    case "folder":
                        SetLocalFolder(inputMatches.Groups[4].Value);
                        return;
                    case "assetFolder":
                        SetAssetFolder();
                        return;
                    case "image":
                        SetLocalImage(inputMatches.Groups[3].Value, inputMatches.Groups[4].Value);
                        return;
                    case "assetImage":
                        SetAssetImage(inputMatches.Groups[4].Value);
                        return;
                    case "var":
                        SetVariableFromString(inputMatches.Groups[5].Value, clazz);
                        return;
                    default:
                        SetPlainString(value);
                        return;
                }
            }
            else
            {
                TextOrSpriteValue = value;
                ParamType = ParameterType.TEXT;
            }
        }

        public void SetLocalFolder(string folderName)
        {
            ParamType = ParameterType.FOLDER;
            isLocal = true;
            atlasName = folderName;
            if (atlasName == "<ROOT>")
            {
                atlasName = string.Empty;
            }
        }

        public void SetAssetFolder()
        {
            ParamType = ParameterType.FOLDER;
            isLocal = false;
            atlasName = string.Empty;
        }

        public void SetLocalImage(string folder, string file)
        {
            ParamType = ParameterType.IMAGE;
            isLocal = true;
            atlasName = folder;
            if (folder is null || atlasName == "<ROOT>")
            {
                atlasName = string.Empty;
            }
            textOrSpriteValue = file;
        }

        public void SetAssetImage(string name)
        {
            ParamType = ParameterType.IMAGE;
            isLocal = false;
            atlasName = string.Empty;
            textOrSpriteValue = name;
        }


        public void SetVariableFromString(string stringNoProtocol, TextRenderingClass clazz = TextRenderingClass.Any)
        {
            ParamType = ParameterType.VARIABLE;
            VariableValue = new TextParameterVariableWrapper(stringNoProtocol, clazz);
        }

        public void SetPlainString(string value)
        {
            VariableValue = null;
            TextOrSpriteValue = value;
            ParamType = ParameterType.TEXT;
        }

        public Dictionary<string, WEImageInfo> GetAtlas(PrefabInfo prefab)
        {
            UpdateCachedAtlas(prefab);
            return m_cachedAtlas;
        }

        private void UpdateCachedAtlas(PrefabInfo prefab)
        {
            if (m_isDirtyImage)
            {
                if (ParamType == ParameterType.FOLDER || ParamType == ParameterType.IMAGE)
                {
                    UpdatePrefabInfo(prefab);
                    //ModInstance.Controller.AtlasesLibrary.GetSpriteLib(isLocal ? atlasName : cachedPrefabId.ToString(), out m_cachedAtlas);
                }
                else
                {
                    m_cachedAtlas = null;
                }
                m_isDirtyImage = false;
            }
        }

        public WEImageInfo GetCurrentWEImageInfo(PrefabInfo prefab)
        {
            UpdateCachedAtlas(prefab);
            return m_cachedAtlas?[TextOrSpriteValue];
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return null;
            }

            switch (ParamType)
            {
                case ParameterType.FOLDER:
                    return $"{(isLocal ? "folder" : "assetFolder")}://{(isLocal && !atlasName.IsNullOrWhiteSpace() ? atlasName : "<ROOT>")}";
                case ParameterType.IMAGE:
                    return $"{(isLocal ? "image" : "assetImage")}://{(isLocal && !atlasName.IsNullOrWhiteSpace() ? atlasName + "/" : "<ROOT>/")}{TextOrSpriteValue}";
                case ParameterType.VARIABLE:
                    return $"var://{VariableValue.m_originalCommand}";
                default:
                    return TextOrSpriteValue;
            }
        }
        public ulong GetCachedPrefab(PrefabInfo prefab)
        {
            UpdatePrefabInfo(prefab);
            return cachedPrefabId;
        }
        private void UpdatePrefabInfo(PrefabInfo prefab)
        {
            if (cachedPrefab != prefab)
            {
                ulong.TryParse((prefab?.name ?? "").Split('.')[0], out cachedPrefabId);
                cachedPrefab = prefab;
            }
        }


    }
}
