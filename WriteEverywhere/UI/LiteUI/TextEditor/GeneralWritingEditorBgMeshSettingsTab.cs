using Kwytto.LiteUI;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorBgMeshSettingsTab : WTSBaseParamsTab<TextToWriteOnXml>
    {

        private readonly GUIColorPicker m_picker;
        private readonly GUIRootWindowBase m_root;
        private Vector2 m_scrollPos;
        private readonly Func<PrefabInfo> m_infoGetter;
        private static readonly string[] contrastOptions = EnumI18nExtensions.GetAllValuesI18n<ColoringSource>();
        private static readonly ColoringSource[] contrastValues = Enum.GetValues(typeof(ColoringSource)).Cast<ColoringSource>().ToArray();

        public GeneralWritingEditorBgMeshSettingsTab(GUIColorPicker picker, Func<PrefabInfo> infoGetter)
        {
            m_picker = picker;
            m_infoGetter = infoGetter;
            m_root = picker.GetComponentInParent<GUIRootWindowBase>();
        }

        public override Texture TabIcon { get; } = GUIKwyttoCommons.GetByNameFromDefaultAtlas("ZoningOptionFill");

        protected override void DrawListing(Vector2 tabAreaSize, TextToWriteOnXml item, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_BACKGROUNDANDBOX_SETTINGS}</i>");
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                using (new GUILayout.VerticalScope())
                {
                    bool changedFrame = false;
                    var hasBg = ((Vector2)item.BackgroundMeshSettings.Size).sqrMagnitude >= 0.0000001f;
                    if (GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useBackground, hasBg, out var boolVal))
                    {
                        item.BackgroundMeshSettings.Size = boolVal ? new Kwytto.Utils.Vector2Xml { X = .001f, Y = .001f } : new Kwytto.Utils.Vector2Xml();
                    }
                    if (hasBg)
                    {
                        GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_colorSource, ref item.BackgroundMeshSettings.m_colorSource, contrastOptions, contrastValues, m_root, isEditable);
                        bool isPlatformRelative = item.BackgroundMeshSettings.m_colorSource == ColoringSource.PlatformLine || item.BackgroundMeshSettings.m_colorSource == ColoringSource.ContrastPlatformLine;
                        GUIKwyttoCommons.AddColorPicker(isPlatformRelative ? Str.we_generalTextEditor_bgFrontColorForMultilineIfActive : Str.we_generalTextEditor_bgFrontColor, m_picker, item.BackgroundMeshSettings.m_bgFrontColor, (x) => item.BackgroundMeshSettings.m_bgFrontColor = x.Value, isEditable);
                        GUIKwyttoCommons.AddColorPicker(Str.we_generalTextEditor_backgroundBackfaceColor, m_picker, item.BackgroundMeshSettings.m_cachedBackColor, (x) => item.BackgroundMeshSettings.m_cachedBackColor = x.Value, isEditable);
                        changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.BackgroundMeshSettings.Size, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, isEditable, .001f);
                        var param = item.BackgroundMeshSettings.BgImage;
                        GUIKwyttoCommons.AddButtonSelector(tabAreaSize.x, Str.we_generalTextEditor_backgroundImage, param is null ? GUIKwyttoCommons.v_null : param.IsEmpty ? GUIKwyttoCommons.v_empty : param.ToString(), () => GoToPicker(-1, TextContent.ParameterizedSpriteSingle, param, item), isEditable);

                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_generalTextEditor_depthNormalBg, ref item.BackgroundMeshSettings.m_normalStrength, -1000, 1000, isEditable);
                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_horizontalAlignmentBoxText, ref item.BackgroundMeshSettings.m_horizontalAlignment, 0, 1, isEditable);
                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_verticalAlignmentBoxText, ref item.BackgroundMeshSettings.m_verticalAlignment, 0, 1, isEditable);
                        if (changedFrame)
                        {
                            item.BackgroundMeshSettings.FrameMeshSettings.ClearCacheArray();
                        }
                    }

                }
                m_scrollPos = scroll.scrollPosition;
            }
        }

        protected override string GetAssetName(TextToWriteOnXml item) => m_infoGetter()?.name;
        protected override void SetTextParameter(TextToWriteOnXml item, int currentEditingParam, string paramValue)
        {
            item.BackgroundMeshSettings.SetBgImage(paramValue);
        }
    }
}
