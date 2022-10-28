using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorBgMeshSettingsTab : WTSBaseParamsTab<TextToWriteOnXml>
    {

        protected override TextRenderingClass RenderingClass => TextRenderingClass.BgMesh;
        private readonly GUIColorPicker m_picker;
        private readonly GUIRootWindowBase m_root;
        private Vector2 m_scrollPos;
        private readonly Func<PrefabInfo> m_infoGetter;
        private readonly string[] contrastOptions;
        private readonly ColoringSource[] contrastValues;
        private readonly TextRenderingClass srcClass;

        public GeneralWritingEditorBgMeshSettingsTab(GUIColorPicker picker, Func<PrefabInfo> infoGetter, TextRenderingClass srcClass) : base()
        {
            m_picker = picker;
            m_infoGetter = infoGetter;
            m_root = picker.GetComponentInParent<GUIRootWindowBase>();
            contrastValues = ColoringSourceExtensions.AvailableAtClass(srcClass);
            contrastOptions = contrastValues.Select(x => x.ValueToI18n()).ToArray();
            this.srcClass = srcClass;
        }

        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_PaintBucket);

        protected override void DrawListing(Vector2 tabAreaSize, TextToWriteOnXml item, bool isEditable)
        {
            GUILayout.Label($"<i>{Str.WTS_BACKGROUNDANDBOX_SETTINGS}</i>");
            using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
            {
                using (new GUILayout.VerticalScope())
                {
                    var bgSettings = item.BackgroundMeshSettings;
                    bool changedFrame = false;
                    var hasBg = ((Vector2)bgSettings.Size).sqrMagnitude >= 0.0000001f;
                    if (GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useBackground, hasBg, out var boolVal))
                    {
                        bgSettings.Size = boolVal ? new Kwytto.Utils.Vector2Xml { X = .001f, Y = .001f } : new Kwytto.Utils.Vector2Xml();
                    }
                    if (hasBg)
                    {
                        GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_colorSource, ref bgSettings.m_colorSource, contrastOptions, contrastValues, m_root, isEditable);
                        bool isPlatformRelative = srcClass != TextRenderingClass.Vehicle && (bgSettings.m_colorSource == ColoringSource.PlatformLine || bgSettings.m_colorSource == ColoringSource.ContrastPlatformLine);
                        GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useFixedIfMultiline, ref bgSettings.m_useFixedIfMultiline, isEditable, isPlatformRelative);
                        if (bgSettings.m_colorSource == ColoringSource.Fixed || (isPlatformRelative && bgSettings.m_useFixedIfMultiline))
                        {
                            GUIKwyttoCommons.AddColorPicker(isPlatformRelative ? Str.we_generalTextEditor_bgFrontColorForMultilineIfActive : Str.we_generalTextEditor_bgFrontColor, m_picker, bgSettings.m_bgFrontColor, (x) => bgSettings.m_bgFrontColor = x.Value, isEditable);
                        }
                        else
                        {
                            GUILayout.Space(12);
                        }
                        GUIKwyttoCommons.AddColorPicker(Str.we_generalTextEditor_backgroundBackfaceColor, m_picker, bgSettings.m_cachedBackColor, (x) => bgSettings.m_cachedBackColor = x.Value, isEditable);
                        changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, bgSettings.Size, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, isEditable, .001f);
                        var param = bgSettings.BgImage;
                        GUIKwyttoCommons.AddButtonSelector(tabAreaSize.x, Str.we_generalTextEditor_backgroundImage, param is null ? GUIKwyttoCommons.v_null : param.IsEmpty ? GUIKwyttoCommons.v_empty : param.ToString(), () => GoToPicker(-1, TextContent.ParameterizedSpriteSingle, param, item), isEditable);

                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_generalTextEditor_depthNormalBg, ref bgSettings.m_normalStrength, -1000, 1000, isEditable);
                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_horizontalAlignmentBoxText, ref bgSettings.m_horizontalAlignment, 0, 1, isEditable);
                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_verticalAlignmentBoxText, ref bgSettings.m_verticalAlignment, 0, 1, isEditable);
                        if (changedFrame)
                        {
                            bgSettings.FrameMeshSettings.ClearCacheArray();
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

        protected override PrefabInfo GetCurrentInfo() => m_infoGetter();
    }
}
