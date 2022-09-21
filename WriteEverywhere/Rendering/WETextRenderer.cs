using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Rendering
{
    public static class WETextRenderer
    {
        public static readonly int SHADER_PROP_COLOR = Shader.PropertyToID("_Color");
        public static readonly int SHADER_PROP_SURF_PROPERTIES = Shader.PropertyToID("_SurfProperties");
        public static readonly int SHADER_PROP_BACK_COLOR = Shader.PropertyToID("_BackfaceColor");
        public static readonly int SHADER_PROP_BACK_MIRRORED = Shader.PropertyToID("_MirrorBack");
        public static readonly int SHADER_PROP_BORDERS = Shader.PropertyToID("_Border");
        public static readonly int SHADER_PROP_PIXELS_METERS = Shader.PropertyToID("_PixelsPerMeters");
        public static readonly int SHADER_PROP_DIMENSIONS = Shader.PropertyToID("_Dimensions");
        private static readonly float m_daynightOffTime = 6 * Convert.ToSingle(Math.Pow(Convert.ToDouble((6 - (15 / 2.5)) / 6), Convert.ToDouble(1 / 1.09)));
        public static Vector3 RenderTextMesh(ushort refID, int boardIdx, int secIdx, ref Color parentColor, BaseWriteOnXml baseWrite, BoardTextDescriptorGeneralXml textDescriptor, ref Matrix4x4 propMatrix, PrefabInfo srcInfo, int instanceFlags, int instanceFlags2, bool currentEditingSizeLine, ref int defaultCallsCounter)
        {
            BasicRenderInformation bri = WTSTextMeshProcess.GetTextMesh(textDescriptor, refID, boardIdx, secIdx, baseWrite);
            if (bri?.m_mesh is null || bri?.m_generatedMaterial is null)
            {
                return default;
            }
            var textColor = GetTextColor(refID, boardIdx, secIdx, baseWrite, textDescriptor);
            Vector3 scl = baseWrite.PropScale;
            return DrawTextBri(refID, boardIdx, secIdx, ref propMatrix, textDescriptor, bri, ref textColor, textDescriptor.PlacingConfig, ref parentColor, srcInfo, ref scl, textDescriptor.m_textAlign, textDescriptor.MaxWidthMeters, instanceFlags, instanceFlags2, currentEditingSizeLine, ref defaultCallsCounter);

        }

        private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private static Vector3 DrawTextBri(ushort refID, int boardIdx, int secIdx, ref Matrix4x4 propMatrix, BoardTextDescriptorGeneralXml textDescriptor,
        BasicRenderInformation renderInfo, ref Color colorToSet, PlacingSettings placingSettings, ref Color parentColor, PrefabInfo srcInfo,
       ref Vector3 baseScale, UIHorizontalAlignment targetTextAlignment, float maxWidth, int instanceFlags, int instanceFlags2, bool currentEditingSizeLine, ref int defaultCallsCounter)
        {

            var textMatrixes = CalculateTextMatrix(placingSettings, ref baseScale, targetTextAlignment, maxWidth, textDescriptor, renderInfo);
            var positionAccumulator = new Vector3();
            foreach (var textItem in textMatrixes)
            {
                Matrix4x4 matrix = propMatrix * textItem.baseMatrix;

                block.Clear();

                Material targetMaterial = renderInfo.m_generatedMaterial;
                CalculateIllumination(refID, boardIdx, secIdx, textDescriptor, block, ref colorToSet, instanceFlags, instanceFlags2);

                defaultCallsCounter++;
                Graphics.DrawMesh(renderInfo.m_mesh, matrix, targetMaterial, 10, null, 0, block, false);

                positionAccumulator += (Vector3)matrix.GetColumn(3) + new Vector3(0, renderInfo.m_mesh.bounds.center.y * matrix.GetColumn(1).y);
                if (currentEditingSizeLine)
                {
                    BasicRenderInformation bgBri = ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, CommonsSpriteNames.K45_SquareIcon.ToString());
                    DrawBgMesh(ref propMatrix, textDescriptor.LineMaxDimensions, Color.magenta, .5f, block, placingSettings, ref baseScale, targetTextAlignment, textItem, bgBri, textDescriptor.TextLineHeight, ref defaultCallsCounter, 1);
                }
                if (((Vector2)textDescriptor.BackgroundMeshSettings.Size).sqrMagnitude != 0)
                {
                    BasicRenderInformation bgBri = ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, CommonsSpriteNames.K45_SquareIcon.ToString());
                    if (bgBri != null)
                    {
                        Matrix4x4 containerMatrix = DrawBgMesh(ref propMatrix, textDescriptor.BackgroundMeshSettings.Size, textDescriptor.BackgroundMeshSettings.BackgroundColor, textDescriptor.BackgroundMeshSettings.m_verticalAlignment, block, textItem.placingSettings, ref baseScale, targetTextAlignment, textItem, bgBri, textDescriptor.TextLineHeight, ref defaultCallsCounter, currentEditingSizeLine ? 2 : 1);
                        if (textDescriptor.BackgroundMeshSettings.m_useFrame)
                        {
                            DrawTextFrame(textDescriptor, block, placingSettings, ref baseScale, ref parentColor, srcInfo, ref containerMatrix, ref defaultCallsCounter);
                        }
                    }
                }

            }
            return positionAccumulator / textMatrixes.Count;
        }

        private static Matrix4x4 DrawBgMesh(ref Matrix4x4 propMatrix, Vector2 size, Color color, float verticalAlignment, MaterialPropertyBlock materialPropertyBlock, PlacingSettings placingSettings, ref Vector3 baseScale, UIHorizontalAlignment targetTextAlignment, TextRenderDescriptor textMatrixTuple,
             BasicRenderInformation bgBri, float lineHeight, ref int defaultCallsCounter, float zDistanceMultiplier)
        {
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, color * new Color(1, 1, 1, 0));
            materialPropertyBlock.SetVector(SHADER_PROP_SURF_PROPERTIES, new Vector4());
            ApplyTextAdjustments(placingSettings, bgBri, ref baseScale, size.y, 0, targetTextAlignment, size.x, false, false, false);

            var containerMatrix = propMatrix
                * Matrix4x4.Translate(placingSettings.Position)
                * Matrix4x4.Translate(new Vector3(0, (size.y * (-verticalAlignment + .5f)) + (verticalAlignment * lineHeight)))
                * textMatrixTuple.rotationMatrix
                * textMatrixTuple.scaleMatrix
                * Matrix4x4.Translate(new Vector3(0, 0, -0.001f * zDistanceMultiplier))
                ;
            var bgMatrix = containerMatrix * Matrix4x4.Scale(size);
            defaultCallsCounter++;
            Graphics.DrawMesh(bgBri.m_mesh, bgMatrix, bgBri.m_generatedMaterial, 10, null, 0, materialPropertyBlock);
            return containerMatrix;
        }

        private static Vector2[] cachedUvFrame;
        private static Vector2[] cachedUvGlass;
        private static Color32[] cachedColorGlass;
        internal static Material m_rotorMaterial;
        private static Material m_outsideMaterial;

        private static void DrawTextFrame(BoardTextDescriptorGeneralXml textDescriptor, MaterialPropertyBlock materialPropertyBlock, PlacingSettings placingSettings, ref Vector3 baseScale, ref Color parentColor, PrefabInfo srcInfo, ref Matrix4x4 containerMatrix, ref int defaultCallsCounter)
        {
            var frameConfig = textDescriptor.BackgroundMeshSettings.FrameMeshSettings;

            if (cachedUvFrame is null)
            {
                WTSDisplayContainerMeshUtils.GenerateDisplayContainer(new Vector2(1, 1), new Vector2(1, 1), new Vector2(), 0.05f, 0.3f, 0.1f, out Vector3[] points);
                cachedUvFrame = points.Select((x, i) => new Vector2(i * .25f % 1, i * .5f % 1)).ToArray();
                cachedUvGlass = points.Take(4).Select(x => new Vector2(0.5f, .5f)).ToArray();
                cachedColorGlass = points.Take(4).Select(x => new Color32(165, 0, 0, 0)).ToArray();
            }
            var instance2 = Singleton<VehicleManager>.instance;
            if (m_rotorMaterial == null)
            {
                m_rotorMaterial = new Material(Shader.Find("Custom/Vehicles/Vehicle/Rotors"))
                {
                    mainTexture = Texture2D.whiteTexture
                };
                var targetTexture = new Texture2D(1, 1);
                targetTexture.SetPixels(targetTexture.GetPixels().Select(x => new Color(.5f, .5f, 0.7f, 1)).ToArray());
                targetTexture.Apply();
                m_rotorMaterial.SetTexture("_XYSMap", targetTexture);
                m_rotorMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive | MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            if (m_outsideMaterial == null)
            {
                m_outsideMaterial = new Material(FontServer.instance.DefaultShader);
            }

            if (frameConfig.cachedFrameArray == null)
            {
                WTSDisplayContainerMeshUtils.GenerateDisplayContainer(textDescriptor.BackgroundMeshSettings.Size,
                    frameConfig.BackSize,
                    frameConfig.BackOffset,
                    frameConfig.FrontDepth,
                    frameConfig.BackDepth,
                    frameConfig.FrontBorderThickness,
                    out frameConfig.cachedFrameArray);

                frameConfig.meshOuterContainer = new Mesh()
                {
                    vertices = frameConfig.cachedFrameArray,
                    triangles = WTSDisplayContainerMeshUtils.m_triangles,
                    uv = cachedUvFrame,
                };
                frameConfig.meshGlass = new Mesh()
                {
                    vertices = frameConfig.cachedFrameArray.Take(4).ToArray(),
                    triangles = WTSDisplayContainerMeshUtils.m_trianglesGlass,
                    uv = cachedUvGlass,
                    colors32 = cachedColorGlass
                };
                foreach (var k in new Mesh[] { frameConfig.meshOuterContainer, frameConfig.meshGlass })
                {
                    WTSUtils.SolveTangents(k);
                }

                if (frameConfig.cachedGlassMain is null)
                {
                    frameConfig.cachedGlassMain = new Texture2D(1, 1);
                }

                if (frameConfig.cachedGlassXYS is null)
                {
                    frameConfig.cachedGlassXYS = new Texture2D(1, 1);
                }

                frameConfig.cachedGlassMain.SetPixels(new Color[] { frameConfig.GlassColor });
                frameConfig.cachedGlassXYS.SetPixels(new Color[] { new Color(.5f, .5f, 1 - frameConfig.GlassSpecularLevel, 1) });
                frameConfig.cachedGlassMain.Apply();
                frameConfig.cachedGlassXYS.Apply();
            }
            Matrix4x4 value;
            if (srcInfo is VehicleInfo vi)
            {
                var idt = Matrix4x4.identity;
                var qtr = Quaternion.Euler(placingSettings.Position);
                var pos = (Vector3)placingSettings.Position;
                value = vi.m_vehicleAI.CalculateTyreMatrix(Vehicle.Flags.Created | Vehicle.Flags.Spawned | Vehicle.Flags.TransferToTarget, ref pos, ref qtr, ref baseScale, ref idt);
            }

            materialPropertyBlock.Clear();
            materialPropertyBlock.SetTexture(instance2.ID_XYSMap, frameConfig.cachedGlassXYS);
            materialPropertyBlock.SetTexture(instance2.ID_MainTex, frameConfig.cachedGlassMain);
            defaultCallsCounter++;
            Graphics.DrawMesh(frameConfig.meshGlass, containerMatrix, m_rotorMaterial, srcInfo.m_prefabDataIndex, null, 0, materialPropertyBlock);

            materialPropertyBlock.Clear();
            var color = frameConfig.m_inheritColor ? parentColor : frameConfig.OutsideColor;
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, color);
            defaultCallsCounter++;
            Graphics.DrawMesh(frameConfig.meshOuterContainer, containerMatrix, m_outsideMaterial, srcInfo.m_prefabDataIndex, null, 0, materialPropertyBlock, true, true);
        }
        private class TextRenderDescriptor
        {
            public Matrix4x4 baseMatrix;
            public Matrix4x4 rotationMatrix;
            public Matrix4x4 scaleMatrix;
            public PlacingSettings placingSettings;
        }

        private static List<TextRenderDescriptor> CalculateTextMatrix(PlacingSettings placingSettings, ref Vector3 baseScale, UIHorizontalAlignment targetTextAlignment, float maxWidth, BoardTextDescriptorGeneralXml textDescriptor, BasicRenderInformation renderInfo, bool centerReference = false)
        {
            var result = new List<TextRenderDescriptor>();
            if (renderInfo == null)
            {
                return result;
            }

            var textMatrix = ApplyTextAdjustments(placingSettings, renderInfo, ref baseScale, textDescriptor.TextLineHeight, textDescriptor.m_verticalAlignment, targetTextAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference, false);

            result.Add(textMatrix);

            if (placingSettings.m_create180degYClone)
            {
                if (textDescriptor.PlacingConfig.m_invertYCloneHorizontalAlign)
                {
                    targetTextAlignment = 2 - targetTextAlignment;
                }
                result.Add(ApplyTextAdjustments(new PlacingSettings
                {
                    Position = new Vector3Xml { X = placingSettings.Position.X, Y = placingSettings.Position.Y, Z = -placingSettings.Position.Z },
                    Rotation = new Vector3Xml
                    {
                        X = placingSettings.Rotation.X,
                        Y = placingSettings.Rotation.Y + 180,
                        Z = placingSettings.Rotation.Z
                    }
                }, renderInfo, ref baseScale, textDescriptor.TextLineHeight, textDescriptor.m_verticalAlignment, targetTextAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference, false));
            }

            return result;
        }

        private static TextRenderDescriptor ApplyTextAdjustments(PlacingSettings placingSettings, BasicRenderInformation renderInfo, ref Vector3 propScale, float lineHeight, float verticalAlignment, UIHorizontalAlignment horizontalAlignment, float maxWidth, bool applyResizeOverflowOnY, bool centerReference, bool mirrored)
        {
            float overflowScaleX = 1f;
            float overflowScaleY = 1f;
            float defaultMultiplierX = lineHeight / renderInfo.m_refY;
            float defaultMultiplierY = lineHeight / renderInfo.m_refY;
            float realWidth = defaultMultiplierX * renderInfo.m_sizeMetersUnscaled.x;
            Vector3 targetRelativePosition = Vector3.zero;
            //LogUtils.DoWarnLog($"[{renderInfo},{refID},{boardIdx},{secIdx}] realWidth = {realWidth}; realHeight = {realHeight};");
            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(placingSettings.Rotation), Vector3.one);
            var lineRelativePosition = 0f;
            if (maxWidth > 0 && maxWidth < realWidth)
            {
                overflowScaleX = maxWidth / realWidth;
                if (applyResizeOverflowOnY)
                {
                    lineRelativePosition = (lineHeight * defaultMultiplierY * (-verticalAlignment + .5f) * 2) + (verticalAlignment * lineHeight * overflowScaleX * 2);
                    overflowScaleY = overflowScaleX;
                }
            }
            else
            {
                if (maxWidth > 0 && horizontalAlignment != UIHorizontalAlignment.Center)
                {
                    float factor = horizontalAlignment == UIHorizontalAlignment.Left ? 0.5f : -0.5f;
                    targetRelativePosition += new Vector3((maxWidth - realWidth) * factor, 0, 0);
                }
            }
            targetRelativePosition += new Vector3(0, lineRelativePosition - (renderInfo.m_YAxisOverflows.min + renderInfo.m_YAxisOverflows.max) * .5f * defaultMultiplierY * overflowScaleY);

            var scaleVector = centerReference ? new Vector3(1, 1, 1) : new Vector3(defaultMultiplierX * overflowScaleX / propScale.x, defaultMultiplierY * overflowScaleY / propScale.y, mirrored ? -1 : 1);
            Matrix4x4 textMatrix =
                Matrix4x4.Translate(placingSettings.Position) *
                rotationMatrix *
                Matrix4x4.Translate(targetRelativePosition) *
                Matrix4x4.Scale(scaleVector) * Matrix4x4.Scale(propScale)
               ;
            return new TextRenderDescriptor
            {
                baseMatrix = textMatrix,
                scaleMatrix = Matrix4x4.Scale(propScale),
                rotationMatrix = rotationMatrix,
                placingSettings = placingSettings
            };
        }

        #region Illumination handling
        private static void CalculateIllumination(ushort refID, int boardIdx, int secIdx, BoardTextDescriptorGeneralXml textDescriptor, MaterialPropertyBlock materialPropertyBlock, ref Color colorToSet, int instanceFlags, int instanceFlags2)
        {
            Vector4 surfProperties = default;
            var randomizer = new Randomizer((refID << 8) + (boardIdx << 2) + secIdx);
            switch (textDescriptor.IlluminationConfig.IlluminationType)
            {
                default:
                case FontStashSharp.MaterialType.OPAQUE:
                    surfProperties.z = 0;
                    break;
                case FontStashSharp.MaterialType.DAYNIGHT:
                    float num = m_daynightOffTime + (randomizer.Int32(100000u) * 1E-05f);
                    surfProperties.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, Singleton<RenderManager>.instance.lightSystem.DayLightIntensity) * textDescriptor.IlluminationConfig.m_illuminationStrength;
                    break;
                case FontStashSharp.MaterialType.FLAGS:
                    surfProperties.z
                        = ((instanceFlags & textDescriptor.IlluminationConfig.m_requiredFlags) == textDescriptor.IlluminationConfig.m_requiredFlags)
                        && ((instanceFlags & textDescriptor.IlluminationConfig.m_forbiddenFlags) == 0)
                        && ((instanceFlags2 & textDescriptor.IlluminationConfig.m_requiredFlags2) == textDescriptor.IlluminationConfig.m_requiredFlags2)
                        && ((instanceFlags2 & textDescriptor.IlluminationConfig.m_forbiddenFlags2) == 0)
                        ? textDescriptor.IlluminationConfig.m_illuminationStrength : 0;
                    break;
                case FontStashSharp.MaterialType.BRIGHT:
                    surfProperties.z = textDescriptor.IlluminationConfig.m_illuminationStrength;
                    break;
            }
            colorToSet *= Color.Lerp(new Color32(200, 200, 200, 255), Color.white, surfProperties.z);
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, colorToSet);


            if (surfProperties.z > 0 && textDescriptor.IlluminationConfig.BlinkType != BlinkType.None)
            {
                CalculateBlinkEffect(textDescriptor, ref surfProperties, ref randomizer);
            }

            surfProperties.x = -textDescriptor.IlluminationConfig.m_illuminationDepth;
            materialPropertyBlock.SetVector(SHADER_PROP_SURF_PROPERTIES, surfProperties);
        }
        private static void CalculateBlinkEffect(BoardTextDescriptorGeneralXml textDescriptor, ref Vector4 objectIndex, ref Randomizer randomizer)
        {
            float num = m_daynightOffTime + (randomizer.Int32(100000u) * 1E-05f);
            Vector4 blinkVector;
            if (textDescriptor.IlluminationConfig.BlinkType == BlinkType.Custom)
            {
                blinkVector = textDescriptor.IlluminationConfig.CustomBlink;
            }
            else
            {
                blinkVector = LightEffect.GetBlinkVector((LightEffect.BlinkType)textDescriptor.IlluminationConfig.BlinkType);
            }
            float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
            num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
            float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
            float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
            objectIndex.z *= 1f - (num3 * num4);
        }
        #endregion

        private static Color GetTextColor(ushort refID, int boardIdx, int secIdx, BaseWriteOnXml descriptor, BoardTextDescriptorGeneralXml textDescriptor)
        {
            if (textDescriptor.ColoringConfig.UseContrastColor)
            {
                return GetContrastColor(refID, boardIdx, secIdx, descriptor);
            }
            else if (textDescriptor.ColoringConfig.m_cachedColor != null)
            {
                return textDescriptor.ColoringConfig.m_cachedColor;
            }
            return Color.white;
        }
        public static Color GetContrastColor(ushort refID, int boardIdx, int secIdx, BaseWriteOnXml instance)
        {
            //if (instance is BoardInstanceRoadNodeXml)
            //{
            //    return WTSRoadNodesData.Instance.BoardsContainers[refID, boardIdx, secIdx]?.m_cachedContrastColor ?? Color.black;
            //}
            //else if (instance is BoardPreviewInstanceXml preview)
            //{
            //    return (preview?.Descriptor?.FixedColor ?? GetCurrentSimulationColor()).ContrastColor();
            //}
            var targetColor = WEDynamicTextRenderingRules.GetPropColor(refID, boardIdx, secIdx, instance, out bool colorFound);
            return (colorFound ? targetColor : Color.black).ContrastColor();
        }
    }

}
