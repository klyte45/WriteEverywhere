using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    public class WESettingsFoldersGeneralTab : IGUIVerticalITab
    {
        public string TabDisplayName => Str.we_settings_folders;
        private Vector2 m_scrollPosition;
        public void DrawArea(Vector2 tabAreaSize)
        {
            using (new GUILayout.AreaScope(new Rect(default, tabAreaSize)))
            {
                using (var scroll = new GUILayout.ScrollViewScope(m_scrollPosition))
                {
                    using (new GUILayout.VerticalScope())
                    {
                        using(new GUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button(Str.WTS_REFRESH_IMAGES_FOLDER))
                            {
                                ModInstance.Controller?.AtlasesLibrary.LoadImagesFromLocalFolders();
                            }
                            if (GUILayout.Button(Str.we_settings_reloadFonts) && FontServer.exists)
                            {
                                MainController.ReloadFontsFromPath();
                            }
                        }
                        GUILayout.Space(12);
                        DoButtonToFolderDraw(Str.WTS_DEFAULT_BUILDINGS_CONFIG_PATH_TITLE, MainController.DefaultBuildingsConfigurationFolder);
                        DoButtonToFolderDraw(Str.WTS_DEFAULT_VEHICLES_CONFIG_PATH_TITLE, MainController.DefaultVehiclesConfigurationFolder);
                        DoButtonToFolderDraw(Str.WTS_DEFAULT_PROP_LAYOUTS_PATH_TITLE, MainController.DefaultPropsLayoutConfigurationFolder);
                        DoButtonToFolderDraw(Str.WTS_DEFAULT_HWSHIELDS_CONFIG_PATH_TITLE, MainController.DefaultHwShieldsConfigurationFolder);
                        DoButtonToFolderDraw(Str.WTS_FONT_FILES_PATH_TITLE, MainController.FontFilesPath);
                        DoButtonToFolderDraw(Str.WTS_EXTRA_SPRITES_PATH_TITLE, MainController.ExtraSpritesFolder);
                       
                    }
                    m_scrollPosition = scroll.scrollPosition;
                }
            }
        }

        private void DoButtonToFolderDraw(string label, string destination)
        {
            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(label);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(Str.we_settings_goToFolderShort))
                    {
                        var fileInfo = KFileUtils.EnsureFolderCreation(destination);
                        ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName);
                    }
                }
                GUILayout.Label($"<Color=#ffff00>{destination}</color>");
                GUILayout.Space(6);
            }
        }

        public void Reset()
        {
            m_scrollPosition = default;
        }
    }
}
