using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WriteEverywhere.Data;
using static ColossalFramework.UI.UITextureAtlas;

namespace WriteEverywhere.Xml
{
    public class TextParameterWrapper
    {
        public string TextOrSpriteValue
        {
            get => textOrSpriteValue; set
            {
                IsEmpty = value is null;
                textOrSpriteValue = IsEmpty ? "" : value;
            }
        }
        private TextParameterVariableWrapper VariableValue { get; set; }
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
        public TextContent VariableValueTextContent => ParamType == ParameterType.VARIABLE ? VariableValue.m_varType.ToContent() : TextContent.None;
        public bool IsEmpty { get; private set; }
        public bool IsParameter => VariableValue?.m_varType == VariableType.CurrentSegmentParameter;

        public int GetParamIdx => VariableValue?.paramContainer.paramIdx ?? -1;

        private UITextureAtlas m_cachedAtlas;
        private string atlasName;
        private bool isLocal;
        private bool m_isDirtyImage = true;
        private PrefabInfo cachedPrefab;
        private ulong cachedPrefabId;
        private string textOrSpriteValue;
        public enum ParameterType
        {
            TEXT,
            IMAGE,
            FOLDER,
            VARIABLE,
            EMPTY
        }
        public TextParameterWrapper()
        {
            ParamType = ParameterType.TEXT;
            TextOrSpriteValue = string.Empty;
        }
        public TextParameterWrapper(string value, Rendering.TextRenderingClass clazz = Rendering.TextRenderingClass.Any, bool acceptLegacy = false) : this()
        {
            if (value is null)
            {
                ParamType = ParameterType.EMPTY;
                IsEmpty = true;
                return;
            }
            var inputMatches = Regex.Match(value, "^(folder|assetFolder|image|assetImage)://(([^/]+)/)?([^/]+)$|^var://([a-zA-Z0-9_]+/.*)?$");
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


        public void SetVariableFromString(string stringNoProtocol, Rendering.TextRenderingClass clazz = Rendering.TextRenderingClass.Any)
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

        public UITextureAtlas GetAtlas(PrefabInfo prefab)
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
                    ModInstance.Controller.AtlasesLibrary.GetAtlas(isLocal ? atlasName : cachedPrefabId.ToString(), out m_cachedAtlas);
                }
                else
                {
                    m_cachedAtlas = null;
                }
                m_isDirtyImage = false;
            }
        }

        public SpriteInfo GetCurrentSpriteInfo(PrefabInfo prefab)
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

        private void UpdatePrefabInfo(PrefabInfo prefab)
        {
            if (cachedPrefab != prefab)
            {
                ulong.TryParse((prefab?.name ?? "").Split('.')[0], out cachedPrefabId);
                cachedPrefab = prefab;
            }
        }

        #region renderingInfo

        public static DynamicSpriteFont GetTargetFont(BaseWriteOnXml instance, BoardTextDescriptorGeneralXml textDescriptor)
            => FontServer.instance.FirstOf(new[]
            {
                        textDescriptor.m_overrideFont,
                        WTSEtcData.Instance.FontSettings.GetTargetFont(textDescriptor.m_fontClass),
                        instance.DescriptorOverrideFont,
                        WTSEtcData.Instance.FontSettings.GetTargetFont(instance.RenderingClass),
                        MainController.DEFAULT_FONT_KEY,
            }.Where(x => !x.IsNullOrWhiteSpace()));

        public static BasicRenderInformation GetRenderInfo(BaseWriteOnXml instance, BoardTextDescriptorGeneralXml textDescriptor, ushort refId, int secIdx, int tercIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                    return null;
                case TextContent.ParameterizedText:
                    return textDescriptor.Value?.GetTargetText(instance, textDescriptor, GetTargetFont(instance, textDescriptor), refId, secIdx, tercIdx, out multipleOutput);
                case TextContent.ParameterizedSpriteFolder:
                    return textDescriptor.Value?.GetSpriteFromCycle(textDescriptor, instance.TargetAssetParameter, refId, secIdx, tercIdx);
                case TextContent.ParameterizedSpriteSingle:
                    return textDescriptor.Value?.GetSpriteFromParameter(instance.TargetAssetParameter);
                case TextContent.LinesNameList:
                    break;
                case TextContent.HwShield:
                    break;
                case TextContent.TimeTemperature:
                    break;
                case TextContent.LinesSymbols:
                    break;
            }
            return null;
        }


        public static BasicRenderInformation GetRenderInfo(TextParameterWrapper tpw, BaseWriteOnXml instance, BoardTextDescriptorGeneralXml textDescriptor, ushort refId, int secIdx, int tercIdx, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            multipleOutput = null;
            switch (textDescriptor.textContent)
            {
                case TextContent.None:
                    LogUtils.DoWarnLog("INVALID TEXT CONTENT: NONE!\n" + Environment.StackTrace);
                    return null;
                case TextContent.ParameterizedText:
                Text:
                    return !(tpw is null)
                        ? tpw.GetTargetText(instance, textDescriptor, GetTargetFont(instance, textDescriptor), refId, secIdx, tercIdx, out multipleOutput)
                        : GetTargetFont(instance, textDescriptor).DrawString(ModInstance.Controller, $"<PARAM IS NOT SET>", default, FontServer.instance.ScaleEffective);
                case TextContent.ParameterizedSpriteFolder:
                ImageFolder:
                    return !(tpw is null)
                        ? tpw.GetSpriteFromCycle(textDescriptor, instance.TargetAssetParameter, refId, secIdx, tercIdx)
                        : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet");
                case TextContent.ParameterizedSpriteSingle:
                ImageSingle:
                    return !(tpw is null)
                        ? tpw.GetSpriteFromParameter(instance.TargetAssetParameter)
                        : ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsNotSet");
                case TextContent.Any:
                case TextContent.TextParameterSequence:
                    if (!(tpw is null))
                    {
                        switch (tpw.ParamType)
                        {
                            case ParameterType.TEXT:
                            case ParameterType.VARIABLE:
                                goto Text;
                            case ParameterType.IMAGE:
                                goto ImageSingle;
                            case ParameterType.FOLDER:
                                goto ImageFolder;
                            case ParameterType.EMPTY:
                                break;
                        }
                    }
                    return GetTargetFont(instance, textDescriptor).DrawString(ModInstance.Controller, $"<SEQ PARAM IS NOT SET>", default, FontServer.instance.ScaleEffective); ;
                case TextContent.LinesNameList:
                    break;
                case TextContent.HwShield:
                    break;
                case TextContent.TimeTemperature:
                    break;
                case TextContent.LinesSymbols:
                    break;
            }
            return null;
        }

        public BasicRenderInformation GetSpriteFromParameter(PrefabInfo prop)
            => IsEmpty
                    ? null
                    : GetImageBRI(prop);
        public BasicRenderInformation GetImageBRI(PrefabInfo prefab)
        {
            if (ParamType == ParameterType.IMAGE)
            {
                if (isLocal)
                {
                    return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(atlasName, TextOrSpriteValue, true);
                }
                else
                {
                    UpdatePrefabInfo(prefab);
                    return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(cachedPrefabId.ToString(), TextOrSpriteValue, cachedPrefabId == default) ?? ModInstance.Controller.AtlasesLibrary.GetFromAssetAtlases(cachedPrefabId, TextOrSpriteValue, true);
                }
            }
            else
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsImageRequired");
            }

        }

        public BasicRenderInformation GetTargetText(BaseWriteOnXml descriptorBuilding, BoardTextDescriptorGeneralXml textDescriptor, DynamicSpriteFont targetFont, ushort refId, int secId, int tercId, out IEnumerable<BasicRenderInformation> multipleOutput)
        {
            if (ParamType != ParameterType.VARIABLE)
            {
                multipleOutput = null;
                return targetFont?.DrawString(ModInstance.Controller, ToString(), default, FontServer.instance.ScaleEffective);
            }
            else
            {
                return VariableValue.GetTargetText(descriptorBuilding, textDescriptor, targetFont, refId, secId, tercId, out multipleOutput);
            }
        }

        public string GetOriginalVariableParam() => ParamType != ParameterType.VARIABLE ? null : VariableValue.m_originalCommand;


        public BasicRenderInformation GetSpriteFromCycle(BoardTextDescriptorGeneralXml textDescriptor, PrefabInfo cachedPrefab, ushort refId, int boardIdx, int secIdx)
        {
            if (IsEmpty)
            {
                return null;
            }
            if (ParamType != TextParameterWrapper.ParameterType.FOLDER)
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsFolderRequired");
            }
            if (textDescriptor.AnimationSettings.m_itemCycleFramesDuration < 1)
            {
                textDescriptor.AnimationSettings.m_itemCycleFramesDuration = 100;
            }
            return GetCurrentSprite(cachedPrefab, (int length) => (int)(((SimulationManager.instance.m_currentFrameIndex + textDescriptor.AnimationSettings.m_extraDelayCycleFrames + (refId * (1 + boardIdx) + (11345476 * secIdx))) % (length * textDescriptor.AnimationSettings.m_itemCycleFramesDuration) / textDescriptor.AnimationSettings.m_itemCycleFramesDuration)));
        }
        private BasicRenderInformation GetCurrentSprite(PrefabInfo prefab, Func<int, int> p)
        {
            if (ParamType == ParameterType.FOLDER)
            {
                if (isLocal)
                {
                    return ModInstance.Controller.AtlasesLibrary.GetSlideFromLocal(atlasName, p, true);
                }
                else
                {
                    UpdatePrefabInfo(prefab);
                    return ModInstance.Controller.AtlasesLibrary.GetSlideFromLocal(cachedPrefabId.ToString(), p, cachedPrefabId == default) ?? ModInstance.Controller.AtlasesLibrary.GetSlideFromAsset(cachedPrefabId, p, true);
                }
            }
            else
            {
                return ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, "FrameParamsFolderRequired");
            }

        }
        #endregion
    }
}
