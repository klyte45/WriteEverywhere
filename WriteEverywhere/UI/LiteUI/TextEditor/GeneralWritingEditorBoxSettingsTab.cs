using Kwytto.LiteUI;
using Kwytto.UI;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal class GeneralWritingEditorBoxSettingsTab : IGUITab<BoardTextDescriptorGeneralXml>
    {

        private readonly GUIColorPicker m_picker;
        private Vector2 m_scrollPos;

        public GeneralWritingEditorBoxSettingsTab(GUIColorPicker picker) => m_picker = picker;

        public Texture TabIcon { get; } = GUIKwyttoCommons.GetByNameFromDefaultAtlas("ToolbarIconProps");

        public bool DrawArea(Vector2 tabAreaSize, ref BoardTextDescriptorGeneralXml currentItem, int currentItemIdx)
        {
            GUILayout.Label($"<i>{Str.WTS_BACKGROUNDANDBOX_SETTINGS}</i>");
            var item = currentItem;
            bool isEditable = true;
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

                        GUIKwyttoCommons.AddColorPicker(Str.WTS_BG_COLOR, m_picker, ref item.BackgroundMeshSettings.m_cachedColor, isEditable);
                        changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.BackgroundMeshSettings.Size, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, Str.WTS_TEXTBACKGROUNDSIZEGENERATED, isEditable, .001f);
                        GUIKwyttoCommons.AddSlider(tabAreaSize.x, Str.we_roadEditor_verticalAlignmentBoxText, ref item.BackgroundMeshSettings.m_verticalAlignment, 0, 1, isEditable);

                        GUILayout.Space(10);
                        GUIKwyttoCommons.AddToggle(Str.WTS_TEXT_USEFRAME, ref item.BackgroundMeshSettings.m_useFrame, isEditable);
                        var usingFrame = item.BackgroundMeshSettings.m_useFrame;
                        if (usingFrame)
                        {
                            changedFrame |= GUIKwyttoCommons.AddVector2Field(tabAreaSize.x, item.BackgroundMeshSettings.FrameMeshSettings.BackSize, Str.WTS_BOXMESH_BACKSIZE, Str.WTS_BOXMESH_BACKSIZE, isEditable);
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
                                GUIKwyttoCommons.AddColorPicker(Str.WTS_BOXMESH_OUTERCOLOR, m_picker, ref item.BackgroundMeshSettings.FrameMeshSettings.m_cachedOutsideColor, isEditable);
                            }
                            GUIKwyttoCommons.AddColorPicker(Str.WTS_TEXT_CONTAINERGLASSCOLOR, m_picker, ref item.BackgroundMeshSettings.FrameMeshSettings.m_cachedGlassColor, isEditable);

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

                }
                m_scrollPos = scroll.scrollPosition;
            }
            return false;
        }
        public void Reset() { }
    }
}
