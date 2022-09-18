
using ColossalFramework.Math;
using HarmonyLib;
using Kwytto.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using WriteEverywhere.Data;

namespace WriteEverywhere.Override
{

    namespace WriteEverywhere.Overrides
    {
        public class NetManagerOverrides : MonoBehaviour, IRedirectable
        {
            public Redirector RedirectorInstance { get; set; }


            #region Events
            public static event Func<ushort, IEnumerator> EventNodeChanged;
            public static event Func<ushort, IEnumerator> EventSegmentChanged;
            public static event Func<ushort, IEnumerator> EventSegmentReleased;
            public static event Func<ushort, IEnumerator> EventSegmentNameChanged;

#pragma warning disable IDE0051 // Remover membros privados não utilizados
            private static void OnNodeChanged(ref ushort node)
            {
                nodeChangeBuffer.Add(node);
                cooldown = 15;
            }
            private static void OnSegmentCreated(ref ushort segment, ref ushort startNode, ref ushort endNode)
            {

                nodeChangeBuffer.Add(startNode);
                nodeChangeBuffer.Add(endNode);
                segmentChangeBuffer.Add(segment);
                cooldown = 15;
            }
            private static void OnSegmentReleased(ref ushort segment)
            {
                ushort segment_ = segment;
                ref NetSegment segmentData = ref NetManager.instance.m_segments.m_buffer[segment_];
                nodeChangeBuffer.Add(segmentData.m_startNode);
                nodeChangeBuffer.Add(segmentData.m_endNode);
                segmentChangeBuffer.Add(segment);
                segmentReleaseBuffer.Add(segment);
                cooldown = 15;
            }
            private static void OnSegmentNameChanged(ref ushort segmentID)
            {
                segmentNameChangeBuffer.Add(segmentID);
                cooldown = 15;
            }



            public static IEnumerable<CodeInstruction> AfterTerrainUpdateTranspile(IEnumerable<CodeInstruction> instr, ILGenerator il)
            {
                var instrList = new List<CodeInstruction>(instr);
                MethodInfo TerrainUpdateNetNode = typeof(NetManagerOverrides).GetMethod("TerrainUpdateNetNode", RedirectorUtils.allFlags);
                MethodInfo TerrainUpdateNetSegment = typeof(NetManagerOverrides).GetMethod("TerrainUpdateNetSegment", RedirectorUtils.allFlags);
                int i = 2;
                for (; i < instrList.Count; i++)
                {
                    if (instrList[i - 2].opcode == OpCodes.Ldloc_S && instrList[i - 2].operand is LocalBuilder lb && lb.LocalIndex == 13
                        && instrList[i - 1].opcode == OpCodes.Ldc_R4 && instrList[i - 1].operand is float k && k == 0)
                    {
                        instrList.InsertRange(i + 1, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_S, 10),
                        new CodeInstruction(OpCodes.Call, TerrainUpdateNetNode),
                    });
                        break;
                    }
                }
                for (; i < instrList.Count; i++)
                {
                    if (instrList[i - 2].opcode == OpCodes.Ldloc_S && instrList[i - 2].operand is LocalBuilder lb && lb.LocalIndex == 23
                        && instrList[i - 1].opcode == OpCodes.Ldc_R4 && instrList[i - 1].operand is float k && k == 0)
                    {
                        instrList.InsertRange(i + 1, new List<CodeInstruction> {
                        new CodeInstruction(OpCodes.Ldloc_S, 16),
                        new CodeInstruction(OpCodes.Call, TerrainUpdateNetSegment),
                    });
                        break;
                    }
                }

                LogUtils.PrintMethodIL(instrList);
                return instrList;
            }

            public static void TerrainUpdateNetNode(ushort netNode)
            {
                if (LoadingManager.instance.m_loadingComplete)
                {
                    nodeChangeBuffer.Add(netNode);
                }
                cooldown = 15;
            }

            public static void TerrainUpdateNetSegment(ushort segmentId)
            {
                if (LoadingManager.instance.m_loadingComplete)
                {
                    segmentChangeBuffer.Add(segmentId);
                }
                cooldown = 15;
            }

            private static int cooldown = 0;
            private static HashSet<ushort> nodeChangeBuffer = new HashSet<ushort>();
            private static HashSet<ushort> segmentChangeBuffer = new HashSet<ushort>();
            private static HashSet<ushort> segmentReleaseBuffer = new HashSet<ushort>();
            private static HashSet<ushort> segmentNameChangeBuffer = new HashSet<ushort>();

            public void Update()
            {
                if (cooldown > 0)
                {
                    cooldown--;
                    return;
                }
                else if (cooldown == 0)
                {
                    cooldown--;
                    foreach (var node in nodeChangeBuffer)
                    {
                        // ModInstance.Controller?.RoadPropsSingleton?.OnNodeChanged(node);
                        StartCoroutine(EventNodeChanged?.Invoke(node));
                        // WTSBuildingDataCaches.PurgeStopCache(node);
                    }
                    foreach (var segment in segmentChangeBuffer)
                    {
                        WTSOnNetData.Instance.OnSegmentChanged(segment);
                        StartCoroutine(EventSegmentChanged?.Invoke(segment));
                    }
                    foreach (var segment in segmentReleaseBuffer)
                    {
                        StartCoroutine(EventSegmentReleased?.Invoke(segment));
                    }
                    foreach (var segment in segmentNameChangeBuffer)
                    {
                        StartCoroutine(EventSegmentNameChanged?.Invoke(segment));
                    }
                    nodeChangeBuffer.Clear();
                    segmentChangeBuffer.Clear();
                }
            }

            #endregion

            #region Hooking

            public void Awake()
            {
                LogUtils.DoLog("Loading Net Manager Overrides");
                RedirectorInstance = GameObjectUtils.CreateElement<Redirector>(transform);
                #region Net Manager Hooks
                MethodInfo OnNodeChanged = GetType().GetMethod("OnNodeChanged", RedirectorUtils.allFlags);
                MethodInfo OnSegmentCreated = GetType().GetMethod("OnSegmentCreated", RedirectorUtils.allFlags);
                MethodInfo OnSegmentReleased = GetType().GetMethod("OnSegmentReleased", RedirectorUtils.allFlags);
                MethodInfo OnSegmentNameChanged = GetType().GetMethod("OnSegmentNameChanged", RedirectorUtils.allFlags);
                MethodInfo AfterTerrainUpdateTranspile = GetType().GetMethod("AfterTerrainUpdateTranspile", RedirectorUtils.allFlags);

                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("CreateNode", RedirectorUtils.allFlags), null, OnNodeChanged);
                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("ReleaseNode", RedirectorUtils.allFlags), null, OnNodeChanged);
                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("CreateSegment", RedirectorUtils.allFlags, null, new[] { typeof(ushort).MakeByRefType(), typeof(Randomizer).MakeByRefType(), typeof(NetInfo), typeof(TreeInfo), typeof(ushort), typeof(ushort), typeof(Vector3), typeof(Vector3), typeof(uint), typeof(uint), typeof(bool) }, null), null, OnSegmentCreated);
                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("ReleaseSegment", RedirectorUtils.allFlags), OnSegmentReleased);
                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("SetSegmentNameImpl", RedirectorUtils.allFlags), null, OnSegmentNameChanged);
                RedirectorInstance.AddRedirect(typeof(NetManager).GetMethod("AfterTerrainUpdate", RedirectorUtils.allFlags), null, null, AfterTerrainUpdateTranspile);
                #endregion

            }
            #endregion


        }
    }

}
