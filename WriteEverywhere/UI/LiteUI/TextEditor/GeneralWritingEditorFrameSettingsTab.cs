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
    internal class GeneralWritingEditorFrameSettingsTab : WTSBaseParamsTab<TextToWriteOnXml>
    {

        private readonly GUIColorPicker m_picker;
        private Vector2 m_scrollPos;
        private readonly Func<PrefabInfo> m_infoGetter;
        private readonly string[] contrastOptions;
        private readonly ColoringSource[] contrastValues;
        private readonly GUIRootWindowBase m_root;

        protected override TextRenderingClass RenderingClass { get; }
        public GeneralWritingEditorFrameSettingsTab(GUIColorPicker picker, Func<PrefabInfo> infoGetter, TextRenderingClass renderingClass) : base()
        {
            m_picker = picker;
            m_root = picker.GetComponentInParent<GUIRootWindowBase>();
            m_infoGetter = infoGetter;
            RenderingClass = renderingClass;
            contrastValues = ColoringSourceExtensions.AvailableAtClass(renderingClass);
            contrastOptions = contrastValues.Select(x => x.ValueToI18n()).ToArray();
        }

        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(Kwytto.UI.CommonsSpriteNames.K45_Wireframe);

        protected override void DrawListing(Vector2 tabAreaSize, TextToWriteOnXml item, bool isEditable)
        {
            var hasBg = ((Vector2)item.BackgroundMeshSettings.Size).sqrMagnitude >= 0.0000001f;
            GUILayout.Label($"<i>{Str.we_generalTextEditor_frameSettings}</i>");
            if (hasBg)
            {
                using (var scroll = new GUILayout.ScrollViewScope(m_scrollPos))
                {
                    using (new GUILayout.VerticalScope())
                    {
                        GUIKwyttoCommons.AddToggle(Str.WTS_TEXT_USEFRAME, ref item.BackgroundMeshSettings.m_useFrame, isEditable);
                        var usingFrame = item.BackgroundMeshSettings.m_useFrame;
                        if (usingFrame)
                        {
                            bool changedFrame = false;
                            var meshSettings = item.BackgroundMeshSettings.FrameMeshSettings;

                            changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, meshSettings.BackSize, Str.WTS_BOXMESH_BACKSIZE, Str.WTS_BOXMESH_BACKSIZE, isEditable, .001f);
                            changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, meshSettings.BackOffset, Str.WTS_BOXMESH_BACKOFFSETFROMCENTERBOTTOM, Str.WTS_BOXMESH_BACKOFFSETFROMCENTERBOTTOM, isEditable);
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_BOXMESH_DEPTH_BACK, meshSettings.BackDepth, out var newVal, isEditable, .001f))
                            {
                                meshSettings.BackDepth = newVal;
                                changedFrame |= true;
                            }
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_BOXMESH_DEPTH_FRONT, meshSettings.FrontDepth, out newVal, isEditable, .001f))
                            {
                                meshSettings.FrontDepth = newVal;
                                changedFrame |= true;
                            }
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_TEXT_CONTAINERFRONTBORDERTHICKNESS, meshSettings.FrontBorderThickness, out newVal, isEditable, 0, Mathf.Min(item.BackgroundMeshSettings.Size.X, item.BackgroundMeshSettings.Size.Y) / 2))
                            {
                                meshSettings.FrontBorderThickness = newVal;
                                changedFrame |= true;
                            }

                            GUILayout.Space(10);
                            GUILayout.Label($"<color=#FFFF00>{Str.WTS_BOXMESH_COLORSGROUP_LABEL}</color>");
                            changedFrame |= GUIKwyttoCommons.AddComboBox(tabAreaSize.x, Str.we_generalTextEditor_colorSource, ref meshSettings.m_colorSource, contrastOptions, contrastValues, m_root, isEditable);
                            bool isPlatformRelative = RenderingClass != TextRenderingClass.Vehicle && (meshSettings.m_colorSource == ColoringSource.PlatformLine || meshSettings.m_colorSource == ColoringSource.ContrastPlatformLine);
                            changedFrame |= GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_useFixedIfMultiline, ref meshSettings.m_useFixedIfMultiline, isEditable, isPlatformRelative);
                            if (meshSettings.m_colorSource == ColoringSource.Fixed || isPlatformRelative)
                            {
                                changedFrame |= GUIKwyttoCommons.AddColorPicker(Str.WTS_BOXMESH_OUTERCOLOR, m_picker, meshSettings.m_cachedOutsideColor, (x) => meshSettings.m_cachedOutsideColor = x.Value, isEditable);
                            }
                            else
                            {
                                GUILayout.Space(12);
                            }
                            changedFrame |= GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_CONTAINERGLASSCOLOR, m_picker, meshSettings.m_cachedGlassColor, (x) => meshSettings.m_cachedGlassColor = x.Value, isEditable);

                            GUILayout.Space(10);
                            GUILayout.Label($"<color=#FFFF00>{Str.WTS_BOXMESH_EFFECTSGROUP_LABEL}</color>");
                            if (GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_CONTAINERGLASSSPECULARITY, meshSettings.GlassSpecularLevel, out newVal, 0, 1, isEditable))
                            {
                                meshSettings.GlassSpecularLevel = newVal;
                            }
                            if (GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_CONTAINERGLASSTRANSPARENCY, meshSettings.GlassTransparency, out newVal, 0, 1, isEditable))
                            {
                                meshSettings.GlassTransparency = newVal;
                            }
                            if (changedFrame)
                            {
                                meshSettings.ClearCacheArray();
                            }
                        }
                    }
                    m_scrollPos = scroll.scrollPosition;
                }
            }
            else
            {
                GUILayout.Label(Str.we_generalTextEditor_bgRequiredForFrame);
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