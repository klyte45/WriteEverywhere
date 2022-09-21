using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Kwytto.UI;
using Kwytto.Utils;
using SpriteFontPlus.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        public static Vector3 RenderTextMesh(ushort refID, int boardIdx, int secIdx, BaseWriteOnXml baseWrite, BoardTextDescriptorGeneralXml textDescriptor, Matrix4x4 propMatrix, int instanceFlags, int instanceFlags2, ref int defaultCallsCounter)
        {
            BasicRenderInformation bri = WTSTextMeshProcess.GetTextMesh(textDescriptor, refID, boardIdx, secIdx, baseWrite);
            if (bri?.m_mesh is null || bri?.m_generatedMaterial is null)
            {
                return default;
            }

            return DrawTextBri(refID, boardIdx, secIdx, propMatrix, textDescriptor, bri, GetTextColor(refID, boardIdx, secIdx, baseWrite, textDescriptor), textDescriptor.PlacingConfig.Position, textDescriptor.PlacingConfig.Rotation, baseWrite.PropScale, textDescriptor.PlacingConfig.m_create180degYClone, textDescriptor.m_textAlign, textDescriptor.MaxWidthMeters, instanceFlags, instanceFlags2, ref defaultCallsCounter);

        }

        private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private static Vector3 DrawTextBri(ushort refID, int boardIdx, int secIdx, Matrix4x4 propMatrix, BoardTextDescriptorGeneralXml textDescriptor,
        BasicRenderInformation renderInfo, Color colorToSet, Vector3 targetPos, Vector3 targetRotation,
        Vector3 baseScale, bool placeClone180Y, UIHorizontalAlignment targetTextAlignment, float maxWidth, int instanceFlags, int instanceFlags2, ref int defaultCallsCounter)
        {

            var textMatrixes = CalculateTextMatrix(targetPos, targetRotation, baseScale, targetTextAlignment, maxWidth, textDescriptor, renderInfo, placeClone180Y);
            var positionAccumulator = new Vector3();
            foreach (var textItem in textMatrixes)
            {
                Matrix4x4 matrix = propMatrix * textItem.baseMatrix;

                block.Clear();

                Material targetMaterial = renderInfo.m_generatedMaterial;
                PropManager instance = CalculateIllumination(refID, boardIdx, secIdx, textDescriptor, block, ref colorToSet, instanceFlags, instanceFlags2);

                defaultCallsCounter++;
                Graphics.DrawMesh(renderInfo.m_mesh, matrix, targetMaterial, 10, null, 0, block, false);
                positionAccumulator += (Vector3)matrix.GetColumn(3) + new Vector3(0, renderInfo.m_mesh.bounds.center.y * matrix.GetColumn(1).y);
                if (((Vector2)textDescriptor.BackgroundMeshSettings.Size).sqrMagnitude != 0)
                {
                    BasicRenderInformation bgBri = ModInstance.Controller.AtlasesLibrary.GetFromLocalAtlases(null, CommonsSpriteNames.K45_SquareIcon.ToString());
                    if (bgBri != null)
                    {
                        Matrix4x4 containerMatrix = DrawBgMesh(ref propMatrix, textDescriptor, block, ref targetPos, ref targetRotation, ref baseScale, targetTextAlignment, textItem, instance, bgBri, ref defaultCallsCounter);
                        //        if (textDescriptor.BackgroundMeshSettings.m_useFrame)
                        //        {
                        //            DrawTextFrame(textDescriptor, block, ref targetPos, ref targetRotation, ref baseScale, ref parentColor, srcInfo, targetCamera, ref containerMatrix, ref defaultCallsCounter);
                        //        }
                    }
                }
            }
            return positionAccumulator / textMatrixes.Count;
        }

        private static Matrix4x4 DrawBgMesh(ref Matrix4x4 propMatrix, BoardTextDescriptorGeneralXml textDescriptor, MaterialPropertyBlock materialPropertyBlock, ref Vector3 targetPos,
            ref Vector3 targetRotation, ref Vector3 baseScale, UIHorizontalAlignment targetTextAlignment, TextRenderDescriptor textMatrixTuple,
            PropManager instance, BasicRenderInformation bgBri, ref int defaultCallsCounter)
        {
            materialPropertyBlock.SetColor(SHADER_PROP_COLOR, textDescriptor.BackgroundMeshSettings.BackgroundColor * new Color(1, 1, 1, 0));
            materialPropertyBlock.SetVector(SHADER_PROP_SURF_PROPERTIES, new Vector4());
            ApplyTextAdjustments(targetPos, targetRotation, bgBri, baseScale, textDescriptor.BackgroundMeshSettings.Size.Y, targetTextAlignment, textDescriptor.BackgroundMeshSettings.Size.X, false, false, false);

            var lineAdjustmentVector = new Vector3(0, .5f * (textMatrixTuple.scaleMatrix * propMatrix.inverse).m11  * textDescriptor.TextLineHeight, -0.001f);
            var containerMatrix = propMatrix
                * Matrix4x4.Translate(targetPos)
                * textMatrixTuple.rotationMatrix
                * Matrix4x4.Translate(lineAdjustmentVector)
                * textMatrixTuple.scaleMatrix
                ;
            var bgMatrix = propMatrix
                * Matrix4x4.Translate(targetPos)
                * textMatrixTuple.rotationMatrix
                * textMatrixTuple.scaleMatrix
                * Matrix4x4.Translate(lineAdjustmentVector)
                * Matrix4x4.Scale(new Vector3(textDescriptor.BackgroundMeshSettings.Size.X / bgBri.m_mesh.bounds.size.x, textDescriptor.BackgroundMeshSettings.Size.Y / bgBri.m_mesh.bounds.size.y, 1))
                ;
            defaultCallsCounter++;
            Graphics.DrawMesh(bgBri.m_mesh, bgMatrix, bgBri.m_generatedMaterial, 10, null, 0, materialPropertyBlock);
            return containerMatrix;
        }

        private class TextRenderDescriptor
        {
            public Matrix4x4 baseMatrix;
            public Matrix4x4 rotationMatrix;
            public Matrix4x4 scaleMatrix;
        }

        private static List<TextRenderDescriptor> CalculateTextMatrix(Vector3 targetPosition, Vector3 targetRotation, Vector3 baseScale, UIHorizontalAlignment targetTextAlignment, float maxWidth, BoardTextDescriptorGeneralXml textDescriptor, BasicRenderInformation renderInfo, bool placeClone180Y, bool centerReference = false)
        {
            var result = new List<TextRenderDescriptor>();
            if (renderInfo == null)
            {
                return result;
            }

            var textMatrix = ApplyTextAdjustments(targetPosition, targetRotation, renderInfo, baseScale, textDescriptor.TextLineHeight, targetTextAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference, textDescriptor.PlacingConfig.m_mirrored);

            result.Add(textMatrix);

            if (placeClone180Y)
            {
                if (textDescriptor.PlacingConfig.m_invertYCloneHorizontalAlign)
                {
                    targetTextAlignment = 2 - targetTextAlignment;
                }
                result.Add(ApplyTextAdjustments(new Vector3(targetPosition.x, targetPosition.y, -targetPosition.z), targetRotation + new Vector3(0, 180), renderInfo, baseScale, textDescriptor.TextLineHeight, targetTextAlignment, maxWidth, textDescriptor.m_applyOverflowResizingOnY, centerReference, textDescriptor.PlacingConfig.m_mirrored));
            }

            return result;
        }

        private static TextRenderDescriptor ApplyTextAdjustments(Vector3 textPosition, Vector3 textRotation, BasicRenderInformation renderInfo, Vector3 propScale, float textScale, UIHorizontalAlignment horizontalAlignment, float maxWidth, bool applyResizeOverflowOnY, bool centerReference, bool mirrored)
        {
            float overflowScaleX = 1f;
            float overflowScaleY = 1f;
            float defaultMultiplierX = textScale / renderInfo.m_refY;
            float defaultMultiplierY = textScale / renderInfo.m_refY;
            float realWidth = defaultMultiplierX * renderInfo.m_sizeMetersUnscaled.x;
            Vector3 targetRelativePosition = textPosition;
            //LogUtils.DoWarnLog($"[{renderInfo},{refID},{boardIdx},{secIdx}] realWidth = {realWidth}; realHeight = {realHeight};");
            var rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(textRotation), Vector3.one);
            if (maxWidth > 0 && maxWidth < realWidth)
            {
                overflowScaleX = maxWidth / realWidth;
                if (applyResizeOverflowOnY)
                {
                    overflowScaleY = overflowScaleX;
                }
            }
            else
            {
                if (maxWidth > 0 && horizontalAlignment != UIHorizontalAlignment.Center)
                {
                    float factor = horizontalAlignment == UIHorizontalAlignment.Left ? 0.5f : -0.5f;
                    targetRelativePosition += rotationMatrix.MultiplyPoint(new Vector3((maxWidth - realWidth) * factor, 0, 0));
                }
            }
            targetRelativePosition += rotationMatrix.MultiplyPoint(new Vector3(0, -(renderInfo.m_YAxisOverflows.min + renderInfo.m_YAxisOverflows.max) / 2 * defaultMultiplierY * overflowScaleY));

            var scaleVector = centerReference ? new Vector3(1, 1, 1) : new Vector3(defaultMultiplierX * overflowScaleX / propScale.x, defaultMultiplierY * overflowScaleY / propScale.y, mirrored ? -1 : 1);
            Matrix4x4 textMatrix =
                Matrix4x4.Translate(targetRelativePosition) *
                rotationMatrix *
                Matrix4x4.Scale(scaleVector) * Matrix4x4.Scale(propScale)
               ;
            return new TextRenderDescriptor
            {
                baseMatrix = textMatrix,
                scaleMatrix = Matrix4x4.Scale(propScale),
                rotationMatrix = rotationMatrix
            };
        }

        #region Illumination handling
        private static PropManager CalculateIllumination(ushort refID, int boardIdx, int secIdx, BoardTextDescriptorGeneralXml textDescriptor, MaterialPropertyBlock materialPropertyBlock, ref Color colorToSet, int instanceFlags, int instanceFlags2)
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

            PropManager instance = Singleton<PropManager>.instance;
            materialPropertyBlock.SetVector(SHADER_PROP_SURF_PROPERTIES, surfProperties);
            return instance;
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
