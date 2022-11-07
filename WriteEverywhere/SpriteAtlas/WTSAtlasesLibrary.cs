using ColossalFramework;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.Localization;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Plugins.Ext;
using WriteEverywhere.Utils;
using static ColossalFramework.UI.UITextureAtlas;

namespace WriteEverywhere.Sprites
{
    public class WTSAtlasesLibrary : MonoBehaviour
    {

        protected void Awake()
        {
            ReloadAssetImages();
            LoadImagesFromLocalFolders();
        }

        public void ReloadAssetImages()
        {
            foreach (var asset in
                VehiclesIndexes.instance.PrefabsData
                .Concat(BuildingIndexes.instance.PrefabsData)
                .Where(x => x.Value.PackageName.TrimToNull() != null)
                .Select(x => Tuple.New(x, KFileUtils.GetRootFolderForK45(x.Value.Info)))
                .GroupBy(x => x.Second)
                .Select(x => x.First())
                )
            {
                if (asset.Second is string str)
                {
                    var filePath = Path.Combine(str, WEMainController.EXTRA_SPRITES_FILES_FOLDER_ASSETS);
                    LogUtils.DoLog($"Trying load path: {filePath}");
                    if (Directory.Exists(filePath))
                        CreateAtlasEntry(AssetAtlases, AssetEntryNameFromData(asset.First.Value), filePath, asset.First.Value.Info);
                }
            }
        }
        #region Imported atlas

        public const string PROTOCOL_IMAGE = "image://";
        public const string PROTOCOL_IMAGE_ASSET = "assetImage://";
        public const string PROTOCOL_FOLDER = "folder://";
        public const string PROTOCOL_FOLDER_ASSET = "assetFolder://";
        private const string INTERNAL_ATLAS_NAME = @"\/INTERNAL\/";

        private Dictionary<string, Dictionary<string, WEImageInfo>> LocalAtlases { get; } = new Dictionary<string, Dictionary<string, WEImageInfo>>();
        private Dictionary<string, Dictionary<string, WEImageInfo>> AssetAtlases { get; } = new Dictionary<string, Dictionary<string, WEImageInfo>>();
        private Dictionary<string, Dictionary<string, BasicRenderInformation>> LocalAtlasesCache { get; } = new Dictionary<string, Dictionary<string, BasicRenderInformation>>();
        private Dictionary<string, Dictionary<string, BasicRenderInformation>> AssetAtlasesCache { get; } = new Dictionary<string, Dictionary<string, BasicRenderInformation>>();


        #region Getters

        public void GetSpriteLib(string atlasName, out Dictionary<string, WEImageInfo> result)
        {
            if (!LocalAtlases.TryGetValue(atlasName ?? string.Empty, out result))
            {
                AssetAtlases.TryGetValue(atlasName ?? string.Empty, out result);
            }
        }

        public string[] GetSpritesFromLocalAtlas(string atlasName) => LocalAtlases.TryGetValue(atlasName ?? string.Empty, out Dictionary<string, WEImageInfo> atlas) ? atlas.Keys.ToArray() : null;
        public string[] GetSpritesFromAssetAtlas(string packageName) => AssetAtlases.TryGetValue(packageName, out Dictionary<string, WEImageInfo> atlas) ? atlas.Keys.ToArray() : null;
        public bool HasAssetAtlas(IIndexedPrefabData prefabData) => AssetAtlases.ContainsKey(AssetEntryNameFromData(prefabData));

        internal BasicRenderInformation GetFromLocalAtlases(WEImages image)
        {
            return GetFromLocalAtlases(INTERNAL_ATLAS_NAME, image.ToString());
        }
        public BasicRenderInformation GetFromLocalAtlases(string atlasName, string spriteName, bool fallbackOnInvalid = false)
        {
            if (spriteName.IsNullOrWhiteSpace())
            {
                return fallbackOnInvalid ? GetFromLocalAtlases(WEImages.FrameParamsInvalidImage) : null;
            }

            if (LocalAtlasesCache.TryGetValue(atlasName ?? string.Empty, out Dictionary<string, BasicRenderInformation> resultDicCache) && resultDicCache.TryGetValue(spriteName ?? "", out BasicRenderInformation cachedInfo))
            {
                return cachedInfo;
            }
            if (!LocalAtlases.TryGetValue(atlasName ?? string.Empty, out Dictionary<string, WEImageInfo> atlas) || !atlas.ContainsKey(spriteName))
            {
                return fallbackOnInvalid ? GetFromLocalAtlases(WEImages.FrameParamsInvalidImage) : null;
            }
            if (resultDicCache == null)
            {
                LocalAtlasesCache[atlasName ?? string.Empty] = new Dictionary<string, BasicRenderInformation>();
            }


            LocalAtlasesCache[atlasName ?? string.Empty][spriteName] = null;

            StartCoroutine(CreateItemAtlasCoroutine(LocalAtlases, LocalAtlasesCache, atlasName ?? string.Empty, spriteName));
            return null;
        }
        public BasicRenderInformation GetSlideFromLocal(string atlasName, Func<int, int> idxFunc, bool fallbackOnInvalid = false) => !LocalAtlases.TryGetValue(atlasName ?? string.Empty, out Dictionary<string, WEImageInfo> atlas)
                ? fallbackOnInvalid ? GetFromLocalAtlases(WEImages.FrameParamsInvalidFolder) : null
                : SceneUtils.IsAssetEditor ? ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(WEImages.FrameBorder) : GetFromLocalAtlases(atlasName ?? string.Empty, atlas.Keys.ElementAt(idxFunc(atlas.Count - 1) + 1), fallbackOnInvalid);
        public BasicRenderInformation GetSlideFromAsset(IIndexedPrefabData prefabData, Func<int, int> idxFunc, bool fallbackOnInvalid = false)
            => !AssetAtlases.TryGetValue(AssetEntryNameFromData(prefabData), out Dictionary<string, WEImageInfo> atlas)
                ? fallbackOnInvalid ? GetFromLocalAtlases(WEImages.FrameParamsInvalidFolder) : null
                : SceneUtils.IsAssetEditor
                    ? ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(WEImages.FrameBorder)
                    : GetFromAssetAtlases(prefabData, atlas.Keys.ElementAt(idxFunc(atlas.Count - 1) + 1), fallbackOnInvalid);

        public BasicRenderInformation GetFromAssetAtlases(IIndexedPrefabData prefabData, string spriteName, bool fallbackOnInvalid = false)
        {
            var assetId = AssetEntryNameFromData(prefabData);
            if (spriteName.IsNullOrWhiteSpace() || !AssetAtlases.ContainsKey(assetId))
            {
                return null;
            }
            if (AssetAtlasesCache.TryGetValue(assetId, out Dictionary<string, BasicRenderInformation> resultDicCache) && resultDicCache.TryGetValue(spriteName ?? "", out BasicRenderInformation cachedInfo))
            {
                return cachedInfo;
            }
            if (!AssetAtlases.TryGetValue(assetId, out Dictionary<string, WEImageInfo> atlas) || !atlas.ContainsKey(spriteName))
            {
                return fallbackOnInvalid ? GetFromLocalAtlases(WEImages.FrameParamsInvalidImageAsset) : null;
            }
            if (resultDicCache == null)
            {
                AssetAtlasesCache[assetId] = new Dictionary<string, BasicRenderInformation>();
            }

            AssetAtlasesCache[assetId][spriteName] = null;
            StartCoroutine(CreateItemAtlasCoroutine(AssetAtlases, AssetAtlasesCache, assetId, spriteName));
            return null;
        }
        private IEnumerator CreateItemAtlasCoroutine<T>(Dictionary<T, Dictionary<string, WEImageInfo>> spriteDict, Dictionary<T, Dictionary<string, BasicRenderInformation>> spriteDictCache, T assetId, string spriteName)
        {
            yield return 0;
            if (!spriteDict.TryGetValue(assetId, out var targetAtlas))
            {
                LogUtils.DoWarnLog($"ATLAS NOT FOUND: {assetId}");
                yield break;
            }
            if (targetAtlas[spriteName] is null)
            {
                LogUtils.DoWarnLog($"SPRITE NOT FOUND: {spriteName}");
                yield break;
            }
            var info = targetAtlas[spriteName];
            var bri = WERenderingHelper.GenerateBri(info.Texture, info.Borders, info.PixelsPerMeter);
            RegisterMesh(spriteName, bri, spriteDictCache[assetId]);
            yield break;
        }
        #endregion

        #region Filtering
        internal string[] FindByInLocal(string targetAtlas, string searchName, out Dictionary<string, WEImageInfo> atlas) => LocalAtlases.TryGetValue(targetAtlas ?? string.Empty, out atlas)
              ? atlas.Keys.Where((x, i) => x.ToLower().Contains(searchName.ToLower())).Select(x => $"{(targetAtlas.IsNullOrWhiteSpace() ? "<ROOT>" : targetAtlas)}/{x}").OrderBy(x => x).ToArray()
              : (new string[0]);
        internal string[] FindByInLocalSimple(string targetAtlas, string searchName, out Dictionary<string, WEImageInfo> atlas) => LocalAtlases.TryGetValue(targetAtlas ?? string.Empty, out atlas)
              ? atlas.Keys.Where((x, i) => x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray()
              : (new string[0]);

        internal string[] FindByInAssetSimple(IIndexedPrefabData prefabData, string searchName, out Dictionary<string, WEImageInfo> atlas)
            => AssetAtlases.TryGetValue(AssetEntryNameFromData(prefabData), out atlas)
                ? atlas.Keys.Where((x, i) => x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray()
                : (new string[0]);
        internal string[] FindByInLocalFolders(string searchName) => LocalAtlases.Keys.Select(x => x == string.Empty ? "<ROOT>" : x).Where(x => x.ToLower().Contains(searchName.ToLower())).OrderBy(x => x).ToArray();


        #endregion

        #region Loading
        public void LoadImagesFromLocalFolders()
        {
            LocalAtlases.Clear();
            var errors = new List<string>();
            var folders = new string[] { WEMainController.ExtraSpritesFolder }.Concat(Directory.GetDirectories(WEMainController.ExtraSpritesFolder));
            foreach (var dir in folders)
            {
                bool isRoot = dir == WEMainController.ExtraSpritesFolder;
                var spritesToAdd = new List<WEImageInfo>();
                WTSAtlasLoadingUtils.LoadAllImagesFromFolderRef(dir, ref spritesToAdd, ref errors, false);
                if (isRoot || (!SceneUtils.IsAssetEditor && spritesToAdd.Count > 0))
                {
                    var atlasName = isRoot ? string.Empty : Path.GetFileNameWithoutExtension(dir);
                    LocalAtlases[atlasName] = new Dictionary<string, WEImageInfo>();
                    if (isRoot)
                    {
                        spritesToAdd.AddRange(UIView.GetAView().defaultAtlas.sprites.Where(x => x?.texture != null).Select(x => CloneWEImageInfo(x)).ToList());
                    }
                    foreach (var entry in spritesToAdd)
                    {
                        LocalAtlases[atlasName][entry.Name] = entry;
                    }
                }
            }
            LocalAtlases[INTERNAL_ATLAS_NAME] = new Dictionary<string, WEImageInfo>();
            foreach (var img in Enum.GetValues(typeof(WEImages)).Cast<WEImages>())
            {
                LocalAtlases[INTERNAL_ATLAS_NAME][img.ToString()] = new WEImageInfo(null) { Texture = KResourceLoader.LoadTextureMod(img.ToString()) };
            }
            LocalAtlasesCache.Clear();
            if (errors.Count > 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    message = $"{Str.WTS_CUSTOMSPRITE_ERRORHEADER}:",
                    scrollText = $"\t{string.Join("\n\t", errors.ToArray())}",
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

        private WEImageInfo CloneWEImageInfo(SpriteInfo x) => x is null ? null : new WEImageInfo(null)
        {
            Borders = new Vector4(x.border.left / x.width, x.border.bottom / x.height, x.border.right / x.width, x.border.top / x.height),
            Name = x.name,
            Texture = x.texture,
            PixelsPerMeter = 100
        };


        private static string AssetEntryNameFromData(IIndexedPrefabData vi)
        {
            return vi.WorkshopId != ~0ul ? vi.WorkshopId.ToString() : vi.PackageName;
        }

        private void CreateAtlasEntry<T>(Dictionary<T, Dictionary<string, WEImageInfo>> atlasDic, T atlasName, string path, PrefabInfo info)
        {
            WTSAtlasLoadingUtils.LoadAllImagesFromFolder(path, out List<WEImageInfo> spritesToAdd, out List<string> errors, false);
            foreach (string error in errors)
            {
                LogUtils.DoErrorLog($"ERROR LOADING IMAGE: {error}");
            }
            if (errors.Count > 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    message = string.Format(Str.we_general_errorsLoadingImagesFromAssetSpriteFolderHeader, $"{info.GetUncheckedLocalizedTitle()} ({info.name})"),
                    scrollText = "\t-" + string.Join("\n\t-", errors.ToArray()),
                    buttons = KwyttoDialog.basicOkButtonBar
                });
            }

            atlasDic[atlasName] = spritesToAdd.ToDictionary(x => x.Name, x => x);
        }
        #endregion

        #endregion

        #region Geometry




        private static void RegisterMesh(string sprite, BasicRenderInformation bri, Dictionary<string, BasicRenderInformation> cache)
        {
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
                    m_mesh = WERenderingHelper.basicMesh,
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
                WTSUtils.SolveTangents(m_bgTexture.m_mesh);
            }
            return m_bgTexture;
        }
        #endregion
    }
}