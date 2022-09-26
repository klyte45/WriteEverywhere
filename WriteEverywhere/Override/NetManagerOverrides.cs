
using ColossalFramework.Math;
using HarmonyLib;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace WriteEverywhere.Overrides
{

    public class NetManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; set; }


        #region Events

        private static void OnNodeChanged(ref ushort node)
        {
            ModInstance.Controller?.nodeChangeBuffer.Add(node);
            ModInstance.Controller?.ResetSegmentCooldown();
        }
        private static void OnSegmentCreated(ref ushort segment, ref ushort startNode, ref ushort endNode)
        {

            ModInstance.Controller?.nodeChangeBuffer.Add(startNode);
            ModInstance.Controller?.nodeChangeBuffer.Add(endNode);
            ModInstance.Controller?.segmentChangeBuffer.Add(segment);
            ModInstance.Controller?.ResetSegmentCooldown();
        }
        private static void OnSegmentReleased(ref ushort segment)
        {
            ushort segment_ = segment;
            ref NetSegment segmentData = ref NetManager.instance.m_segments.m_buffer[segment_];
            ModInstance.Controller?.nodeChangeBuffer.Add(segmentData.m_startNode);
            ModInstance.Controller?.nodeChangeBuffer.Add(segmentData.m_endNode);
            ModInstance.Controller?.segmentChangeBuffer.Add(segment);
            ModInstance.Controller?.segmentReleaseBuffer.Add(segment);
            ModInstance.Controller?.ResetSegmentCooldown();
        }
        private static void OnSegmentNameChanged(ref ushort segmentID)
        {
            ModInstance.Controller?.segmentNameChangeBuffer.Add(segmentID);
            ModInstance.Controller?.ResetSegmentCooldown();
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

