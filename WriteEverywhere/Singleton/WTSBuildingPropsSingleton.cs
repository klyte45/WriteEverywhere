
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Kwytto.LiteUI;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Font;
using WriteEverywhere.Layout;
using WriteEverywhere.Rendering;
using WriteEverywhere.TransportLines;
using WriteEverywhere.UI;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;
using static Kwytto.Utils.StopSearchUtils;

namespace WriteEverywhere.Singleton
{
    public class WTSBuildingPropsSingleton : MonoBehaviour
    {
        public DynamicSpriteFont DrawFont => FontServer.instance[Data.DefaultFont] ?? FontServer.instance[WEMainController.DEFAULT_FONT_KEY];
        private readonly Dictionary<string, StopPointDescriptorLanes[]> m_buildingStopsDescriptor = new Dictionary<string, StopPointDescriptorLanes[]>();
        public WTSBuildingData Data => WTSBuildingData.Instance;
        public SimpleXmlDictionary<string, WriteOnBuildingXml> CityDescriptors => Data.CityDescriptors;
        public SimpleXmlDictionary<string, WriteOnBuildingXml> GlobalDescriptors => Data.GlobalDescriptors;
        public SimpleXmlDictionary<string, WriteOnBuildingXml> AssetsDescriptors => Data.AssetsDescriptors;

        internal StopInformation[][][] m_platformToLine = new StopInformation[BuildingManager.MAX_BUILDING_COUNT][][];
        internal StopInformation[] m_stopInformation = new StopInformation[NetManager.MAX_NODE_COUNT];
        private ulong m_lastUpdateLines = SimulationManager.instance.m_currentTickIndex;
        private readonly ulong[] m_buildingLastUpdateLines = new ulong[BuildingManager.MAX_BUILDING_COUNT];

        public void ResetLines()
        {
            m_platformToLine = new StopInformation[BuildingManager.MAX_BUILDING_COUNT][][];
            m_stopInformation = new StopInformation[NetManager.MAX_NODE_COUNT];
        }

        #region Initialize
        public void Awake() => LoadAllBuildingConfigurations();

        public void Start()
        {
            ModInstance.Controller.EventOnLineUpdated += OnLineUpdated;
            ModInstance.Controller.EventNodeChanged += OnNodeChanged;
            TransportManager.instance.eventLineColorChanged += OnLineUpdated;
            ModInstance.Controller.EventOnLineBuildingUpdated += OnBuildingLineChanged;
        }

        private void OnLineUpdated(ushort obj) => m_lastUpdateLines = SimulationManager.instance.m_currentTickIndex;

        private IEnumerator OnNodeChanged(ushort id)
        {
            yield return null;
            ushort buildingId = NetNode.FindOwnerBuilding(id, 56f);
            if (buildingId > 0 && Data.BuildingCachedPositionsData.ContainsKey(buildingId))
            {
                m_buildingLastUpdateLines[buildingId] = 0;
            }
        }


        private void OnBuildingLineChanged(ushort id)
        {
            ushort parentId = id;
            int count = 16;
            while (parentId > 0 && count > 0)
            {
                m_buildingLastUpdateLines[parentId] = 0;

                parentId = BuildingManager.instance.m_buildings.m_buffer[parentId].m_parentBuilding;
                count--;
            }
            if (count == 0)
            {
                LogUtils.DoErrorLog($"INFINITELOOP! {id} {parentId} ");
            }
        }
        #endregion      

        public void AfterRenderInstanceImpl(RenderManager.CameraInfo cameraInfo, ushort sourceBuildingId, int layerMask, ref RenderManager.Instance renderInstance)
        {
            ref Building[] bArray = ref BuildingManager.instance.m_buildings.m_buffer;
            ushort parentBuildingId = sourceBuildingId;
            ushort subBuildingIdx = 0;
            while (bArray[parentBuildingId].m_parentBuilding != 0)
            {
                parentBuildingId = bArray[parentBuildingId].m_parentBuilding;
                subBuildingIdx++;
            }

            ref Building data = ref bArray[parentBuildingId];


            string refName = GetReferenceModelName(ref data);
            var targetDescriptor = PrepareTargetDescriptor(ref renderInstance.m_dataMatrix1, subBuildingIdx, data.Info, refName);

            if (subBuildingIdx == 0)
            {
                UpdateLinesBuilding(parentBuildingId, refName, targetDescriptor, ref data, ref renderInstance.m_dataMatrix1);
            }

            if ((targetDescriptor?.PropInstances?.Length ?? 0) == 0)
            {
                return;
            }

            for (int i = 0; i < targetDescriptor.PropInstances.Length; i++)
            {
                if (targetDescriptor.PropInstances[i].SubBuildingPivotReference == subBuildingIdx)
                {
                    RenderDescriptor(cameraInfo, parentBuildingId, data.m_position, data.m_angle, data.Info, (int)data.m_flags, (int)data.m_flags2, renderInstance.m_dataMatrix1, layerMask, ref targetDescriptor, i);
                }
            }
        }

        internal WriteOnBuildingXml PrepareTargetDescriptor(ref Matrix4x4 refMatrix, ushort subBuildingIdx, BuildingInfo info, string refName)
        {
            if (BuildingLiteUI.Instance.Visible && refName != null && (SceneUtils.IsAssetEditor || BuildingLiteUI.Instance.CurrentEditingInfo?.name == refName))
            {
                if (!m_buildingStopsDescriptor.ContainsKey(refName))
                {
                    StartCoroutine(DoMapStopPoints(refName, info, 1f));//TODO: Thresold salvo em algum lugar
                }
                if (!(m_buildingStopsDescriptor[refName] is null) && subBuildingIdx == 0)
                {
                    float angle = SimulationManager.instance.m_currentTickIndex;
                    var lowestPoint = Mathf.Min(m_buildingStopsDescriptor[refName].Select(x => x.platformLine.Position(0.5f).y).ToArray());
                    for (int i = 0; i < m_buildingStopsDescriptor[refName].Length; i++)
                    {
                        var color = m_colorOrder[i % m_colorOrder.Length];
                        var position = m_buildingStopsDescriptor[refName][i].platformLine.Position(0.5f);
                        var circleSize = m_buildingStopsDescriptor[refName][i].width * .333333333f;
                        m_onOverlayRenderQueue.Add(new DrawInformation
                        {
                            position = refMatrix.MultiplyPoint(position),
                            circleSize = circleSize,
                            color = m_colorOrder[i % m_colorOrder.Length],
                            layer = info.m_prefabDataLayer,
                            text = $"P{i + 1}"
                        });


                        var bri = FontServer.instance[WEMainController.DEFAULT_FONT_KEY].DrawString(ModInstance.Controller, string.Format(position.y < 0 ? "[{0}]" : "{0}", i + 1), default, FontServer.instance.ScaleEffective);
                        if (bri != null)
                        {
                            overlayBlock.Clear();
                            overlayBlock.SetColor(WETextRenderer.SHADER_PROP_BACK_COLOR, color);
                            overlayBlock.SetColor(WETextRenderer.SHADER_PROP_COLOR, color);
                            overlayBlock.SetVector(WETextRenderer.SHADER_PROP_SURF_PROPERTIES, new Vector4(1, 0, .3F));
                            WETextRenderer.RenderBri(bri, ref BuildingManager.instance.m_drawCallData.m_batchedCalls, refMatrix * Matrix4x4.TRS(position + new Vector3(0, position.y < 0 ? info.m_mesh.bounds.max.y + 5 - lowestPoint : 3, 0), Quaternion.Euler(new Vector3(0, angle, 0)), new Vector3(.05f * circleSize, .05f * circleSize, .05f * circleSize))
                                , info.m_prefabDataLayer, overlayBlock);
                        }
                    }
                }
            }
            GetTargetDescriptor(refName, out _, out WriteOnBuildingXml targetDescriptor);
            return targetDescriptor;
        }

        private IEnumerator DoMapStopPoints(string refName, BuildingInfo info, float thresold)
        {
            m_buildingStopsDescriptor[refName] = null;
            var result = new Wrapper<StopPointDescriptorLanes[]>();
            yield return null;
            yield return MapStopPoints(info, thresold, result);
            m_buildingStopsDescriptor[refName] = result.Value;
        }

        private static readonly MaterialPropertyBlock overlayBlock = new MaterialPropertyBlock();


        public static void AfterEndOverlayImpl(RenderManager.CameraInfo cameraInfo)
        {
            if (BuildingLiteUI.Instance.Visible)
            {
                foreach (var tuple in ModInstance.Controller.BuildingPropsSingleton.m_onOverlayRenderQueue)
                {
                    Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo,
                       tuple.color,
                       tuple.position,
                       tuple.circleSize * 2,
                    -1, 1280f, false, true);
                }
                ModInstance.Controller.BuildingPropsSingleton.m_onOverlayRenderQueue.Clear();
            }
        }
        internal static void GetTargetDescriptor(string building, out ConfigurationSource source, out WriteOnBuildingXml target)
        {
            if (building == null)
            {
                source = ConfigurationSource.NONE;
                target = null;
                return;
            }

            if (WTSBuildingData.Instance.CityDescriptors.ContainsKey(building))
            {
                source = ConfigurationSource.CITY;
                target = WTSBuildingData.Instance.CityDescriptors[building];
                return;
            }

            if (WTSBuildingData.Instance.GlobalDescriptors.ContainsKey(building))
            {
                source = ConfigurationSource.GLOBAL;
                target = WTSBuildingData.Instance.GlobalDescriptors[building];
                return;
            }

            if (WTSBuildingData.Instance.AssetsDescriptors.ContainsKey(building))
            {
                source = ConfigurationSource.ASSET;
                target = WTSBuildingData.Instance.AssetsDescriptors[building];
                return;
            }

            source = ConfigurationSource.NONE;
            target = null;

        }

        private readonly List<DrawInformation> m_onOverlayRenderQueue = new List<DrawInformation>();

        private struct DrawInformation
        {
            public Vector3 position;
            public float circleSize;
            public Color color;
            public string text;
            public int layer;
        }

        internal static readonly Color[] m_colorOrder = new Color[]
        {
            Color.red,
            Color.Lerp(Color.red, Color.yellow,0.5f),
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.Lerp(Color.blue, Color.magenta,0.5f),
            Color.magenta,
            Color.white,
            Color.gray,
            Color.Lerp( Color.red,                                    Color.black,0.5f),
            Color.Lerp( Color.Lerp(Color.red, Color.yellow,0.5f),     Color.black,0.5f),
            Color.Lerp( Color.yellow,                                 Color.black,0.5f),
            Color.Lerp( Color.green,                                  Color.black,0.5f),
            Color.Lerp( Color.cyan,                                   Color.black,0.5f),
            Color.Lerp( Color.blue,                                   Color.black,0.5f),
            Color.Lerp( Color.Lerp(Color.blue, Color.magenta,0.5f),   Color.black,0.5f),
            Color.Lerp( Color.magenta,                                Color.black,0.5f),
            Color.Lerp( Color.white,                                  Color.black,0.25f),
            Color.Lerp( Color.white,                                  Color.black,0.75f)
        };

        internal void RenderDescriptor(RenderManager.CameraInfo cameraInfo, ushort buildingID,
            Vector3 position,
            float angleBuilding,
            BuildingInfo info,
            int flags1,
            int flags2,
            Matrix4x4 refMatrix,
            int layerMask,
            ref WriteOnBuildingXml parentDescriptor, int idx)
        {
            var targetDescriptor = parentDescriptor.PropInstances[idx];
            if (targetDescriptor?.SimpleProp is null)
            {
                return;
            }
            var targetProp = targetDescriptor.SimpleProp;


            if (!Data.BuildingCachedPositionsData.ContainsKey(buildingID) || Data.BuildingCachedPositionsData[buildingID].Length != parentDescriptor.PropInstances.Length)
            {
                Data.BuildingCachedPositionsData[buildingID] = new PropLayoutCachedBuildingData[parentDescriptor.PropInstances.Length];
            }
            ref PropLayoutCachedBuildingData item = ref Data.BuildingCachedPositionsData[buildingID][idx];
            if (SimulationManager.instance.m_currentTickIndex % 10 == 0 && (item.m_buildingPositionWhenGenerated != position || item.m_buildingRotationWhenGenerated != angleBuilding || (BuildingLiteUI.Instance.Visible && (SceneUtils.IsAssetEditor || BuildingLiteUI.Instance.CurrentInfo == info))))
            {
                var isFromPositionChanged = item.m_buildingPositionWhenGenerated != position;
                item.m_buildingPositionWhenGenerated = position;
                if (SceneUtils.IsAssetEditor)
                {
                    item.m_cachedMatrix = refMatrix;
                }
                else
                {
                    if (targetDescriptor.SubBuildingPivotReference >= 0 && targetDescriptor.SubBuildingPivotReference < info.m_subBuildings.Length)
                    {
                        BuildingManager inst = BuildingManager.instance;
                        uint targetBuildingId = buildingID;
                        for (int i = 0; i <= targetDescriptor.SubBuildingPivotReference; i++)
                        {
                            targetBuildingId = inst.m_buildings.m_buffer[targetBuildingId].m_subBuilding;
                        }
                        item.m_cachedMatrix = RenderManager.instance.RequireInstance(targetBuildingId, 1u, out uint num)
                            ? RenderManager.instance.m_instances[num].m_dataMatrix1
                            : refMatrix;
                    }
                    else
                    {
                        item.m_cachedMatrix = refMatrix;
                    }
                }
                item.m_buildingRotationWhenGenerated = angleBuilding;
                item.m_cachedPosition = item.m_cachedMatrix.MultiplyPoint(targetDescriptor.PropPosition);
                item.m_cachedOriginalPosition = targetDescriptor.PropPosition;
                item.m_cachedRotation = targetDescriptor.PropRotation;
                item.m_cachedScale = targetDescriptor.PropScale;
                item.m_cachedArrayRepeatTimes = targetDescriptor.ArrayRepeatTimes;
                item.m_cachedArrayItemPace = targetDescriptor.ArrayRepeat;
                if (isFromPositionChanged)
                {
                    ModInstance.Instance.RequireRunCoroutine("targetDescriptor.OnChangeMatrixData()", targetDescriptor.OnChangeMatrixData());
                }
            }
            Vector3 targetPostion = item.m_cachedPosition;
            var hasFixedCamera = false;
            for (int i = 0; i < targetDescriptor.ArrayRepeatTimes; i++)
            {
                if (i > 0)
                {
                    targetPostion = item.m_cachedMatrix.MultiplyPoint(targetDescriptor.PropPosition + (i * (Vector3)targetDescriptor.ArrayRepeat));
                }
                RenderSign(parentDescriptor, angleBuilding, info, flags1, flags2, cameraInfo, buildingID, idx, targetPostion, item.m_cachedRotation, layerMask, targetDescriptor, targetProp, i, ref hasFixedCamera);
            }
        }

        private static float targetHeight;
        private uint lastFrameOverriden;
        private void RenderSign(WriteOnBuildingXml parentDescriptor, float angleBuilding, BuildingInfo info, int flags1, int flags2, RenderManager.CameraInfo cameraInfo,
            ushort buildingId, int boardIdx, Vector3 position, Vector3 rotation, int layerMask, WriteOnBuildingPropXml targetDescriptor, PropInfo cachedProp, int arrayIdx, ref bool hasFixedCamera)
        {
            var propname = targetDescriptor.m_simplePropName;
            if (propname is null)
            {
                return;
            }
            Color parentColor = WEDynamicTextRenderingRules.RenderPropMesh(cachedProp, cameraInfo, buildingId, boardIdx, 0, layerMask, angleBuilding, position, Vector4.zero, rotation, targetDescriptor.PropScale, targetDescriptor, out Matrix4x4 propMatrix, out bool rendered, new InstanceID { Building = buildingId });

            if (rendered)
            {
                for (int j = 0; j < targetDescriptor.TextDescriptors.Length; j++)
                {
                    if (cameraInfo.CheckRenderDistance(position, WETextRenderer.RENDER_DISTANCE_FACTOR * targetDescriptor.TextDescriptors[j].TextLineHeight * targetDescriptor.PropScale.magnitude * (targetDescriptor.TextDescriptors[j].IlluminationConfig.IlluminationType == MaterialType.OPAQUE ? 1 : 2)))
                    {
                        bool currentTextSelected = !hasFixedCamera
                            && BuildingLiteUI.Instance.Visible
                            && BuildingLiteUI.LockSelection
                            && BuildingLiteUI.Instance.IsOnTextEditor
                            && (SceneUtils.IsAssetEditor || BuildingLiteUI.Instance.CurrentGrabbedId == buildingId)
                            && BuildingUIBasicTab.CurrentFocusInstance == arrayIdx
                            && BuildingLiteUI.Instance.CurrentTextSel == j
                            && BuildingLiteUI.Instance.CurrentPropSel == boardIdx
                            && !ModInstance.Controller.BuildingToolInstance.enabled;
                        var textPos = WETextRenderer.RenderTextMesh(parentDescriptor, buildingId, boardIdx, 0, ref parentColor, targetDescriptor, targetDescriptor.TextDescriptors[j], ref propMatrix, info, flags1, flags2, currentTextSelected && BuildingLiteUI.Instance.IsOnTextDimensionsView, ref BuildingManager.instance.m_drawCallData.m_batchedCalls);

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

                if (BuildingLiteUI.LockSelection &&
                    (!BuildingLiteUI.Instance.IsOnTextEditor || !hasFixedCamera || BuildingLiteUI.Instance.CurrentTextSel < 0)
                    && BuildingLiteUI.Instance.Visible
                    && (SceneUtils.IsAssetEditor || BuildingLiteUI.Instance.CurrentGrabbedId == buildingId)
                    && BuildingUIBasicTab.CurrentFocusInstance == arrayIdx
                    && BuildingLiteUI.Instance.CurrentPropSel == boardIdx
                    && !ModInstance.Controller.BuildingToolInstance.enabled)
                {
                    ToolsModifierControl.cameraController.m_targetPosition.x = position.x;
                    ToolsModifierControl.cameraController.m_targetPosition.z = position.z;
                    targetHeight = position.y + (cachedProp.m_mesh.bounds.center.y * targetDescriptor.PropScale.y);
                    lastFrameOverriden = SimulationManager.instance.m_currentTickIndex;
                }
            }
        }

        internal void UpdateLinesBuilding(ushort buildingID, ref Building data, ref Matrix4x4 refMatrix)
        {
            if (m_platformToLine[buildingID] == null || (m_buildingLastUpdateLines[buildingID] != m_lastUpdateLines && m_platformToLine[buildingID].Length > 0))
            {
                var refName = GetReferenceModelName(ref data);
                UpdateLinesBuilding(buildingID, refName, null, ref data, ref refMatrix);
            }
        }
        private void UpdateLinesBuilding(ushort buildingID, string refName, WriteOnBuildingXml propDescriptor, ref Building data, ref Matrix4x4 refMatrix)
        {
            if ((data.Info.m_buildingAI is TransportStationAI || data.Info.m_buildingAI is OutsideConnectionAI) && (m_platformToLine[buildingID] == null || (m_buildingLastUpdateLines[buildingID] != m_lastUpdateLines && m_platformToLine[buildingID].Length > 0)))
            {
                if (!m_buildingStopsDescriptor.ContainsKey(refName))
                {
                    m_buildingStopsDescriptor[refName] = null;
                    StartCoroutine(DoMapStopPoints(refName, data.Info, propDescriptor?.StopMappingThresold ?? 1f));
                }
                if (m_buildingStopsDescriptor[refName] == null)
                {
                    return;
                }
                NetManager nmInstance = NetManager.instance;
                LogUtils.DoLog($"--------------- UpdateLinesBuilding {buildingID}");

                m_platformToLine[buildingID]?.ForEach(x => x?.ForEach(y => m_stopInformation[y.m_stopId] = default));
                m_platformToLine[buildingID] = null;
                var platforms = m_buildingStopsDescriptor[refName].Select((v, i) => new { Key = i, Value = v }).ToDictionary(o => o.Key, o => o.Value);

                if (platforms.Count == 0)
                {
                    m_platformToLine[buildingID] = new StopInformation[0][];
                }
                else
                {

                    var boundaries = new List<Quad2>();
                    ushort subBuilding = buildingID;
                    var allnodes = new List<ushort>();
                    while (subBuilding > 0)
                    {
                        boundaries.Add(GetBounds(ref BuildingManager.instance.m_buildings.m_buffer[subBuilding]));
                        ushort node = BuildingManager.instance.m_buildings.m_buffer[subBuilding].m_netNode;
                        while (node > 0)
                        {
                            allnodes.Add(node);
                            node = nmInstance.m_nodes.m_buffer[node].m_nextBuildingNode;
                        }
                        subBuilding = BuildingManager.instance.m_buildings.m_buffer[subBuilding].m_subBuilding;
                    }
                    foreach (ushort node in allnodes)
                    {
                        if (!boundaries.Any(x => x.Intersect(nmInstance.m_nodes.m_buffer[node].m_position)))
                        {
                            for (int segIdx = 0; segIdx < 8; segIdx++)
                            {
                                ushort segmentId = nmInstance.m_nodes.m_buffer[node].GetSegment(segIdx);
                                if (segmentId != 0 && allnodes.Contains(nmInstance.m_segments.m_buffer[segmentId].GetOtherNode(node)))
                                {


                                    boundaries.Add(GetBounds(
                                        nmInstance.m_nodes.m_buffer[nmInstance.m_segments.m_buffer[segmentId].m_startNode].m_position,
                                        nmInstance.m_nodes.m_buffer[nmInstance.m_segments.m_buffer[segmentId].m_endNode].m_position,
                                        nmInstance.m_segments.m_buffer[segmentId].Info.m_halfWidth)
                                        );
                                }
                            }
                        }
                    }
                    List<ushort> nearStops = FindNearStops(data.m_position, ItemClass.Service.PublicTransport, ItemClass.Service.PublicTransport, VehicleInfo.VehicleType.None, true, 400f, out _, out _, boundaries);

                    m_platformToLine[buildingID] = new StopInformation[m_buildingStopsDescriptor[refName].Length][];
                    if (nearStops.Count > 0)
                    {

                        if (ModInstance.DebugMode)
                        {
                            LogUtils.DoLog($"[{InstanceManager.instance.GetName(new InstanceID { Building = buildingID })}] nearStops = [\n\t\t{string.Join(",\n\t\t", nearStops.Select(x => $"[{x} => {nmInstance.m_nodes.m_buffer[x].m_position} (TL {nmInstance.m_nodes.m_buffer[x].m_transportLine} => [{TransportManager.instance.m_lines.m_buffer[nmInstance.m_nodes.m_buffer[x].m_transportLine].Info.m_transportType}-{TransportManager.instance.m_lines.m_buffer[nmInstance.m_nodes.m_buffer[x].m_transportLine].m_lineNumber}] {TransportManager.instance.GetLineName(nmInstance.m_nodes.m_buffer[x].m_transportLine)} )]").ToArray())}\n\t] ");
                        }
                        string buildingName = refName;
                        for (int i = 0; i < m_buildingStopsDescriptor[buildingName].Length; i++)
                        {
                            Matrix4x4 inverseMatrix = refMatrix.inverse;
                            float maxDist = m_buildingStopsDescriptor[buildingName][i].width;
                            float maxHeightDiff = m_buildingStopsDescriptor[buildingName][i].width;
                            if (ModInstance.DebugMode)
                            {
                                LogUtils.DoLog($"platLine ({i}) = {m_buildingStopsDescriptor[buildingName][i].platformLine.a} {m_buildingStopsDescriptor[buildingName][i].platformLine.b} {m_buildingStopsDescriptor[buildingName][i].platformLine.c} {m_buildingStopsDescriptor[buildingName][i].platformLine.d}");
                                LogUtils.DoLog($"maxDist ({i}) = {maxDist}");
                                LogUtils.DoLog($"maxHeightDiff ({i}) = {maxHeightDiff}");
                                LogUtils.DoLog($"refMatrix ({i}) = {refMatrix}");
                                LogUtils.DoLog($"inverseMatrix ({i}) = {inverseMatrix}");
                            }
                            if (inverseMatrix == default)
                            {
                                LogUtils.DoWarnLog($"--------------- end UpdateLinesBuilding - inverseMatrix is zero (ID: {buildingID}) \n{Environment.StackTrace}");
                                return;
                            }
                            float angleBuilding = data.m_angle * Mathf.Rad2Deg;
                            m_platformToLine[buildingID][i] = nearStops
                                .Where(stopId =>
                                {
                                    Vector3 relPos = inverseMatrix.MultiplyPoint(nmInstance.m_nodes.m_buffer[stopId].m_position);
                                    float dist = m_buildingStopsDescriptor[buildingName][i].platformLine.DistanceSqr(relPos, out _);
                                    float diffY = Mathf.Abs(relPos.y - m_buildingStopsDescriptor[buildingName][i].platformLine.Position(0.5f).y);
                                    if (ModInstance.DebugMode)
                                    {
                                        LogUtils.DoLog($"stop {stopId} => relPos = {relPos}; dist = {dist}; diffY = {diffY}");
                                    }
                                    return dist < maxDist && diffY < maxHeightDiff;
                                })
                                .Select(stopId =>
                                {
                                    float anglePlat = (m_buildingStopsDescriptor[buildingName][i].directionPath.GetAngleXZ() + 360 + angleBuilding) % 360;
                                    return UpdateStopInformation(stopId, buildingName.TrimToNull() ?? $"[STOP {stopId}]", i, anglePlat, nmInstance);
                                }).Where(x => x.m_lineId != 0).ToArray();
                            if (ModInstance.DebugMode)
                            {
                                LogUtils.DoLog($"NearLines ({i}) = [{string.Join(",", m_platformToLine[buildingID][i].Select(x => x.ToString()).ToArray())}]");
                            }
                        }
                    }
                }
                if (m_platformToLine[buildingID]?.Length > 0)
                {
                    m_platformToLine[buildingID]?.ForEach(x => x?.ForEach(y => m_stopInformation[y.m_stopId] = y));
                }

                m_buildingLastUpdateLines[buildingID] = m_lastUpdateLines;
                LogUtils.DoLog($"--------------- end UpdateLinesBuilding {buildingID}");
            }
        }

        private StopInformation UpdateStopInformation(ushort stopId, string buildingName = null, int i = 0, float anglePlat = 0, NetManager nmInstance = null)
        {
            if (nmInstance is null)
            {
                nmInstance = NetManager.instance;
            }
            var stopLine = ModInstance.Controller.ConnectorTLM.GetStopLine(stopId);

            var result = new StopInformation
            {
                m_lineId = stopLine.lineId,
                m_regionalLine = stopLine.regional,
                m_stopId = stopId,
                m_destinationId = FindDestinationStop(stopId)
            };

            ref NetSegment[] segBuffer = ref nmInstance.m_segments.m_buffer;

            ushort segmentPrev = nmInstance.m_nodes.m_buffer[stopId].GetSegment(0);
            ushort segmentNext = nmInstance.m_nodes.m_buffer[stopId].GetSegment(1);
            if (segmentPrev != 0 && segmentNext != 0)
            {
                ushort nextStop = TransportLine.GetNextStop(stopId);
                if (segBuffer[segmentPrev].GetOtherNode(stopId) == nextStop)
                {
                    ushort next = segmentPrev;
                    segmentPrev = segmentNext;
                    segmentNext = next;
                }
                float angleDirPrev, angleDirNext;
                PathUnit.Position pathPrev;
                PathUnit.Position pathnext;
                if (segBuffer[segmentPrev].m_startNode == stopId)
                {
                    pathPrev = PathManager.instance.m_pathUnits.m_buffer[nmInstance.m_segments.m_buffer[segmentPrev].m_path].m_position01;
                }
                else
                {
                    PathManager.instance.m_pathUnits.m_buffer[nmInstance.m_segments.m_buffer[segmentPrev].m_path].GetLast2Positions(out pathPrev, out _);
                }
                if (segBuffer[segmentNext].m_startNode == stopId)
                {
                    pathnext = PathManager.instance.m_pathUnits.m_buffer[nmInstance.m_segments.m_buffer[segmentNext].m_path].m_position01;
                }
                else
                {
                    PathManager.instance.m_pathUnits.m_buffer[nmInstance.m_segments.m_buffer[segmentPrev].m_path].GetLast2Positions(out pathnext, out _);
                }
                angleDirPrev = ((segBuffer[pathPrev.m_segment].GetBezier().Position(pathPrev.m_offset / 255f) - nmInstance.m_nodes.m_buffer[stopId].m_position).GetAngleXZ() + 360) % 360;
                angleDirNext = ((segBuffer[pathnext.m_segment].GetBezier().Position(pathnext.m_offset / 255f) - nmInstance.m_nodes.m_buffer[stopId].m_position).GetAngleXZ() + 360) % 360;
                float diff = Mathf.Abs(angleDirPrev - angleDirNext);
                float diffPlat = Mathf.Abs(anglePlat - ((angleDirPrev + angleDirNext) / 2)) % 180;
                if (ModInstance.DebugMode)
                {
                    LogUtils.DoLog($"ANGLE COMPARISON: diff = {diff} | diffPlat = {diffPlat} | PLAT = {anglePlat} | DIR IN = {angleDirPrev} | DIR OUT = {angleDirNext} | ({buildingName} =>  P[{i}] | L = {stopLine} )");
                }

                switch (GetPathType(angleDirPrev, anglePlat, angleDirNext))
                {
                    case PathType.FORWARD:
                        result.m_nextStopId = nextStop;
                        result.m_previousStopId = TransportLine.GetPrevStop(stopId);
                        break;
                    case PathType.BACKWARD_FORWARD:
                        result.m_nextStopId = nextStop;
                        break;
                    case PathType.FORWARD_BACKWARD:
                        result.m_previousStopId = nextStop;
                        break;
                    case PathType.BACKWARD:
                        result.m_previousStopId = nextStop;
                        result.m_nextStopId = TransportLine.GetPrevStop(stopId);
                        break;
                }

                if (result.m_destinationId == 0)
                {
                    result.m_destinationId = nextStop;
                }
            }
            return result;
        }

        protected PathType GetPathType(float angleIn, float anglePass, float angleOut)
        {
            float Δin = (anglePass - angleIn + 720) % 360;
            float Δout = (angleOut - anglePass + 720) % 360;
            bool inFw = (Δin < 180);
            bool outFw = (Δout < 180);
            LogUtils.DoLog($"GetPathType [{angleIn}|{anglePass}|{angleOut}]  Δin = {Δin}, Δout ={Δout}, infw = {inFw}, outFw = {outFw}");
            return (PathType)((inFw ? 0 : 1) + (outFw ? 0 : 2));
        }

        protected enum PathType
        {
            FORWARD,
            BACKWARD_FORWARD,
            FORWARD_BACKWARD,
            BACKWARD,
        }


        protected Quad2 GetBounds(ref Building data)
        {
            int width = data.Width;
            int length = data.Length;
            var vector = new Vector2(Mathf.Cos(data.m_angle), Mathf.Sin(data.m_angle));
            var vector2 = new Vector2(vector.y, -vector.x);
            vector *= width * 4f;
            vector2 *= length * 4f;
            Vector2 a = VectorUtils.XZ(data.m_position);
            var quad = default(Quad2);
            quad.a = a - vector - vector2;
            quad.b = a + vector - vector2;
            quad.c = a + vector + vector2;
            quad.d = a - vector + vector2;
            return quad;
        }
        protected Quad2 GetBounds(Vector3 ref1, Vector3 ref2, float halfWidth)
        {
            Vector2 ref1v2 = VectorUtils.XZ(ref1);
            Vector2 ref2v2 = VectorUtils.XZ(ref2);
            float halfLength = (ref1v2 - ref2v2).magnitude / 2;
            Vector2 center = (ref1v2 + ref2v2) / 2;
            float angle = Vector2.Angle(ref1v2, ref2v2);


            var vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var vector2 = new Vector2(vector.y, -vector.x);
            vector *= halfWidth;
            vector2 *= halfLength;
            var quad = default(Quad2);
            quad.a = center - vector - vector2;
            quad.b = center + vector - vector2;
            quad.c = center + vector + vector2;
            quad.d = center - vector + vector2;
            return quad;
        }
        private ushort FindDestinationStop(ushort stopId) => WTSStopUtils.GetStopDestinationData(stopId).m_destinationId;

        private readonly TransportInfo.TransportType[] m_allowedTypesNextPreviousStations =
    {
            TransportInfo.TransportType.Metro,
            TransportInfo.TransportType.Monorail,
            TransportInfo.TransportType.Train,
            TransportInfo.TransportType.Ship,
        };

        public static string GetReferenceModelName(ref Building data)
        {
            string refName = data.Info.name;
            if (refName.Contains("_XANALOGX_"))
            {
                refName = refName.Substring(0, refName.LastIndexOf("_XANALOGX_"));
            }

            return refName;
        }


        private static string DefaultFilename { get; } = $"{WEMainController.m_defaultFileNameBuildingsXml}.xml";
        public bool IsReady => LoadingCoroutineLocal is null && LoadingCoroutineAssets is null;

        private Coroutine LoadingCoroutineAssets;
        private Coroutine LoadingCoroutineLocal;

        public Coroutine LoadAllBuildingConfigurations() => IsReady ? StartCoroutine(LoadAllBuildingConfigurations_Coroutine()) : null;

        public IEnumerator LoadAllBuildingConfigurations_Coroutine()
        {
            LogUtils.DoLog("LOADING BUILDING CONFIG START -----------------------------");
            LoadingCoroutineAssets = StartCoroutine(ReloadBuildingAssets_Coroutine());
            if (!SceneUtils.IsAssetEditor)
            {
                LoadingCoroutineLocal = StartCoroutine(ReloadBuildingShared_coroutine());
            }
            else
            {
                Data.GlobalDescriptors.Clear();
                Data.CityDescriptors.Clear();
            }
            yield return new WaitUntil(() => IsReady);
            Data.CleanCache();
            LogUtils.DoLog("LOADING BUILDING CONFIG END -----------------------------");
        }

        private IEnumerator ReloadBuildingShared_coroutine()
        {
            Data.GlobalDescriptors.Clear();
            var errorList = new List<string>();
            LogUtils.DoLog($"DefaultBuildingsConfigurationFolder = {WEMainController.DefaultBuildingsConfigurationFolder}");
            int counter = 0;
            foreach (string filename in Directory.GetFiles(WEMainController.DefaultBuildingsConfigurationFolder, "*.xml"))
            {
                try
                {
                    if (ModInstance.DebugMode)
                    {
                        LogUtils.DoLog($"Trying deserialize {filename}:\n{File.ReadAllText(filename)}");
                    }
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        LoadDescriptorsFromXmlCommon(stream);
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoWarnLog($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}\n{e}");
                    errorList.Add($"Error Loading file \"{filename}\" ({e.GetType()}): {e.Message}");
                }
                if (counter++ > 10)
                {
                    yield return 0;
                    counter = 0;
                }
            }

            LoadingCoroutineLocal = null;
            if (errorList.Count > 0)
            {
                KwyttoDialog.ShowModal(new KwyttoDialog.BindProperties
                {
                    title = "WTS - Errors loading vehicle Files",
                    scrollText = string.Join("\r\n", errorList.ToArray()),
                    buttons = KwyttoDialog.basicOkButtonBar,
                    showClose = true
                });
            }
        }

        private IEnumerator ReloadBuildingAssets_Coroutine()
        {
            yield return 0;
            Data.AssetsDescriptors.Clear();
            var counter = 0;
            foreach (var asset in BuildingIndexes.instance.PrefabsData.Where(x => x.Value.PackageName.TrimToNull() != null))
            {
                LoadDescriptorsFromXmlAsset(asset.Value.Info as BuildingInfo);
                if (counter++ > 10)
                {
                    yield return 0;
                    counter = 0;
                }
            }
            LoadingCoroutineAssets = null;
        }



        private void LoadDescriptorsFromXmlCommon(FileStream stream) => LoadDescriptorsFromXml(stream, null, ref Data.GlobalDescriptors);
        private void LoadDescriptorsFromXmlAsset(BuildingInfo info)
        {
            if (WEMainController.GetDirectoryForAssetOwn(info) is string str)
            {
                var filePath = Path.Combine(str, DefaultFilename);
                if (File.Exists(filePath))
                {
                    using (FileStream stream = File.OpenRead(filePath))
                    {
                        LoadDescriptorsFromXml(stream, info, ref Data.AssetsDescriptors);
                    }
                }
            }
        }
        private void LoadDescriptorsFromXml(FileStream stream, BuildingInfo info, ref SimpleXmlDictionary<string, WriteOnBuildingXml> referenceDic)
        {
            var serializer = new XmlSerializer(typeof(WriteOnBuildingXml));

            LogUtils.DoLog($"trying deserialize: {info}");

            if (serializer.Deserialize(stream) is WriteOnBuildingXml config)
            {
                if (info != null)
                {
                    config.BuildingInfoName = info.name;
                }
                else if (config.BuildingInfoName == null)
                {
                    throw new Exception("Building name not set at file!!!!");
                }
                referenceDic[config.BuildingInfoName] = config;
            }
            else
            {
                throw new Exception("The file wasn't recognized as a valid descriptor!");
            }
        }
        public void MarkLinesDirty() => m_lastUpdateLines = SimulationManager.instance.m_currentTickIndex;
        public static StopPointDescriptorLanes[] GetStopPointsDescriptorFor(string building)
        {
            var instance = ModInstance.Controller.BuildingPropsSingleton;
            if (!(building is null) && !instance.m_buildingStopsDescriptor.ContainsKey(building))
            {
                GetTargetDescriptor(building, out _, out WriteOnBuildingXml target);
                instance.m_buildingStopsDescriptor[building] = null;
                ModInstance.Controller.BuildingPropsSingleton.StartCoroutine(ModInstance.Controller.BuildingPropsSingleton.DoMapStopPoints(building, PrefabCollection<BuildingInfo>.FindLoaded(building), target?.StopMappingThresold ?? 1f));
            }
            return instance.m_buildingStopsDescriptor[building];
        }
        internal static void SetAssetDescriptor(BuildingInfo info, WriteOnBuildingXml desc)
        {
            if (desc is null)
            {
                if (WTSBuildingData.Instance.AssetsDescriptors.ContainsKey(info.name))
                {
                    WTSBuildingData.Instance.AssetsDescriptors.Remove(info.name);
                }
            }
            else
            {
                desc.BuildingInfoName = info.name;
                WTSBuildingData.Instance.AssetsDescriptors[info.name] = desc;
            }
            WTSBuildingData.Instance.CleanCache();
        }
        internal static void SetCityDescriptor(BuildingInfo info, WriteOnBuildingXml desc)
        {
            if (desc is null)
            {
                if (WTSBuildingData.Instance.CityDescriptors.ContainsKey(info.name))
                {
                    WTSBuildingData.Instance.CityDescriptors.Remove(info.name);
                }
            }
            else
            {
                desc.BuildingInfoName = info.name;
                WTSBuildingData.Instance.CityDescriptors[info.name] = desc;
            }
            WTSBuildingData.Instance.CleanCache();
        }

        public static void AfterUpdateTransformOverride(CameraController __instance)
        {
            if (LoadingManager.instance.m_loadingComplete && SimulationManager.instance.m_currentTickIndex - ModInstance.Controller.BuildingPropsSingleton.lastFrameOverriden > 24)
            {
                return;
            }
            __instance.m_minDistance = 1;

            Vector3 vector = __instance.transform.position;
            vector.y = targetHeight + (Mathf.Sin(__instance.m_currentAngle.y * Mathf.Deg2Rad) * __instance.m_targetSize);
            __instance.transform.position = vector;
        }
        public static ushort FindParentBuilding(ushort buildingID)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            ushort result = buildingID;
            ushort parentBuilding = instance.m_buildings.m_buffer[buildingID].m_parentBuilding;
            int num = 0;
            while (parentBuilding != 0)
            {
                result = parentBuilding;
                parentBuilding = instance.m_buildings.m_buffer[parentBuilding].m_parentBuilding;
                if (++num >= 49152)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            return result;
        }
    }


}
