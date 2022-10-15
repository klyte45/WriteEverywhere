using Kwytto.LiteUI;
using System;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorFrameSettingsTab : WTSBaseParamsTab<TextToWriteOnXml>
    {

        private readonly GUIColorPicker m_picker;
        private Vector2 m_scrollPos;
        private readonly Func<PrefabInfo> m_infoGetter;

        public GeneralWritingEditorFrameSettingsTab(GUIColorPicker picker, Func<PrefabInfo> infoGetter)
        {
            m_picker = picker;
            m_infoGetter = infoGetter;
        }

        public override Texture TabIcon { get; } = GUIKwyttoCommons.GetByNameFromDefaultAtlas("ToolbarIconProps");

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
                        bool changedFrame = false;
                        GUIKwyttoCommons.AddToggle(Str.WTS_TEXT_USEFRAME, ref item.BackgroundMeshSettings.m_useFrame, isEditable);
                        var usingFrame = item.BackgroundMeshSettings.m_useFrame;
                        if (usingFrame)
                        {
                            changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.BackgroundMeshSettings.FrameMeshSettings.BackSize, Str.WTS_BOXMESH_BACKSIZE, Str.WTS_BOXMESH_BACKSIZE, isEditable, .001f);
                            changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.BackgroundMeshSettings.FrameMeshSettings.BackOffset, Str.WTS_BOXMESH_BACKOFFSETFROMCENTERBOTTOM, Str.WTS_BOXMESH_BACKOFFSETFROMCENTERBOTTOM, isEditable);
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_BOXMESH_DEPTH_BACK, item.BackgroundMeshSettings.FrameMeshSettings.BackDepth, out var newVal, isEditable, .001f))
                            {
                                item.BackgroundMeshSettings.FrameMeshSettings.BackDepth = newVal;
                                changedFrame |= true;
                            }
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_BOXMESH_DEPTH_FRONT, item.BackgroundMeshSettings.FrameMeshSettings.FrontDepth, out newVal, isEditable, .001f))
                            {
                                item.BackgroundMeshSettings.FrameMeshSettings.FrontDepth = newVal;
                                changedFrame |= true;
                            }
                            if (GUIKwyttoCommons.AddFloatField(tabAreaSize.x, Str.WTS_TEXT_CONTAINERFRONTBORDERTHICKNESS, item.BackgroundMeshSettings.FrameMeshSettings.FrontBorderThickness, out newVal, isEditable, 0, Mathf.Min(item.BackgroundMeshSettings.Size.X, item.BackgroundMeshSettings.Size.Y) / 2))
                            {
                                item.BackgroundMeshSettings.FrameMeshSettings.FrontBorderThickness = newVal;
                                changedFrame |= true;
                            }

                            GUILayout.Space(10);
                            GUILayout.Label($"<color=#FFFF00>{Str.WTS_BOXMESH_COLORSGROUP_LABEL}</color>");
                            GUIKwyttoCommons.AddToggle(Str.we_generalTextEditor_usePrefabColor, ref item.BackgroundMeshSettings.FrameMeshSettings.m_inheritColor, isEditable);
                            if (!item.BackgroundMeshSettings.FrameMeshSettings.m_inheritColor)
                            {
                                GUIKwyttoCommons.AddColorPicker(Str.WTS_BOXMESH_OUTERCOLOR, m_picker, item.BackgroundMeshSettings.FrameMeshSettings.m_cachedOutsideColor, (x) => item.BackgroundMeshSettings.FrameMeshSettings.m_cachedOutsideColor = x.Value, isEditable);
                            }
                            changedFrame |= GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_CONTAINERGLASSCOLOR, m_picker, item.BackgroundMeshSettings.FrameMeshSettings.m_cachedGlassColor, (x) => item.BackgroundMeshSettings.FrameMeshSettings.m_cachedGlassColor = x.Value, isEditable);

                            GUILayout.Space(10);
                            GUILayout.Label($"<color=#FFFF00>{Str.WTS_BOXMESH_EFFECTSGROUP_LABEL}</color>");
                            if (GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_CONTAINERGLASSSPECULARITY, item.BackgroundMeshSettings.FrameMeshSettings.GlassSpecularLevel, out newVal, 0, 1, isEditable))
                            {
                                item.BackgroundMeshSettings.FrameMeshSettings.GlassSpecularLevel = newVal;
                            }
                            if (GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.WTS_TEXT_CONTAINERGLASSTRANSPARENCY, item.BackgroundMeshSettings.FrameMeshSettings.GlassTransparency, out newVal, 0, 1, isEditable))
                            {
                                item.BackgroundMeshSettings.FrameMeshSettings.GlassTransparency = newVal;
                            }
                        }
                        if (changedFrame)
                        {
                            item.BackgroundMeshSettings.FrameMeshSettings.ClearCacheArray();
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
    }
}