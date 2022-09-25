using ColossalFramework;
using Kwytto.Utils;
using System;
using UnityEngine;

namespace WriteEverywhere.Overrides
{
    public class RoadNodeOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            #region Hooks
            System.Reflection.MethodInfo postRenderMeshs = GetType().GetMethod("AfterRenderSegment", RedirectorUtils.allFlags);
            System.Reflection.MethodInfo orig = typeof(NetSegment).GetMethod("RenderInstance", new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(int) });
            System.Reflection.MethodInfo calcGroup = typeof(NetManager).GetMethod("CalculateGroupData");
            System.Reflection.MethodInfo popGroup = typeof(NetManager).GetMethod("PopulateGroupData");

            System.Reflection.MethodInfo AfterCalculateGroupData = GetType().GetMethod("AfterCalculateGroupData", RedirectorUtils.allFlags);
            System.Reflection.MethodInfo AfterPopulateGroupData = GetType().GetMethod("AfterPopulateGroupData", RedirectorUtils.allFlags);

            LogUtils.DoLog($"Patching: {orig} => {postRenderMeshs} ");
            AddRedirect(orig, null, postRenderMeshs);
            LogUtils.DoLog($"PatchingC: {calcGroup} => {AfterCalculateGroupData} ");
            AddRedirect(calcGroup, null, AfterCalculateGroupData);
            LogUtils.DoLog($"PatchingP: {popGroup} => {AfterPopulateGroupData} ");
            AddRedirect(popGroup, null, AfterPopulateGroupData);
            #endregion
        }


        public static void AfterRenderSegment(RenderManager.CameraInfo cameraInfo, ushort segmentID)
        {
            if (LoadingManager.instance.m_currentlyLoading)
            {
                return;
            }

            ref NetSegment data = ref NetManager.instance.m_segments.m_buffer[segmentID];
            ModInstance.Controller?.OnNetPropsSingleton?.AfterRenderInstanceImpl(cameraInfo, segmentID, ref data);
        }


        public static void AfterCalculateGroupData(ref NetManager __instance, int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays, ref bool __result)
        {
            if (LoadingManager.instance.m_currentlyLoading || ModInstance.Controller?.OnNetPropsSingleton is null)
            {
                return;
            }

            int num = groupX * 270 / 45;
            int num2 = groupZ * 270 / 45;
            int num3 = (groupX + 1) * 270 / 45 - 1;
            int num4 = (groupZ + 1) * 270 / 45 - 1;

            for (int k = num2; k <= num4; k++)
            {
                for (int l = num; l <= num3; l++)
                {
                    int num8 = k * 270 + l;
                    ushort num9 = __instance.m_segmentGrid[num8];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        ModInstance.Controller.OnNetPropsSingleton.CalculateGroupData(num9, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
                        num9 = __instance.m_segments.m_buffer[num9].m_nextGridSegment;
                        if (++num10 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }

        public static void AfterPopulateGroupData(ref NetManager __instance, int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, ref Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance, ref bool requireSurfaceMaps)
        {
            if (LoadingManager.instance.m_currentlyLoading)
            {
                return;
            }

            int num = groupX * 270 / 45;
            int num2 = groupZ * 270 / 45;
            int num3 = (groupX + 1) * 270 / 45 - 1;
            int num4 = (groupZ + 1) * 270 / 45 - 1;

            for (int k = num2; k <= num4; k++)
            {
                for (int l = num; l <= num3; l++)
                {
                    int num8 = k * 270 + l;
                    ushort num9 = __instance.m_segmentGrid[num8];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        ModInstance.Controller?.OnNetPropsSingleton?.PopulateGroupData(num9, layer, ref vertexIndex, ref triangleIndex, ref groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                        num9 = __instance.m_segments.m_buffer[num9].m_nextGridSegment;
                        if (++num10 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }



    }
}
