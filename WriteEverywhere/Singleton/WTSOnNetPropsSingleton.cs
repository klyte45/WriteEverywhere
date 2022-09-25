extern alias ADR;
using ColossalFramework.Math;
using Kwytto.Utils;
using System.Collections.Generic;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Rendering;
using WriteEverywhere.UI;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Singleton
{
    public class WTSOnNetPropsSingleton : MonoBehaviour
    {
        public WTSOnNetData Data => WTSOnNetData.Instance;

        #region Initialize

        public void Start() => ModInstance.Controller.EventOnDistrictChanged += OnDistrictChange;


        private void OnDistrictChange() => WTSCacheSingleton.ClearCacheDistrictArea();
        #endregion



        public void AfterRenderInstanceImpl(RenderManager.CameraInfo cameraInfo, ushort segmentId, ref NetSegment data)
        {

            ref WriteOnNetGroupXml itemGroup = ref Data.m_boardsContainers[segmentId];
            if (itemGroup == null || !itemGroup.HasAnyBoard())
            {
                return;
            }
            for (var i = 0; i < itemGroup.BoardsData.Length; i++)
            {
                var targetDescriptor = itemGroup.BoardsData[i];
                if (targetDescriptor?.SimpleProp is null)
                {
                    continue;
                }

                if (targetDescriptor.m_cachedPositions == null || targetDescriptor.m_cachedRotations == null)
                {
                    bool segmentInverted = (data.m_flags & NetSegment.Flags.Invert) > 0;

                    targetDescriptor.m_cachedPositions = new List<Vector3Xml>();
                    targetDescriptor.m_cachedRotations = new List<Vector3Xml>();
                    if (targetDescriptor.SegmentPositionRepeating)
                    {
                        if (targetDescriptor.SegmentPositionRepeatCount == 1)
                        {
                            var segPos = (targetDescriptor.SegmentPositionStart + targetDescriptor.SegmentPositionEnd) * .5f;
                            CreateSegmentRenderInstance(ref data, targetDescriptor, segmentInverted, segPos);
                        }
                        else if (targetDescriptor.SegmentPositionRepeatCount > 0)
                        {
                            var step = (targetDescriptor.SegmentPositionEnd - targetDescriptor.SegmentPositionStart) / (targetDescriptor.SegmentPositionRepeatCount - 1);
                            for (int k = 0; k < targetDescriptor.SegmentPositionRepeatCount; k++)
                            {
                                CreateSegmentRenderInstance(ref data, targetDescriptor, segmentInverted, targetDescriptor.SegmentPositionStart + (step * k));
                            }
                        }
                    }
                    else
                    {
                        var segPos = targetDescriptor.SegmentPosition;
                        CreateSegmentRenderInstance(ref data, targetDescriptor, segmentInverted, segPos);
                    }
                }
                RenderSign(cameraInfo, segmentId, i, ref targetDescriptor, targetDescriptor.SimpleCachedProp);
            }

        }

        private static void CreateSegmentRenderInstance(ref NetSegment data, OnNetInstanceCacheContainerXml targetDescriptor, bool segmentInverted, float segPos)
        {
            Vector3Xml cachedPosition;
            Vector3Xml cachedRotation;
            float effectiveSegmentPos = segmentInverted ? 1 - segPos : segPos;
            Vector3 bezierPos = data.GetBezier().Position(effectiveSegmentPos);

            data.GetClosestPositionAndDirection(bezierPos, out _, out Vector3 dir);
            float rotation = dir.GetAngleXZ();
            rotation += targetDescriptor.PivotPosition.GetRotationZ() + (segmentInverted ? 180 : 0);


            Vector3 rotationVectorX = VectorUtils.X_Y(KMathUtils.DegreeToVector2(rotation - 90));
            cachedPosition = (Vector3Xml)(bezierPos + (rotationVectorX * ((data.Info.m_halfWidth * targetDescriptor.PivotPosition.GetSideOffsetPositionMultiplier()) + targetDescriptor.PropPosition.X)) + (VectorUtils.X_Y(KMathUtils.DegreeToVector2(rotation)) * targetDescriptor.PropPosition.Z));
            cachedPosition.Y += targetDescriptor.PropPosition.Y;
            cachedRotation = (Vector3Xml)(targetDescriptor.PropRotation + new Vector3(0, rotation + 90));

            targetDescriptor.m_cachedRotations.Add(cachedRotation);
            targetDescriptor.m_cachedPositions.Add(cachedPosition);
        }

        internal void CalculateGroupData(ushort segmentID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays)
        {
            ref WriteOnNetGroupXml itemGroup = ref Data.m_boardsContainers[segmentID];
            if (itemGroup == null || !itemGroup.HasAnyBoard())
            {
                return;
            }
            for (var i = 0; i < itemGroup.BoardsData.Length; i++)
            {
                var targetDescriptor = itemGroup.BoardsData[i];
                if (targetDescriptor?.SimpleProp == null)
                {
                    continue;
                }
                WEDynamicTextRenderingRules.GetColorForRule(segmentID, i, 0, targetDescriptor, out bool rendered);
                if (rendered)
                {
                    int deltaVertexCount = 0;
                    int deltaTriangleCount = 0;
                    int deltaObjectCount = 0;
                    PropInstance.CalculateGroupData(targetDescriptor.SimpleCachedProp, layer, ref deltaVertexCount, ref deltaTriangleCount, ref deltaObjectCount, ref vertexArrays);

                    int multiplier = 1;

                    if (targetDescriptor.SegmentPositionRepeating)
                    {
                        multiplier = targetDescriptor.SegmentPositionRepeatCount;
                    }

                    vertexCount += multiplier * deltaVertexCount;
                    triangleCount += multiplier * deltaTriangleCount;
                    objectCount += multiplier * deltaTriangleCount;
                }
            }
        }

        internal void PopulateGroupData(ushort segmentID, int layer, ref int vertexIndex, ref int triangleIndex, ref Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
        {
            ref WriteOnNetGroupXml itemGroup = ref Data.m_boardsContainers[segmentID];
            if (itemGroup == null || !itemGroup.HasAnyBoard())
            {
                return;
            }
            for (var i = 0; i < itemGroup.BoardsData.Length; i++)
            {
                var targetDescriptor = itemGroup.BoardsData[i];
                if (targetDescriptor is null)
                {
                    continue;
                }

                if (targetDescriptor?.SimpleProp is null)
                {
                    continue;
                }
                if (!(targetDescriptor.m_cachedRotations is null) && !(targetDescriptor.m_cachedPositions is null))
                {
                    if (!(targetDescriptor.SimpleCachedProp is null))
                    {
                        for (int k = 0; k < targetDescriptor.m_cachedPositions.Count; k++)
                        {
                            WEDynamicTextRenderingRules.PropInstancePopulateGroupData(targetDescriptor.SimpleCachedProp, layer, new InstanceID { NetSegment = segmentID }, targetDescriptor.m_cachedPositions[k], targetDescriptor.Scale, targetDescriptor.m_cachedRotations[k], ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                        }
                    }
                }
            }
        }


        private void RenderSign(RenderManager.CameraInfo cameraInfo, ushort segmentId, int boardIdx, ref OnNetInstanceCacheContainerXml targetDescriptor, PropInfo cachedProp)
        {
            if (targetDescriptor.m_cachedPositions is null || targetDescriptor.m_cachedRotations is null)
            {
                return;
            }
            bool hasFixedCamera = false;
            for (int i = 0; i < targetDescriptor.m_cachedPositions.Count; i++)
            {
                var position = targetDescriptor.m_cachedPositions[i];
                var rotation = targetDescriptor.m_cachedRotations[i];

                var propname = targetDescriptor.m_simplePropName;
                if (propname is null)
                {
                    return;
                }

                Color parentColor = WEDynamicTextRenderingRules.RenderPropMesh(cachedProp, cameraInfo, segmentId, boardIdx, 0, 0xFFFFFFF, 0, position, Vector4.zero, rotation, targetDescriptor.PropScale, targetDescriptor, out Matrix4x4 propMatrix, out bool rendered, new InstanceID { NetNode = segmentId });

                if (rendered)
                {

                    for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
                    {
                        if (cameraInfo.CheckRenderDistance(position, WETextRenderer.RENDER_DISTANCE_FACTOR * targetDescriptor.TextDescriptors[j].TextLineHeight * (targetDescriptor.TextDescriptors[j].IlluminationConfig.IlluminationType == MaterialType.OPAQUE ? 1 : 3)))
                        {
                            bool currentTextSelected = !hasFixedCamera && WTSOnNetLiteUI.LockSelection && WTSOnNetLiteUI.Instance.IsOnTextEditor && i == WTSOnNetLiteUI.LockSelectionInstanceNum && j == WTSOnNetLiteUI.LockSelectionTextIdx && WTSOnNetLiteUI.Instance.Visible && (WTSOnNetLiteUI.Instance.CurrentSegmentId == segmentId) && WTSOnNetLiteUI.Instance.ListSel == boardIdx && !ModInstance.Controller.RoadSegmentToolInstance.enabled;
                            var textPos = WETextRenderer.RenderTextMesh(null, segmentId, boardIdx, i, ref parentColor, targetDescriptor, targetDescriptor.TextDescriptors[j], ref propMatrix, cachedProp, 0, 0, currentTextSelected && WTSOnNetLiteUI.Instance.IsOnTextEditorSizeView, ref NetManager.instance.m_drawCallData.m_batchedCalls);
                            if (currentTextSelected && textPos != default)
                            {
                                ToolsModifierControl.cameraController.m_targetPosition.x = textPos.x;
                                ToolsModifierControl.cameraController.m_targetPosition.z = textPos.z;
                                targetHeight = textPos.y;
                                lastFrameOverriden = SimulationManager.instance.m_currentTickIndex;
                                hasFixedCamera = true;
                            }
                        }
                    }

                    if (WTSOnNetLiteUI.LockSelection && (!WTSOnNetLiteUI.Instance.IsOnTextEditor || WTSOnNetLiteUI.LockSelectionTextIdx < 0 || !hasFixedCamera) && i == WTSOnNetLiteUI.LockSelectionInstanceNum && WTSOnNetLiteUI.Instance.Visible && (WTSOnNetLiteUI.Instance.CurrentSegmentId == segmentId) && WTSOnNetLiteUI.Instance.ListSel == boardIdx && !ModInstance.Controller.RoadSegmentToolInstance.enabled)
                    {
                        ToolsModifierControl.cameraController.m_targetPosition.x = position.X;
                        ToolsModifierControl.cameraController.m_targetPosition.z = position.Z;
                        targetHeight = position.Y + cachedProp.m_mesh.bounds.center.y * targetDescriptor.PropScale.y;
                        lastFrameOverriden = SimulationManager.instance.m_currentTickIndex;
                    }
                }
            }
        }
        private static float targetHeight;
        private uint lastFrameOverriden;

        public static void AfterUpdateTransformOverride(CameraController __instance)
        {
            if (LoadingManager.instance.m_loadingComplete && SimulationManager.instance.m_currentTickIndex - ModInstance.Controller.OnNetPropsSingleton.lastFrameOverriden > 24)
            {
                return;
            }
            __instance.m_minDistance = 1;

            Vector3 vector = __instance.transform.position;
            vector.y = targetHeight + (Mathf.Sin(__instance.m_currentAngle.y * Mathf.Deg2Rad) * __instance.m_targetSize);
            __instance.transform.position = vector;
        }
    }
}
