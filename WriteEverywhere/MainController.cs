using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Assets;
using WriteEverywhere.Data;
using WriteEverywhere.ModShared;
using WriteEverywhere.Rendering;
using WriteEverywhere.Singleton;
using WriteEverywhere.Sprites;
using WriteEverywhere.Tools;
using WriteEverywhere.UI;

namespace WriteEverywhere
{
    public class WEMainController : BaseController<ModInstance, WEMainController>
    {
        public static string FOLDER_PATH => ModInstance.ModSettingsRootFolder;

        #region Events
        public event Action EventFontsReloadedFromFolder;
        public event Action EventOnDistrictChanged;
        public event Action EventOnParkChanged;
        public event Action<ushort?> EventOnBuildingNameChanged;
        public event Action EventOnZeroMarkerChanged;
        public event Action EventOnPostalCodeChanged;
        #endregion

        #region Tool Access
        public RoadSegmentTool RoadSegmentToolInstance => ToolsModifierControl.toolController.GetComponent<RoadSegmentTool>();
        public BuildingEditorTool BuildingToolInstance => ToolsModifierControl.toolController.GetComponent<BuildingEditorTool>();
        #endregion

        #region Shader
        public WEAssetLibrary ShaderLib => WEAssetLibrary.instance;
        public Shader defaultTextShader { get; private set; } = WEAssetLibrary.instance.FontShader;
        public Shader defaultFrameShader { get; private set; } = Shader.Find("Custom/Buildings/Building/NoBase");
        public Shader defaultHighlightShader { get; private set; } = Shader.Find("Hidden/InternalErrorShader");

        public static Shader GetDefaultFrameShader()
        {
            return WEAssetLibrary.instance.GetShaders().TryGetValue("klyte/wts/wtsshaderframe", out Shader value) ? value : value;
        }
        public Material highlightMaterial { get; private set; }
        #endregion

        #region Fonts
        public const string DEFAULT_FONT_KEY = "/DEFAULT/";
        public const string FONTS_FILES_FOLDER = "Fonts";
        public static int DefaultTextureSizeFont => 512 << ModInstance.StartTextureSizeFont;
        public static string FontFilesPath { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + FONTS_FILES_FOLDER;
        public static void ReloadFontsFromPath()
        {
            FontServer.instance.ResetCollection();
            FontServer.instance.RegisterFont(DEFAULT_FONT_KEY, KResourceLoader.LoadResourceDataMod("UI.DefaultFont.SourceSansPro-Regular.ttf"), DefaultTextureSizeFont);
            KFileUtils.EnsureFolderCreation(FontFilesPath);
            foreach (string fontFile in Directory.GetFiles(FontFilesPath, "*.ttf"))
            {
                FontServer.instance.RegisterFont(Path.GetFileNameWithoutExtension(fontFile), File.ReadAllBytes(fontFile), DefaultTextureSizeFont);
            }
            ModInstance.Controller?.EventFontsReloadedFromFolder?.Invoke();
        }
        #endregion

        #region Prop extra files
        public const string DEFAULT_GAME_PROP_LAYOUT_FOLDER = "PropsDefaultLayouts";
        public const string m_defaultFileNamePropsXml = "WTS_DefaultPropsConfig";
        public static string DefaultPropsLayoutConfigurationFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + DEFAULT_GAME_PROP_LAYOUT_FOLDER;
        #endregion

        #region Sprites atlases
        public const string EXTRA_SPRITES_FILES_FOLDER = "Sprites";
        public const string EXTRA_SPRITES_FILES_FOLDER_ASSETS = "K45WTS_Sprites";
        public static string ExtraSpritesFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + EXTRA_SPRITES_FILES_FOLDER;
        internal WTSAtlasesLibrary AtlasesLibrary { get; private set; }
        #endregion

        #region Bridges
        public IBridgeCD ConnectorCD { get; } = BridgeUtils.GetMostPrioritaryImplementation<IBridgeCD>();
        public IBridgeTLM ConnectorTLM { get; } = BridgeUtils.GetMostPrioritaryImplementation<IBridgeTLM>();
        public IBridgeVS ConnectorVS { get; } = BridgeUtils.GetMostPrioritaryImplementation<IBridgeVS>();
        #endregion

        #region OnNet

        internal WTSOnNetPropsSingleton OnNetPropsSingleton { get; private set; }
        #endregion

        #region Vehicles
        public static string DefaultVehiclesConfigurationFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + DEFAULT_GAME_VEHICLES_CONFIG_FOLDER;
        public const string m_defaultFileNameVehiclesXml = "WE_DefaultVehiclesConfig";
        public const string DEFAULT_GAME_VEHICLES_CONFIG_FOLDER = "VehiclesDefaultPlacing";
        internal WTSVehicleTextsSingleton VehicleTextsSingleton { get; private set; }
        #endregion

        #region Buildings
        public const string DEFAULT_GAME_BUILDINGS_CONFIG_FOLDER = "BuildingsDefaultPlacing";
        public const string m_defaultFileNameBuildingsXml = "WTS_DefaultBuildingsConfig";
        public static string DefaultBuildingsConfigurationFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + DEFAULT_GAME_BUILDINGS_CONFIG_FOLDER;

        internal WTSBuildingPropsSingleton BuildingPropsSingleton { get; private set; }
        #endregion

        public CommandLevelSingleton CommandLevelSingleton => CommandLevelSingleton.Instance;

        public void Awake()
        {
            ToolsModifierControl.toolController.AddExtraToolToController<SegmentEditorPickerTool>();
            ToolsModifierControl.toolController.AddExtraToolToController<BuildingEditorTool>();
            ToolsModifierControl.toolController.AddExtraToolToController<RoadSegmentTool>();
            ToolsModifierControl.toolController.AddExtraToolToController<VehicleEditorTool>();

            KFileUtils.EnsureFolderCreation(DefaultBuildingsConfigurationFolder);
            KFileUtils.EnsureFolderCreation(DefaultVehiclesConfigurationFolder);
            KFileUtils.EnsureFolderCreation(DefaultPropsLayoutConfigurationFolder);
            KFileUtils.EnsureFolderCreation(FontFilesPath);
            KFileUtils.EnsureFolderCreation(ExtraSpritesFolder);

            FontServer.Ensure();
            FontServer.instance.SetQualityMultiplier(WESettingsQualityTab.m_qualityArray[ModInstance.FontQuality]);
            ReloadFontsFromPath();
            OnNetPropsSingleton = gameObject.AddComponent<WTSOnNetPropsSingleton>();
            VehicleTextsSingleton = gameObject.AddComponent<WTSVehicleTextsSingleton>();
            BuildingPropsSingleton = gameObject.AddComponent<WTSBuildingPropsSingleton>();
        }

        protected override void StartActions()
        {
            base.StartActions();

            GameObjectUtils.CreateElement<WTSOnNetLiteUI>(UIView.GetAView().gameObject.transform, "WTSOnNetLiteUI");
            GameObjectUtils.CreateElement<WTSVehicleLiteUI>(UIView.GetAView().gameObject.transform, "WTSVehicleLiteUI");
            GameObjectUtils.CreateElement<BuildingLiteUI>(UIView.GetAView().gameObject.transform, "BuildingLiteUI");
            highlightMaterial = new Material(defaultHighlightShader);
            AtlasesLibrary = gameObject.AddComponent<WTSAtlasesLibrary>();


            BuildingManager.instance.EventBuildingReleased += WTSBuildingData.Instance.CacheData.PurgeBuildingCache;
            BuildingManager.instance.EventBuildingRelocated += WTSBuildingData.Instance.CacheData.PurgeBuildingCache;

            EventSegmentNameChanged += OnNameSeedChanged;
            BuildingManager.instance.EventBuildingRelocated += WTSCacheSingleton.ClearCacheBuildingName;
            BuildingManager.instance.EventBuildingReleased += WTSCacheSingleton.ClearCacheBuildingName;
            BuildingManager.instance.EventBuildingCreated += WTSCacheSingleton.ClearCacheBuildingName;
            EventOnDistrictChanged += WTSCacheSingleton.ClearCacheDistrictName;
            EventOnParkChanged += WTSCacheSingleton.ClearCacheParkName;
            EventOnBuildingNameChanged += WTSCacheSingleton.ClearCacheBuildingName;
            EventOnZeroMarkerChanged += OnNameSeedChanged;

        }

        private IEnumerator OnNameSeedChanged(ushort segmentId)
        {
            yield return 0;
            OnNameSeedChanged();
        }
        private void OnNameSeedChanged()
        {
            WTSCacheSingleton.ClearCacheSegmentNameParam();
            WTSCacheSingleton.ClearCacheBuildingName(null);
        }

        public static FontServer fontServer = FontServer.instance;

        #region Changes handler

        public void OnBuildingNameChanged(ushort buildingId) => EventOnBuildingNameChanged?.Invoke(buildingId);
        public void OnSegmentNameChanged(ushort segmentId) => EventSegmentNameChanged?.Invoke(segmentId);

        public event Func<ushort, IEnumerator> EventNodeChanged;
        public event Func<ushort, IEnumerator> EventSegmentChanged;
        public event Func<ushort, IEnumerator> EventSegmentReleased;
        public event Func<ushort, IEnumerator> EventSegmentNameChanged;

        public event Action<ushort> EventOnLineUpdated;
        public event Action<ushort> EventOnLineBuildingUpdated;


        private int segmentsCooldown = 0;
        internal readonly HashSet<ushort> nodeChangeBuffer = new HashSet<ushort>();
        internal readonly HashSet<ushort> segmentChangeBuffer = new HashSet<ushort>();
        internal readonly HashSet<ushort> segmentReleaseBuffer = new HashSet<ushort>();
        internal readonly HashSet<ushort> segmentNameChangeBuffer = new HashSet<ushort>();

        internal readonly HashSet<ushort> m_lineStack = new HashSet<ushort>();
        internal readonly HashSet<ushort> m_buildingStack = new HashSet<ushort>();
        private uint linesCooldown = 0;
        public void Update()
        {
            if (segmentsCooldown > 0)
            {
                segmentsCooldown--;
            }
            else if (segmentsCooldown == 0)
            {
                segmentsCooldown--;
                foreach (var node in nodeChangeBuffer)
                {
                    StartCoroutine(EventNodeChanged?.Invoke(node));
                    WTSBuildingData.Instance.CacheData.PurgeStopCache(node);
                }
                foreach (var segment in segmentChangeBuffer)
                {
                    WTSOnNetData.Instance.OnSegmentChanged(segment);
                    StartCoroutine(EventSegmentChanged?.Invoke(segment));
                }
                foreach (var segment in segmentReleaseBuffer)
                {
                    StartCoroutine(EventSegmentReleased?.Invoke(segment));
                }
                foreach (var segment in segmentNameChangeBuffer)
                {
                    StartCoroutine(EventSegmentNameChanged?.Invoke(segment));
                }
                nodeChangeBuffer.Clear();
                segmentChangeBuffer.Clear();
            }


            if (linesCooldown == 1)
            {
                bool shouldDecrement = true;
                if (m_lineStack.Count > 0)
                {
                    var next = m_lineStack.First();
                    EventOnLineUpdated?.Invoke(next);
                    m_lineStack.Remove(next);
                    shouldDecrement = false;
                }
                if (m_buildingStack.Count > 0)
                {
                    var next = m_buildingStack.First();
                    EventOnLineBuildingUpdated?.Invoke(next);
                    m_buildingStack.Remove(next);
                    shouldDecrement = false;
                }
                if (shouldDecrement)
                {
                    linesCooldown = 0;
                }
            }
            else if (linesCooldown > 0)
            {
                linesCooldown--;
            }
        }

        internal void ResetSegmentCooldown()
        {
            segmentsCooldown = 15;
        }

        internal void ResetLinesCooldown()
        {
            linesCooldown = 15;
        }

        #endregion



        public static Material ___OUTSIDE_MAT { get => WETextRenderer.m_outsideMaterial; set => WETextRenderer.m_outsideMaterial = value; }
        public static Vector2[] __cachedUvFrame;
        public static Vector2[] __cachedUvGlass;
        public static Material m_rotorMaterial;
        public static Material m_outsideMaterial;
        public static float __constMultiplierVertex = 100;

        public static bool ___RELOADSH
        {
            get => false; set
            {
                if (value)
                {
                    WEAssetLibrary.instance.ReloadFromDisk();
                    ModInstance.Controller.defaultTextShader = WEAssetLibrary.instance.GetShaders().TryGetValue("klyte/wts/wtsshader", out var x) ? x : null;
                    ReloadFontsFromPath();
                    ModInstance.Controller.AtlasesLibrary.LoadImagesFromLocalFolders();
                    WETextRenderer.m_outsideMaterial = null;
                }
            }
        }
    }
}
