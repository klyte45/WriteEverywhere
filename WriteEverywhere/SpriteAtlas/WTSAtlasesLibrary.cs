extern alias TLM;
using ColossalFramework;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TLM::Bridge_WE2TLM;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Localization;
using WriteEverywhere.Rendering;
using WriteEverywhere.Utils;
using static ColossalFramework.UI.UITextureAtlas;

namespace WriteEverywhere.Sprites
{
    public class WTSAtlasesLibrary : MonoBehaviour
    {

        protected void Awake()
        {
            KFileUtils.ScanPrefabsFoldersDirectory<VehicleInfo>(MainController.EXTRA_SPRITES_FILES_FOLDER_ASSETS, LoadImagesFromPrefab);
            KFileUtils.ScanPrefabsFoldersDirectory<BuildingInfo>(MainController.EXTRA_SPRITES_FILES_FOLDER_ASSETS, LoadImagesFromPrefab);
            KFileUtils.ScanPrefabsFoldersDirectory<PropInfo>(MainController.EXTRA_SPRITES_FILES_FOLDER_ASSETS, LoadImagesFromPrefab);

            ResetTransportAtlas();
            TransportManager.instance.eventLineColorChanged += (x) => PurgeLine(new WTSLine(x, false));
            TransportManager.instance.eventLineNameChanged += (x) => PurgeLine(new WTSLine(x, false));

            LoadImagesFromLocalFolders();
        }

        protected void Start() => ModInstance.Controller.EventFontsReloadedFromFolder += ResetTransportAtlas;

        #region Imported atlas

        public const string PROTOCOL_IMAGE = "image://";
        public const string PROTOCOL_IMAGE_ASSET = "assetImage://";
        public const string PROTOCOL_FOLDER = "folder://";
        public const string PROTOCOL_FOLDER_ASSET = "assetFolder://";


        private Dictionary<string, UITextureAtlas> LocalAtlases { get; } = new Dictionary<string, UITextureAtlas>();
        private Dictionary<ulong, UITextureAtlas> AssetAtlases { get; } = new Dictionary<ulong, UITextureAtlas>();
        private Dictionary<string, Dictionary<string, BasicRenderInformation>> LocalAtlasesCache { get; } = new Dictionary<string, Dictionary<string, BasicRenderInformation>>();
        private Dictionary<ulong, Dictionary<string, BasicRenderInformation>> AssetAtlasesCache { get; } = new Dictionary<ulong, Dictionary<string, BasicRenderInformation>>();


        #region Getters

        public void GetAtlas(string atlasName, out UITextureAtlas result)
        {
            if (!LocalAtlases.TryGetValue(atlasName ?? string.Empty, out result) && ulong.TryParse(atlasName ?? string.Empty, out ulong workshopId))
            {
                AssetAtlases.TryGetValue(workshopId, out result);
            }
        }

        public string[] GetSpritesFromLocalAtlas(string atlasName) => LocalAtlases.TryGetValue(atlasName ?? string.Empty, out UITextureAtlas atlas) ? atlas.spriteNames : null;
        public string[] GetSpritesFromAssetAtlas(ulong workshopId) => AssetAtlases.TryGetValue(workshopId, out UITextureAtlas atlas) ? atlas.spriteNames : null;
        public bool HasAtlas(ulong workshopId) => AssetAtlases.TryGetValue(workshopId, out _);
        public BasicRenderInformation GetFromLocalAtlases(string atlasName, string spriteName, bool fallbackOnInvalid = false)
        {
            if (spriteName.IsNullOrWhiteSpace())
            {
                return GetFromLocalAtlases(null, "FrameParamsInvalidImage");
            }

            if (LocalAtlasesCache.TryGetValue(atlasName ?? string.Empty, out Dictionary<string, BasicRenderInformation> resultDicCache) && resultDicCache.TryGetValue(spriteName ?? "", out BasicRenderInformation cachedInfo))
            {
                return cachedInfo;
            }
            if (!LocalAtlases.TryGetValue(atlasName ?? string.Empty, out UITextureAtlas atlas) || !atlas.spriteNames.Contains(spriteName))
            {
                return fallbackOnInvalid ? GetFromLocalAtlases(null, "FrameParamsInvalidImage") : null;
            }
            if (resultDicCache == null)
            {
                LocalAtlasesCache[atlasName ?? string.Empty] = new Dictionary<string, BasicRenderInformation>();
            }


            LocalAtlasesCache[atlasName ?? string.Empty][spriteName] = null;

            StartCoroutine(CreateItemAtlasCoroutine(LocalAtlases, LocalAtlasesCache, atlasName ?? string.Empty, spriteName));
            return null;
        }
        public BasicRenderInformation GetSlideFromLocal(string atlasName, Func<int, int> idxFunc, bool fallbackOnInvalid = false) => !LocalAtlases.TryGetValue(atlasName ?? string.Empty, out UITextureAtlas atlas)
                ? fallbackOnInvalid ? GetFromLocalAtlases(null, "FrameParamsInvalidFolder") : null
                : GetFromLocalAtlases(atlasName ?? string.Empty, atlas.spriteNames[idxFunc(atlas.spriteNames.Length - 1) + 1], fallbackOnInvalid);
        public BasicRenderInformation GetSlideFromAsset(ulong assetId, Func<int, int> idxFunc, bool fallbackOnInvalid = false) => !AssetAtlases.TryGetValue(assetId, out UITextureAtlas atlas)
                ? fallbackOnInvalid ? GetFromLocalAtlases(null, "FrameParamsInvalidFolder") : null
                : GetFromAssetAtlases(assetId, atlas.spriteNames[idxFunc(atlas.spriteNames.Length - 1) + 1], fallbackOnInvalid);

        public BasicRenderInformation GetFromAssetAtlases(ulong assetId, string spriteName, bool fallbackOnInvalid = false)
        {
            if (spriteName.IsNullOrWhiteSpace() || !AssetAtlases.ContainsKey(assetId))
            {
                return null;
            }
            if (AssetAtlasesCache.TryGetValue(assetId, out Dictionary<string, BasicRenderInformation> resultDicCache) && resultDicCache.TryGetValue(spriteName ?? "", out BasicRenderInformation cachedInfo))
            {
                return cachedInfo;
            }
            if (!AssetAtlases.TryGetValue(assetId, out UITextureAtlas atlas) || !atlas.spriteNames.Contains(spriteName))
            {
                return fallbackOnInvalid ? GetFromLocalAtlases(null, "FrameParamsInvalidImageAsset") : null;
            }
            if (resultDicCache == null)
            {
                AssetAtlasesCache[assetId] = new Dictionary<string, BasicRenderInformation>();
            }

            AssetAtlasesCache[assetId][spriteName] = null;
            StartCoroutine(CreateItemAtlasCoroutine(AssetAtlases, AssetAtlasesCache, assetId, spriteName));
            return null;
        }
        private IEnumerator CreateItemAtlasCoroutine<T>(Dictionary<T, UITextureAtlas> spriteDict, Dictionary<T, Dictionary<string, BasicRenderInformation>> spriteDictCache, T assetId, string spriteName)
        {
            yield return 0;
            if (!spriteDict.TryGetValue(assetId, out UITextureAtlas targetAtlas))
            {
                LogUtils.DoWarnLog($"ATLAS NOT FOUND: {assetId}");
                yield break;
            }
            if (targetAtlas[spriteName] is null)
            {
                LogUtils.DoWarnLog($"SPRITE NOT FOUND: {spriteName}");
                yield break;
            }
            var bri = new BasicRenderInformation
            {
                m_YAxisOverflows = new RangeVector { min = 0, max = 20 },
                m_refText = $"<sprite asset,{assetId},{spriteName}>"
            };
            BuildMeshFromAtlas(bri, targetAtlas[spriteName]);
            RegisterMesh(spriteName, bri, spriteDictCache[assetId]);
            yield break;
        }
        #endregion

        #region Filtering
        internal string[] FindByInLocal(string targetAtlas, string searchName, out UITextureAtlas atlas) => LocalAtlases.TryGetValue(targetAtlas ?? string.Empty, out atlas)
              ? atlas.spriteNames.Where((x, i) => i > 0 && x.ToLower().Contains(searchName.ToLower())).Select(x => $"{(targetAtlas.IsNullOrWhiteSpace() ? "<ROOT>" : targetAtlas)}/{x}").OrderBy(x => x).ToArray()
              : (new string[0]);
        internal string[] FindByInLocalSimple(string targetAtlas, string searchName, out UITextureAtlas atlas) => LocalAtlases.TryGetValue(targetAtlas ?? string.Empty, out atlas)
              ? atlas.spriteNames.Where((x, i) => i > 0 && x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray()
              : (new string[0]);
        internal string[] FindByInAsset(ulong assetId, string searchName, out UITextureAtlas atlas, bool asRoot = false) => AssetAtlases.TryGetValue(assetId, out atlas)
                ? atlas.spriteNames.Where((x, i) => i > 0 && x.ToLower().Contains(searchName.ToLower())).Select(x => $"{(asRoot ? "<ROOT>" : assetId.ToString())}/{x}").OrderBy(x => x).ToArray()
                : (new string[0]);
        internal string[] FindByInAssetSimple(ulong assetId, string searchName, out UITextureAtlas atlas) => AssetAtlases.TryGetValue(assetId, out atlas)
                ? atlas.spriteNames.Where((x, i) => i > 0 && x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray()
                : (new string[0]);
        internal string[] FindByInLocalFolders(string searchName) => LocalAtlases.Keys.Select(x => x == string.Empty ? "<ROOT>" : x).Where(x => x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray();

        internal string[] OnFilterParamImagesAndFoldersByText(UISprite sprite, string inputText, string propName, out string protocolFound)
        {
            Match match;
            if ((inputText?.Length ?? 0) >= 4 && (match = Regex.Match(inputText ?? "", $"^({PROTOCOL_IMAGE}|{PROTOCOL_IMAGE_ASSET}|{PROTOCOL_FOLDER}|{PROTOCOL_FOLDER_ASSET})(([^/]+)/)?(.*)$")).Success)
            {
                protocolFound = match.Groups[1].Value;
                var subfolder = match.Groups[3].Value;


                var searchName = match.Groups[4].Value;
                string[] results;
                UITextureAtlas atlas;
                switch (protocolFound)
                {
                    default:
                    case PROTOCOL_IMAGE:
                        results = (subfolder.IsNullOrWhiteSpace() ? ModInstance.Controller.AtlasesLibrary.FindByInLocalFolders(searchName).Select(x => $"{x}/") : new List<string>()).Concat(ModInstance.Controller.AtlasesLibrary.FindByInLocal(subfolder == "<ROOT>" ? null : subfolder, searchName, out atlas)).ToArray();
                        break;
                    case PROTOCOL_IMAGE_ASSET:
                        results = ModInstance.Controller.AtlasesLibrary.FindByInAsset(ulong.TryParse(propName?.Split('.')[0] ?? "", out ulong wId) ? wId : 0u, searchName, out atlas, true);
                        break;
                    case PROTOCOL_FOLDER:
                        results = ModInstance.Controller.AtlasesLibrary.FindByInLocalFolders(searchName);
                        atlas = null;
                        break;
                    case PROTOCOL_FOLDER_ASSET:
                        results = AssetAtlases.TryGetValue(ulong.TryParse(propName?.Split('.')[0] ?? "", out wId) ? wId : 0, out atlas) ? new string[] { "<ROOT>" } : new string[0];
                        break;
                }
                sprite.atlas = atlas;
                return results;
            }
            else
            {
                protocolFound = null;
                return null;
            }
        }
        #endregion

        #region Loading
        public void LoadImagesFromLocalFolders()
        {
            LocalAtlases.Clear();
            var errors = new List<string>();
            var folders = new string[] { MainController.ExtraSpritesFolder }.Concat(Directory.GetDirectories(MainController.ExtraSpritesFolder));
            foreach (var dir in folders)
            {
                bool isRoot = dir == MainController.ExtraSpritesFolder;
                var spritesToAdd = new List<SpriteInfo>();
                WTSAtlasLoadingUtils.LoadAllImagesFromFolderRef(dir, ref spritesToAdd, ref errors, isRoot);
                if (isRoot || spritesToAdd.Count > 0)
                {
                    var atlasName = isRoot ? string.Empty : Path.GetFileNameWithoutExtension(dir);
                    LocalAtlases[atlasName] = ScriptableObject.CreateInstance<UITextureAtlas>();
                    LocalAtlases[atlasName].material = new Material(ModInstance.Controller.defaultTextShader);
                    if (isRoot)
                    {
                        spritesToAdd.AddRange(UIView.GetAView().defaultAtlas.sprites.Select(x => CloneSpriteInfo(x)).ToList());
                    }
                    TextureAtlasUtils.RegenerateTextureAtlas(LocalAtlases[atlasName], spritesToAdd);
                }
            }
            LocalAtlasesCache.Clear();
            if (errors.Count > 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    message = $"{Str.WTS_CUSTOMSPRITE_ERRORHEADER}:\n\t{string.Join("\n\t", errors.ToArray())}",
                    buttons = new[]{
                        KwyttoDialog.SpaceBtn,
                        new KwyttoDialog.ButtonDefinition
                        {
                            title = KStr.comm_releaseNotes_Ok,
                            onClick=() => true,
                        },
                        KwyttoDialog.SpaceBtn
                    }

                });
            }
        }

        private SpriteInfo CloneSpriteInfo(SpriteInfo x) => new SpriteInfo
        {
            border = x.border,
            name = x.name,
            region = default,
            texture = x.texture
        };

        private void LoadImagesFromPrefab(ulong workshopId, string directoryPath, PrefabInfo info)
        {
            if (workshopId > 0 && workshopId != ~0UL && !AssetAtlases.ContainsKey(workshopId))
            {
                CreateAtlasEntry(AssetAtlases, workshopId, directoryPath, false);
            }

        }

        private UITextureAtlas CreateAtlasEntry<T>(Dictionary<T, UITextureAtlas> atlasDic, T atlasName, string path, bool addPrefix)
        {
            UITextureAtlas targetAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            targetAtlas.material = new Material(ModInstance.Controller.defaultTextShader);
            WTSAtlasLoadingUtils.LoadAllImagesFromFolder(path, out List<SpriteInfo> spritesToAdd, out List<string> errors, addPrefix);
            TextureAtlasUtils.RegenerateTextureAtlas(targetAtlas, spritesToAdd);
            foreach (string error in errors)
            {
                LogUtils.DoErrorLog($"ERROR LOADING IMAGE: {error}");
            }
            atlasDic[atlasName] = targetAtlas;
            return targetAtlas;
        }
        #endregion

        #endregion

        #region Transport lines
        private UITextureAtlas m_transportLineAtlas;
        private Dictionary<int, BasicRenderInformation> TransportLineCache { get; } = new Dictionary<int, BasicRenderInformation>();
        private Dictionary<int, BasicRenderInformation> RegionalTransportLineCache { get; } = new Dictionary<int, BasicRenderInformation>();

        private bool TransportIsDirty { get; set; }
        private void ResetTransportAtlas()
        {
            m_transportLineAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            m_transportLineAtlas.material = new Material(ModInstance.Controller.defaultTextShader);
        }
        internal void PurgeLine(WTSLine line)
        {
            string id = $"{line.ToExternalRefId()}";
            if (!(m_transportLineAtlas[id] is null))
            {
                m_transportLineAtlas.Remove(id);
            }
            (line.regional ? RegionalTransportLineCache : TransportLineCache).Remove(line.lineId);
            TransportIsDirty = true;
        }
        public void PurgeAllLines()
        {
            TransportLineCache.Clear();
            RegionalTransportLineCache.Clear();
            ResetTransportAtlas();
            TransportIsDirty = true;
        }
        public CommonsSpriteNames LineIconTest
        {
            get => m_lineIconTest; set
            {
                m_lineIconTest = value;
                PurgeLine(new WTSLine(0, false));
            }
        }
        private CommonsSpriteNames m_lineIconTest = CommonsSpriteNames.K45_HexagonIcon;
        internal List<BasicRenderInformation> DrawLineFormats(IEnumerable<WTSLine> ids)
        {
            var bris = new List<BasicRenderInformation>();
            if (ids.Count() == 0)
            {
                return bris;
            }

            foreach (var id in ids.OrderBy(x =>
             x.lineId < 0 ? x.lineId.ToString("D6") : ModInstance.Controller.ConnectorTLM.GetLineSortString(x)
            ))
            {
                if ((id.regional ? RegionalTransportLineCache : TransportLineCache).TryGetValue(id.lineId, out BasicRenderInformation bri))
                {
                    if (bri != null)
                    {
                        bris.Add(bri);
                    }
                }
                else
                {
                    (id.regional ? RegionalTransportLineCache : TransportLineCache)[id.lineId] = null;
                    StartCoroutine(WriteTransportLineTextureCoroutine(id));
                }
            }
            return bris;

        }
        private IEnumerator WriteTransportLineTextureCoroutine(WTSLine line)
        {
            string id = $"{line.ToExternalRefId()}";
            if (m_transportLineAtlas[id] == null)
            {
                yield return 0;
                while (!CheckTransportLineCoroutineCanContinue())
                {
                    yield return null;
                }
                LineLogoParameter lineParams = line.ZeroLine ? new LineLogoParameter(LineIconTest.ToString(), (Color)ColorExtensions.FromRGB(0x5e35b1), "K")
                : line.lineId < 0 ? new LineLogoParameter(((CommonsSpriteNames)((-line.lineId % (Enum.GetValues(typeof(CommonsSpriteNames)).Length - 1)) + 1)).ToString(), WEDynamicTextRenderingRules.m_spectreSteps[(-line.lineId) % WEDynamicTextRenderingRules.m_spectreSteps.Length], $"{-line.lineId}")
                : ModInstance.Controller.ConnectorTLM.GetLineLogoParameters(line);
                if (lineParams == null || lineParams.color == Color.clear)
                {
                    yield break;
                }
                var drawingCoroutine = CoroutineWithData.From(this, RenderSpriteLine(
                    FontServer.instance[WTSEtcData.Instance.FontSettings.PublicTransportLineSymbolFont] ??
                    FontServer.instance[MainController.DEFAULT_FONT_KEY], UIView.GetAView().defaultAtlas, lineParams.fileName, lineParams.color, lineParams.text));
                yield return drawingCoroutine.Coroutine;
                while (!CheckTransportLineCoroutineCanContinue())
                {
                    yield return null;
                }

                TextureAtlasUtils.RegenerateTextureAtlas(m_transportLineAtlas, new List<UITextureAtlas.SpriteInfo>
                {
                    new UITextureAtlas.SpriteInfo
                    {
                        name = id,
                        texture = drawingCoroutine.result
                    }
                });
                TransportIsDirty = true;
                StopAllCoroutines();
                TransportLineCache.Clear();
                RegionalTransportLineCache.Clear();
                yield break;
            }
            yield return 0;
            var bri = new BasicRenderInformation
            {
                m_YAxisOverflows = new RangeVector { min = 0, max = 20 },
            };

            yield return 0;
            BuildMeshFromAtlas(bri, m_transportLineAtlas[id]);
            yield return 0;
            RegisterMeshSingle(line.lineId, bri, line.regional ? RegionalTransportLineCache : TransportLineCache);
            TransportIsDirty = false;
            yield break;
        }
        private static bool CheckTransportLineCoroutineCanContinue()
        {
            if (m_lastCoroutineStepTL != SimulationManager.instance.m_currentTickIndex)
            {
                m_lastCoroutineStepTL = SimulationManager.instance.m_currentTickIndex;
                m_coroutineCounterTL = 0;
            }
            if (m_coroutineCounterTL >= 1)
            {
                return false;
            }
            m_coroutineCounterTL++;
            return true;
        }
        private static uint m_lastCoroutineStepTL = 0;
        private static uint m_coroutineCounterTL = 0;
        public static IEnumerator<Texture2D> RenderSpriteLine(DynamicSpriteFont font, UITextureAtlas atlas, string spriteName, Color bgColor, string text, float textScale = 1)
        {
            if (font is null)
            {
                font = FontServer.instance[MainController.DEFAULT_FONT_KEY];
            }

            UITextureAtlas.SpriteInfo spriteInfo = atlas[spriteName];
            if (spriteInfo == null)
            {
                CODebugBase<InternalLogChannel>.Warn(InternalLogChannel.UI, "Missing sprite " + spriteName + " in " + atlas.name);
                yield break;
            }
            else
            {
                while (!CheckTransportLineCoroutineCanContinue())
                {
                    yield return null;
                }

                int height = spriteInfo.texture.height;
                int width = spriteInfo.texture.width;
                var formTexture = new Texture2D(width, height);
                formTexture.SetPixels(spriteInfo.texture.GetPixels());
                TextureScaler.scale(formTexture, width * 2, height * 2);
                Texture2D texText = font.DrawTextToTexture(text, 1);

                Color[] formTexturePixels = formTexture.GetPixels();
                int borderWidth = 8;
                height *= 2;
                width *= 2;


                int targetWidth = width + borderWidth;
                int targetHeight = height + borderWidth;
                TextureScaler.scale(formTexture, targetWidth, targetHeight);
                Color contrastColor = bgColor.ContrastColor();
                Color[] targetColorArray = formTexture.GetPixels().Select(x => new Color(contrastColor.r, contrastColor.g, contrastColor.b, x.a)).ToArray();
                Destroy(formTexture);
                var targetBorder = new RectOffset(spriteInfo.border.left * 2, spriteInfo.border.right * 2, spriteInfo.border.top * 2, spriteInfo.border.bottom * 2);

                float textBoundHeight = Mathf.Min(height * .66f, (height * .85f) - targetBorder.vertical);
                float textBoundWidth = ((width * .9f) - targetBorder.horizontal);

                var textAreaSize = new Vector4(
                    (1f - (textBoundWidth / width)) * (targetBorder.horizontal == 0 ? 0.5f : 1f * targetBorder.left / targetBorder.horizontal) * width,
                    height * (1f - (textBoundHeight / height)) * (targetBorder.vertical == 0 ? 0.5f : 1f * targetBorder.bottom / targetBorder.vertical),
                    textBoundWidth,
                    textBoundHeight);


                float proportionTexText = texText.width / texText.height;
                float proportionTextBound = textBoundWidth / textBoundHeight;
                float widthReducer = Mathf.Min(proportionTextBound / proportionTexText, 1);
                float heightReducer = Mathf.Min(widthReducer * 3, 1);
                float scaleTextTex = Mathf.Min(textAreaSize.z / (texText.width * widthReducer), textAreaSize.w / (texText.height * heightReducer));
                TextureScaler.scale(texText, Mathf.FloorToInt(texText.width * widthReducer * scaleTextTex), Mathf.FloorToInt(texText.height * heightReducer * scaleTextTex));

                Color[] textColors = texText.GetPixels();
                int textWidth = texText.width;
                int textHeight = texText.height;
                Destroy(texText);


                Task<Tuple<Color[], int, int>> task = ThreadHelper.taskDistributor.Dispatch(() =>
                {
                    TextureRenderUtils.MergeColorArrays(targetColorArray, targetWidth, formTexturePixels.Select(x => new Color(bgColor.r, bgColor.g, bgColor.b, x.a)).ToArray(), borderWidth / 2, borderWidth / 2, width, height);
                    Color[] textOutlineArray = textColors.Select(x => new Color(bgColor.r, bgColor.g, bgColor.b, x.a)).ToArray();
                    int topMerge = Mathf.RoundToInt((textAreaSize.y + ((textBoundHeight - textHeight) / 2)));
                    int leftMerge = Mathf.RoundToInt((textAreaSize.x + ((textBoundWidth - textWidth) / 2)));

                    for (int i = 0; i <= borderWidth / 2; i++)
                    {
                        for (int j = 0; j <= borderWidth / 2; j++)
                        {
                            TextureRenderUtils.MergeColorArrays(targetColorArray, targetWidth, textOutlineArray, leftMerge + i + (borderWidth / 4), topMerge + j + (borderWidth / 4), textWidth, textHeight);
                        }
                    }
                    TextureRenderUtils.MergeColorArrays(colorOr: targetColorArray,
                                                        widthOr: targetWidth,
                                                        colors: textColors.Select(x => new Color(contrastColor.r, contrastColor.g, contrastColor.b, x.a)).ToArray(),
                                                        startX: leftMerge + (borderWidth / 2),
                                                        startY: topMerge + (borderWidth / 2),
                                                        sizeX: textWidth,
                                                        sizeY: textHeight);
                    return Tuple.New(targetColorArray, targetWidth, targetHeight);
                });
                while (!task.hasEnded || m_coroutineCounterTL > 1)
                {
                    if (task.hasEnded)
                    {
                        m_coroutineCounterTL++;
                    }

                    yield return null;
                    if (m_lastCoroutineStepTL != SimulationManager.instance.m_currentTickIndex)
                    {
                        m_lastCoroutineStepTL = SimulationManager.instance.m_currentTickIndex;
                        m_coroutineCounterTL = 0;
                    }
                }
                m_coroutineCounterTL++;

                var targetTexture = new Texture2D(task.result.Second, task.result.Third, TextureFormat.RGBA32, false);
                targetTexture.SetPixels(task.result.First);
                targetTexture.Apply();
                yield return targetTexture;
            }
        }
        #endregion

        #region Geometry

        private static readonly int[] kTriangleIndices = new int[]    {
            0,
            1,
            3,
            3,
            1,
            2
        };

        public static readonly Mesh basicMesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(-.5f, -.5f, 0f),
                new Vector3(0.5f, -.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-.5f, 0.5f, 0f),
            },
            uv = new[]
            {
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            },
            triangles = kTriangleIndices
        };

        static WTSAtlasesLibrary()
        {
            WTSUtils.SolveTangents(basicMesh);
        }


        internal static void BuildMeshFromAtlas(BasicRenderInformation bri, SpriteInfo spriteInfo)
        {
            bri.m_mesh = basicMesh;
            bri.m_fontBaseLimits = new RangeVector { min = 0, max = 1 };
            bri.m_YAxisOverflows = new RangeVector { min = -.5f, max = .5f };
            bri.m_sizeMetersUnscaled = new Vector2(spriteInfo.width / spriteInfo.height, 1);
            bri.m_offsetScaleX = spriteInfo.width / spriteInfo.height;
            bri.m_generatedMaterial = new Material(FontServer.instance.DefaultShader)
            {
                mainTexture = spriteInfo.texture
            };
            bri.m_borders = new Vector4(spriteInfo.border.left, spriteInfo.border.top, spriteInfo.border.right, spriteInfo.border.bottom);
            bri.m_pixelDensityMeters = 100f;
            bri.m_lineOffset = .5f;
            bri.m_expandXIfAlone = true;
        }


        private static void RegisterMesh(string sprite, BasicRenderInformation bri, Dictionary<string, BasicRenderInformation> cache)
        {
            WTSUtils.SolveTangents(bri.m_mesh);
            RegisterMeshSingle(sprite, bri, cache);
        }

        internal static void RegisterMeshSingle<T>(T sprite, BasicRenderInformation bri, Dictionary<T, BasicRenderInformation> cache)
        {
            WTSUtils.SolveTangents(bri.m_mesh);
            if (cache.TryGetValue(sprite, out BasicRenderInformation currentVal) && currentVal == null)
            {
                cache[sprite] = bri;
            }
            else
            {
                cache.Remove(sprite);
            }
        }

        private BasicRenderInformation m_bgTexture;
        public BasicRenderInformation GetWhiteTextureBRI()
        {
            if (m_bgTexture == null)
            {
                m_bgTexture = new BasicRenderInformation
                {
                    m_mesh = WTSAtlasesLibrary.basicMesh,
                    m_fontBaseLimits = new RangeVector { min = 0, max = 1 },
                    m_YAxisOverflows = new RangeVector { min = -.5f, max = .5f },
                    m_sizeMetersUnscaled = new Vector2(1, 1),
                    m_offsetScaleX = 1,
                    m_generatedMaterial = new Material(ModInstance.Controller.defaultTextShader)
                    {
                        mainTexture = Texture2D.whiteTexture
                    },
                    m_borders = default,
                    m_pixelDensityMeters = 100f,
                    m_lineOffset = .5f,
                    m_expandXIfAlone = true
                };
                m_bgTexture.m_mesh.RecalculateBounds();
                m_bgTexture.m_mesh.RecalculateNormals();
                m_bgTexture.m_mesh.RecalculateTangents();
            }
            return m_bgTexture;
        }
        #endregion
    }
}