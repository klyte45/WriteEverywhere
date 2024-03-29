﻿
using ColossalFramework;
using ColossalFramework.Math;
using Kwytto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Font.Utility;
using WriteEverywhere.Layout;
using WriteEverywhere.TransportLines;
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
        public static readonly float RENDER_DISTANCE_FACTOR = 1500f;
        private static readonly float m_daynightOffTime = 6 * Convert.ToSingle(Math.Pow(Convert.ToDouble((6 - (15 / 2.5)) / 6), Convert.ToDouble(1 / 1.09)));


        internal static Vector3 RenderTextMesh(WriteOnBuildingXml propGroup, ushort refID, int boardIdx, int secIdx, ref Color parentColor, BaseWriteOnXml baseWrite, TextToWriteOnXml textDescriptor, ref Matrix4x4 propMatrix, PrefabInfo srcInfo, int instanceFlags, int instanceFlags2, bool currentEditingSizeLine, ref int defaultCallsCounter)
        {
            if (!textDescriptor.TestFlags(instanceFlags, instanceFlags2))
            {
                return default;
            }
            BasicRenderInformation bri = WETextMeshProcess.GetTextMesh(propGroup, textDescriptor, refID, boardIdx, secIdx, baseWrite);
            if (bri?.m_mesh is null || bri?.m_generatedMaterial is null)
            {
                return default;
            }
            var textColor = GetTargetColor(ref propMatrix, refID, boardIdx, secIdx, baseWrite, textDescriptor.ColoringConfig.m_colorSource, textDescriptor.ColoringConfig.m_cachedColor, textDescriptor.ColoringConfig.m_useFixedIfMultiline);
            Vector3 scl = baseWrite.PropScale;
            return DrawTextBri(baseWrite, refID, boardIdx, secIdx, ref propMatrix, textDescriptor, bri, ref textColor, textDescriptor.PlacingConfig, ref parentColor, srcInfo, ref scl, textDescriptor.m_horizontalAlignment, textDescriptor.MaxWidthMeters, instanceFlags, instanceFlags2, currentEditingSizeLine, ref defaultCallsCounter);

        }

        private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private static Vector3 DrawTextBri(BaseWriteOnXml baseWrite, ushort refID, int boardIdx, int secIdx, ref Matrix4x4 propMatrix, TextToWriteOnXml textDescriptor,
        BasicRenderInformation renderInfo, ref Color colorToSet, PlacingSettings placingSettings, ref Color parentColor, PrefabInfo srcInfo,
        ref Vector3 baseScale, float horizontalAlignment, float maxWidth, int instanceFlags, int instanceFlags2, bool currentEditingSizeLine, ref int defaultCallsCounter)
        {

            var textMatrixes = CalculateTextMatrix(placingSettings, ref baseScale, horizontalAlignment, maxWidth, textDescriptor, renderInfo);
            var positionAccumulator = new Vector3();
            foreach (var textItem in textMatrixes)
            {
                Matrix4x4 matrix = propMatrix * textItem.baseMatrix;

                block.Clear();
                var surfProperties = CalculateIllumination(refID, boardIdx, secIdx, textDescriptor, instanceFlags, instanceFlags2);
                block.SetVector(SHADER_PROP_SURF_PROPERTIES, surfProperties);
                block.SetColor(SHADER_PROP_COLOR, colorToSet *= Color.Lerp(new Color32(200, 200, 200, 255), Color.white, surfProperties.z));
                block.SetColor(SHADER_PROP_BACK_COLOR, textDescriptor.ColoringConfig.UseFrontColorAsBackColor ? colorToSet : textDescriptor.ColoringConfig.m_cachedBackColor);
                block.SetVector(SHADER_PROP_DIMENSIONS, textItem.finalSize);
                RenderBri(renderInfo, ref defaultCallsCounter, matrix, srcInfo.m_prefabDataIndex, block);

                positionAccumulator += (Vector3)matrix.GetColumn(3) + new Vector3(0, renderInfo.m_mesh.bounds.center.y * matrix.GetColumn(1).y);
                if (currentEditingSizeLine)
                {
                    DrawBgMesh(ref propMatrix,
                        textDescriptor.LineMaxDimensions,
                        Color.magenta,
                        Color.magenta,
                        .5f,
                        block, default,
                        textItem.placingSettings,
                        textItem,
                        null,
                        srcInfo,
                        ModInstance.Controller.AtlasesLibrary.GetWhiteTextureBRI(), textDescriptor.TextLineHeight,
                        ref defaultCallsCounter,
                        0,
                        1,
                        ModInstance.Controller.highlightMaterial);
                }
                if (((Vector2)textDescriptor.BackgroundMeshSettings.Size).sqrMagnitude != 0)
                {
                    var bgColor = GetTargetColor(ref propMatrix, refID, boardIdx, secIdx, baseWrite, textDescriptor.BackgroundMeshSettings.m_colorSource, textDescriptor.BackgroundMeshSettings.m_bgFrontColor, textDescriptor.ColoringConfig.m_useFixedIfMultiline);
                    Matrix4x4 containerMatrix = DrawBgMesh(ref propMatrix,
                        textDescriptor.BackgroundMeshSettings.Size,
                        bgColor,
                        textDescriptor.BackgroundMeshSettings.m_cachedBackColor,
                        textDescriptor.BackgroundMeshSettings.m_verticalAlignment,
                        block, surfProperties,
                        textItem.placingSettings,
                        textItem,
                        textDescriptor.BackgroundMeshSettings.BgImage,
                        srcInfo,
                        ModInstance.Controller.AtlasesLibrary.GetWhiteTextureBRI(), textDescriptor.TextLineHeight,
                        ref defaultCallsCounter,
                        textDescriptor.BackgroundMeshSettings.m_normalStrength,
                        currentEditingSizeLine ? 2 : 1);
                    if (textDescriptor.BackgroundMeshSettings.m_useFrame)
                    {
                        var frameConfig = textDescriptor.BackgroundMeshSettings.FrameMeshSettings;
                        DrawTextFrame(textDescriptor, block, placingSettings, ref baseScale, GetTargetColor(ref containerMatrix, refID, boardIdx, secIdx, baseWrite, frameConfig.m_colorSource, frameConfig.OutsideColor, frameConfig.m_useFixedIfMultiline), srcInfo, ref containerMatrix, ref defaultCallsCounter);
                    }
                }

            }
            return positionAccumulator / textMatrixes.Count;
        }

        public static int RenderBri(BasicRenderInformation renderInfo, ref int defaultCallsCounter, Matrix4x4 matrix, int layer, MaterialPropertyBlock block)
        {
            block.SetFloat(SHADER_PROP_PIXELS_METERS, renderInfo.m_pixelDensityMeters);
            block.SetVector(SHADER_PROP_BORDERS, renderInfo.m_borders);
            defaultCallsCounter++;
            Graphics.DrawMesh(renderInfo.m_mesh, matrix, renderInfo.m_generatedMaterial, layer, null, 0, block);
            return defaultCallsCounter;
        }

        private static Matrix4x4 DrawBgMesh(ref Matrix4x4 propMatrix, Vector2 size, Color color, Color backColor, float verticalAlignment, MaterialPropertyBlock materialPropertyBlock, Vector4 surfProperties,
            PlacingSettings placingSettings, TextRenderDescriptor textMatrixTuple, TextParameterWrapper bgImage, PrefabInfo srcInfo, BasicRenderInformation bgBri, float lineHeight,
             ref int defaultCallsCounter, float normalStrength, float zDistanceMultiplier, Material overrideMaterial = null)
        {
            materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor(SHADER_PROP_SURF_PROPERTIES, new Vector4(normalStrength, 0, surfProperties.z));
            color *= Color.Lerp(new Color32(200, 200, 200, 255), Color.white, surfProperties.z);
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, color);
            materialPropertyBlock.SetColor(SHADER_PROP_BACK_COLOR, backColor);

            var containerMatrix = propMatrix
                * Matrix4x4.Translate(placingSettings.Position)
                * textMatrixTuple.rotationMatrix
                * Matrix4x4.Translate(new Vector3(0, (size.y * (-verticalAlignment + .5f)) + (verticalAlignment * lineHeight)))
                * textMatrixTuple.scaleMatrix
                * Matrix4x4.Translate(new Vector3(0, 0, Mathf.Sign(textMatrixTuple.rotationMatrix.determinant) * -0.001f * zDistanceMultiplier))
            ;
            var bgMatrix = containerMatrix * Matrix4x4.Scale(new Vector3(size.x, size.y, 1));


            if (overrideMaterial is null && bgImage != null && bgImage.ParamType == ParameterType.IMAGE && ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(bgImage.AtlasName, bgImage.TextOrSpriteValue, true) is BasicRenderInformation image)
            {
                overrideMaterial = image.m_generatedMaterial;
                block.SetVector(SHADER_PROP_DIMENSIONS, size);
                block.SetFloat(SHADER_PROP_PIXELS_METERS, image.m_pixelDensityMeters);
                block.SetVector(SHADER_PROP_BORDERS, image.m_borders);
            }

            defaultCallsCounter++;
            Graphics.DrawMesh(bgBri.m_mesh, bgMatrix, overrideMaterial ?? bgBri.m_generatedMaterial, srcInfo.m_prefabDataIndex, null, 0, materialPropertyBlock);
            return containerMatrix;
        }

        internal static ref Vector2[] CachedUvGlass => ref WEMainController.__cachedUvGlass;
        internal static ref Material RotorMaterial => ref WEMainController.m_rotorMaterial;
        internal static ref Material OutsideMaterial => ref WEMainController.m_outsideMaterial;

        private static void DrawTextFrame(TextToWriteOnXml textDescriptor, MaterialPropertyBlock materialPropertyBlock, PlacingSettings placingSettings, ref Vector3 baseScale, Color color, PrefabInfo srcInfo, ref Matrix4x4 containerMatrix, ref int defaultCallsCounter)
        {
            var frameConfig = textDescriptor.BackgroundMeshSettings.FrameMeshSettings;


            var instance2 = Singleton<VehicleManager>.instance;
            if (RotorMaterial == null)
            {
                RotorMaterial = new Material(Shader.Find("Custom/Vehicles/Vehicle/Rotors"))
                {
                    mainTexture = Texture2D.whiteTexture
                };
                var targetTexture = new Texture2D(1, 1);
                targetTexture.SetPixels(targetTexture.GetPixels().Select(x => new Color(.5f, .5f, 0.7f, 1)).ToArray());
                targetTexture.Apply();
                RotorMaterial.SetTexture("_XYSMap", targetTexture);
                var targetTextureACI = new Texture2D(1, 1);
            }
            if (OutsideMaterial == null)
            {
                OutsideMaterial = new Material(ModInstance.Controller.defaultFrameShader);
            }

            if (frameConfig.meshOuterContainer is null)
            {
                WEDisplayContainerMeshUtils.GenerateDisplayContainer(textDescriptor.BackgroundMeshSettings.Size,
                    frameConfig.BackSize,
                    frameConfig.BackOffset,
                    frameConfig.FrontDepth,
                    frameConfig.BackDepth,
                    frameConfig.FrontBorderThickness,
                    out frameConfig.meshOuterContainer,
                    out var glassVerts);

                frameConfig.meshGlass = new Mesh()
                {
                    vertices = glassVerts,
                    triangles = WEDisplayContainerMeshUtils.m_trianglesGlass,
                    uv = CachedUvGlass,
                    colors = glassVerts.Select(x => new Color(1 - frameConfig.GlassTransparency, 0, 0, 0)).ToArray(),
                };
                WTSUtils.SolveTangents(frameConfig.meshGlass);
                frameConfig.meshOuterContainer.RecalculateTangents();
                frameConfig.meshOuterContainer.RecalculateBounds();

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
            Graphics.DrawMesh(frameConfig.meshGlass, containerMatrix, RotorMaterial, srcInfo.m_prefabDataIndex, null, 0, materialPropertyBlock);

            materialPropertyBlock.Clear();
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, color);
            defaultCallsCounter++;
            Graphics.DrawMesh(frameConfig.meshOuterContainer, containerMatrix, OutsideMaterial, srcInfo.m_prefabDataIndex, null, 0, materialPropertyBlock);
        }
        private class TextRenderDescriptor
        {
            public Matrix4x4 baseMatrix;
            public Matrix4x4 rotationMatrix;
            public Matrix4x4 scaleMatrix;
            public PlacingSettings placingSettings;
            public Vector2 finalSize;
        }

        private static List<TextRenderDescriptor> CalculateTextMatrix(PlacingSettings placingSettings, ref Vector3 baseScale, float horizontalAlignment, float maxWidth, TextToWriteOnXml textDescriptor, BasicRenderInformation renderInfo, bool centerReference = false)
        {
            var result = new List<TextRenderDescriptor>();
            if (renderInfo == null)
            {
                return result;
            }

            var textMatrix = ApplyTextAdjustments(placingSettings, renderInfo, ref baseScale, textDescriptor.TextLineHeight, textDescriptor.m_verticalAlignment, horizontalAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference);

            result.Add(textMatrix);

            if (placingSettings.m_yCloneType != YCloneType.None)
            {
                if (textDescriptor.PlacingConfig.m_invertYCloneHorizontalAlign)
                {
                    horizontalAlignment = 1 - horizontalAlignment;
                }
                result.Add(ApplyTextAdjustments(new PlacingSettings
                {
                    Position = new Vector3Xml { X = (placingSettings.m_yCloneType == YCloneType.OnX ? -1 : 1) * placingSettings.Position.X, Y = placingSettings.Position.Y, Z = (placingSettings.m_yCloneType == YCloneType.OnZ ? -1 : 1) * placingSettings.Position.Z },
                    Rotation = new Vector3Xml
                    {
                        X = placingSettings.Rotation.X,
                        Y = placingSettings.Rotation.Y + 180,
                        Z = placingSettings.Rotation.Z
                    }
                }, renderInfo, ref baseScale, textDescriptor.TextLineHeight, textDescriptor.m_verticalAlignment, horizontalAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference));
            }

            return result;
        }

        private static TextRenderDescriptor ApplyTextAdjustments(PlacingSettings placingSettings, BasicRenderInformation renderInfo, ref Vector3 propScale, float lineHeight, float verticalAlignment, float horizontalAlignment, float maxWidth, bool applyResizeOverflowOnY, bool centerReference)
        {
            float overflowScaleX = 1f;
            float overflowScaleY = 1f;
            float defaultMultiplierX = lineHeight / renderInfo.m_refY;
            float defaultMultiplierY = lineHeight / renderInfo.m_refY;
            float realWidth = defaultMultiplierX * renderInfo.m_sizeMetersUnscaled.x;
            Vector3 targetRelativePosition = Vector3.zero;

            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(placingSettings.Rotation), Vector3.one);
            var lineRelativePosition = 0f;
            if (maxWidth > 0)
            {
                if (maxWidth < realWidth)
                {
                    overflowScaleX = maxWidth / realWidth;
                    if (applyResizeOverflowOnY)
                    {
                        lineRelativePosition = lineHeight * (verticalAlignment * (1 - overflowScaleX));
                        overflowScaleY = overflowScaleX;
                    }
                }
                else if (renderInfo.m_expandXIfAlone && !applyResizeOverflowOnY)
                {
                    overflowScaleX = maxWidth / realWidth;
                }
                else
                {
                    targetRelativePosition += new Vector3((maxWidth - realWidth) * (.5f - horizontalAlignment), 0, 0);
                }

            }
            if (renderInfo.m_lineOffset != 0)
            {
                lineRelativePosition += overflowScaleY * renderInfo.m_lineOffset * defaultMultiplierY;
            }
            targetRelativePosition += new Vector3(0, lineRelativePosition - (renderInfo.m_YAxisOverflows.min + renderInfo.m_YAxisOverflows.max) * .5f * defaultMultiplierY * overflowScaleY);

            var scaleVector = centerReference ? new Vector3(1, 1, 1) : new Vector3(defaultMultiplierX * overflowScaleX / propScale.x * renderInfo.m_offsetScaleX, defaultMultiplierY * overflowScaleY / propScale.y, 1);
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
                placingSettings = placingSettings,
                finalSize = new Vector2(scaleVector.x * propScale.x, scaleVector.y * propScale.y)
            };
        }

        #region Illumination handling
        private static Vector4 CalculateIllumination(ushort refID, int boardIdx, int secIdx, TextToWriteOnXml textDescriptor, int instanceFlags, int instanceFlags2)
        {
            Vector4 surfProperties = default;
            var randomizer = new Randomizer((refID << 8) + (boardIdx << 2) + secIdx);
            switch (textDescriptor.IlluminationConfig.IlluminationType)
            {
                default:
                case MaterialType.OPAQUE:
                    surfProperties.z = 0;
                    break;
                case MaterialType.DAYNIGHT:
                    float num = m_daynightOffTime + (randomizer.Int32(100000u) * 1E-05f);
                    surfProperties.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, Singleton<RenderManager>.instance.lightSystem.DayLightIntensity) * textDescriptor.IlluminationConfig.m_illuminationStrength;
                    break;
                case MaterialType.FLAGS:
                    surfProperties.z =
                        textDescriptor.TestFlags(instanceFlags, instanceFlags2)
                        ? textDescriptor.IlluminationConfig.m_illuminationStrength
                        : 0;
                    break;
                case MaterialType.BRIGHT:
                    surfProperties.z = textDescriptor.IlluminationConfig.m_illuminationStrength;
                    break;
            }

            if (surfProperties.z > 0 && textDescriptor.IlluminationConfig.BlinkType != BlinkType.None)
            {
                CalculateBlinkEffect(textDescriptor, ref surfProperties, ref randomizer);
            }

            surfProperties.x = textDescriptor.IlluminationConfig.m_illuminationDepth;
            return surfProperties;
        }
        private static void CalculateBlinkEffect(TextToWriteOnXml textDescriptor, ref Vector4 objectIndex, ref Randomizer randomizer)
        {
            float num = m_daynightOffTime + (randomizer.Int32(100000u) * 1E-05f);
            Vector4 blinkVector = textDescriptor.IlluminationConfig.BlinkType == BlinkType.Custom
                ? (Vector4)textDescriptor.IlluminationConfig.CustomBlink
                : LightEffect.GetBlinkVector((LightEffect.BlinkType)textDescriptor.IlluminationConfig.BlinkType);
            float num2 = num * 3.71f + (Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w);
            num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
            float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
            float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
            objectIndex.z *= 1f - (num3 * num4);
        }
        #endregion

        private static Color GetTargetColor(ref Matrix4x4 propMatrix, ushort refID, int boardIdx, int secIdx, BaseWriteOnXml descriptor, ColoringSource clrSrc, Color? fixedColor, bool fixedIfMultiline)
        {
            switch (clrSrc)
            {
                case ColoringSource.Prop:
                case ColoringSource.ContrastProp:
                    var targetColor = WEDynamicTextRenderingRules.GetPropColor(refID, boardIdx, secIdx, descriptor, out bool colorFound);
                    targetColor = (colorFound ? targetColor : Color.black);
                    return clrSrc == ColoringSource.ContrastProp ? targetColor.ContrastColor() : targetColor;
                case ColoringSource.District:
                case ColoringSource.ContrastDistrict:
                    Color districtColor;
                    if (descriptor is WriteOnBuildingPropXml)
                    {
                        districtColor = ModInstance.Controller.ConnectorCD.GetDistrictColor(DistrictManager.instance.GetDistrict(BuildingManager.instance.m_buildings.m_buffer[refID].m_position));
                    }
                    else if (descriptor is WriteOnNetXml)
                    {
                        districtColor = ModInstance.Controller.ConnectorCD.GetDistrictColor(DistrictManager.instance.GetDistrict(propMatrix.GetColumn(3)));
                    }
                    else
                    {
                        break;
                    }
                    return clrSrc == ColoringSource.District ? districtColor : districtColor.ContrastColor();
                case ColoringSource.PlatformLine:
                case ColoringSource.ContrastPlatformLine:
                    if (descriptor is WriteOnBuildingPropXml bpx)
                    {
                        var targetStopsList = WTSStopUtils.GetAllTargetStopInfo(bpx, refID);
                        if (targetStopsList.Length == 0 || (targetStopsList.Length > 1 && fixedIfMultiline))
                        {
                            break;
                        }
                        var effectiveStop = targetStopsList[0];
                        var effColor = ModInstance.Controller.ConnectorTLM.GetLineColor(new WTSLine(effectiveStop));
                        return clrSrc == ColoringSource.PlatformLine ? effColor : effColor.ContrastColor();
                    }
                    else if (descriptor is LayoutDescriptorVehicleXml)
                    {
                        var line = VehicleManager.instance.m_vehicles.m_buffer[refID].m_transportLine;
                        if (line == 0)
                        {
                            break;
                        }
                        var effColor = ModInstance.Controller.ConnectorTLM.GetLineColor(new WTSLine(line, false));
                        return clrSrc == ColoringSource.PlatformLine ? effColor : effColor.ContrastColor();
                    }
                    break;
            }
            return fixedColor ?? Color.white;
        }
    }

}
