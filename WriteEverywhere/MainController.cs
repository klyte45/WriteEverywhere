extern alias ADR;
extern alias TLM;

using ColossalFramework.UI;
using Kwytto.Interfaces;
using Kwytto.Utils;
using SpriteFontPlus;
using System;
using System.IO;
using UnityEngine;
using WriteEverywhere.ModShared;
using WriteEverywhere.Sprites;
using WriteEverywhere.Tools;
using WriteEverywhere.UI;

namespace WriteEverywhere
{
    public class MainController : BaseController<ModInstance, MainController>
    {
        public static readonly string FOLDER_PATH = KFileUtils.BASE_FOLDER_PATH + "WriteEverywhere";

        #region Tool Access
        public RoadSegmentTool RoadSegmentToolInstance => ToolsModifierControl.toolController.GetComponent<RoadSegmentTool>();
        #endregion

        #region Shader
        public WTSShaderLibrary ShaderLib => WTSShaderLibrary.instance;
        public static Shader DEFAULT_SHADER_TEXT = WTSShaderLibrary.instance.GetShaders().TryGetValue("klyte/wts/wtsshader", out Shader value) ? value : value;
        #endregion

        #region Fonts
        public const string DEFAULT_FONT_KEY = "/DEFAULT/";
        public const string FONTS_FILES_FOLDER = "Fonts";
        public static int DefaultTextureSizeFont => 512 << ModInstance.StartTextureSizeFont;
        public static string FontFilesPath { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + FONTS_FILES_FOLDER;
        public event Action EventFontsReloadedFromFolder;
        public static void ReloadFontsFromPath()
        {
            FontServer.instance.ResetCollection();
            FontServer.instance.RegisterFont(DEFAULT_FONT_KEY, KResourceLoader.LoadResourceDataMod("UI.DefaultFont.SourceSansPro-Regular.ttf"), DefaultTextureSizeFont);

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
        public ADR::Bridge_WE2ADR.IBridge ConnectorADR { get; } = new BridgeADRFallback();
        public TLM::Bridge_WE2TLM.IBridge ConnectorTLM { get; } = new BridgeTLMFallback();
        #endregion



        public void Awake()
        {
            ToolsModifierControl.toolController.AddExtraToolToController<SegmentEditorPickerTool>();
            ToolsModifierControl.toolController.AddExtraToolToController<RoadSegmentTool>();
        }

        protected override void StartActions()
        {
            base.StartActions();

            var uiGO = new GameObject("WE");
            uiGO.transform.SetParent(UIView.GetAView().gameObject.transform);
            uiGO.AddComponent<WTSOnNetLiteUI>();
        }
    }
}
