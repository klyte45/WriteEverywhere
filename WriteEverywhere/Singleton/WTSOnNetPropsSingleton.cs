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
                WTSDynamicTextRenderingRules.GetColorForRule(segmentID, i, 0, null, targetDescriptor, out bool rendered);
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

        internal void PopulateGroupData(ushort segmentID, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
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
                            WTSDynamicTextRenderingRules.PropInstancePopulateGroupData(targetDescriptor.SimpleCachedProp, layer, new InstanceID { NetSegment = segmentID }, targetDescriptor.m_cachedPositions[k], targetDescriptor.Scale, targetDescriptor.m_cachedRotations[k], ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
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
            for (int i = 0; i < targetDescriptor.m_cachedPositions.Count; i++)
            {
                var position = targetDescriptor.m_cachedPositions[i];
                var rotation = targetDescriptor.m_cachedRotations[i];

                var propname = targetDescriptor.m_simplePropName;
                if (propname is null)
                {
                    return;
                }

                Color parentColor = WTSDynamicTextRenderingRules.RenderPropMesh(cachedProp, cameraInfo, segmentId, boardIdx, 0, 0xFFFFFFF, 0, position, Vector4.zero, rotation, targetDescriptor.PropScale, null, targetDescriptor, out Matrix4x4 propMatrix, out bool rendered, new InstanceID { NetNode = segmentId });



                for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
                {
                    if (cameraInfo.CheckRenderDistance(position, WTSDynamicTextRenderingRules.RENDER_DISTANCE_FACTOR * targetDescriptor.TextDescriptors[j].TextLineHeight * (targetDescriptor.TextDescriptors[j].IlluminationConfig.IlluminationType == FontStashSharp.MaterialType.OPAQUE ? 1 : 3)))
                    {
                        MaterialPropertyBlock properties = PropManager.instance.m_materialBlock;
                        properties.Clear();
                        WTSDynamicTextRenderingRules.RenderTextMesh(segmentId, boardIdx, i, targetDescriptor, propMatrix, targetDescriptor.Scale, ref targetDescriptor.TextDescriptors[j], properties, 0, 0, parentColor, cachedProp, ref NetManager.instance.m_drawCallData.m_batchedCalls, targetDescriptor.FontName);
                    }


                }

                if (WTSOnNetLiteUI.LockSelection && i == WTSOnNetLiteUI.LockSelectionInstanceNum && WTSOnNetLiteUI.Instance.Visible && (WTSOnNetLiteUI.Instance.CurrentSegmentId == segmentId) && WTSOnNetLiteUI.Instance.ListSel == boardIdx && !ModInstance.Controller.RoadSegmentToolInstance.enabled)
                {
                    ToolsModifierControl.cameraController.m_targetPosition = position;
                }
            }
        }
    }
}
