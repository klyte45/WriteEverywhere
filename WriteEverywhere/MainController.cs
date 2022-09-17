extern alias TLM;
extern alias ADR;

using Kwytto.Interfaces;
using Kwytto.Utils;
using SpriteFontPlus;
using System;
using System.IO;
using UnityEngine;
using WriteEverywhere.ModShared;
using WriteEverywhere.Sprites;
using WriteEverywhere.Tools;

namespace WriteEverywhere
{
    public class MainController : BaseController<ModInstance, MainController>
    {
        public static readonly string FOLDER_PATH = KFileUtils.BASE_FOLDER_PATH + "WriteEverywhere";
        public RoadSegmentTool RoadSegmentToolInstance => ToolsModifierControl.toolController.GetComponent<RoadSegmentTool>();
        public WTSShaderLibrary ShaderLib => WTSShaderLibrary.instance;

        public const string FONTS_FILES_FOLDER = "Fonts";
        public const string EXTRA_SPRITES_FILES_FOLDER = "Sprites";

        public static int DefaultTextureSizeFont => 512 << ModInstance.StartTextureSizeFont;
        public static string ExtraSpritesFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + EXTRA_SPRITES_FILES_FOLDER;
        public static string FontFilesPath { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + FONTS_FILES_FOLDER;

        internal WTSAtlasesLibrary AtlasesLibrary { get; private set; }
        public const string EXTRA_SPRITES_FILES_FOLDER_ASSETS = "K45WTS_Sprites";
        public const string m_defaultFileNamePropsXml = "WTS_DefaultPropsConfig";
        public const string DEFAULT_GAME_PROP_LAYOUT_FOLDER = "PropsDefaultLayouts";

        public static string DefaultPropsLayoutConfigurationFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + DEFAULT_GAME_PROP_LAYOUT_FOLDER;
        public ADR::Bridge_WE2ADR.IBridge ConnectorADR { get; } = new BridgeADRFallback();
        public TLM::Bridge_WE2TLM.IBridge ConnectorTLM { get; } = new BridgeTLMFallback();

        public static Shader DEFAULT_SHADER_TEXT = WTSShaderLibrary.instance.GetShaders()["Klyte/WTS/WTSShader"];

        public const string DEFAULT_FONT_KEY = "/DEFAULT/";
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
    }
}
