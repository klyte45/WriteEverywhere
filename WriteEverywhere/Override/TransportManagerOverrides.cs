
using Kwytto.Utils;
using System;
using System.Reflection;
using UnityEngine;
using WriteEverywhere.Data;

namespace WriteEverywhere.Overrides
{
    public class TransportManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; private set; }


        #region Events

        public static void PushIntoStackBuilding(bool __result, Vector3 position)
        {
            if (__result)
            {
                ushort buildingId = BuildingManager.instance.FindBuilding(position, 100f, ItemClass.Service.None, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
                ModInstance.Controller?.m_buildingStack.Add(buildingId);
                ModInstance.Controller?.ResetLinesCooldown();
            }
        }
        public static void BeforeRemoveStop(ref TransportLine __instance, int index, ushort lineID)
        {
            if ((__instance.m_flags & TransportLine.Flags.Temporary) != TransportLine.Flags.None || __instance.m_stops > NetManager.MAX_NODE_COUNT)
            {
                return;
            }
            ushort num;
            if (index == -1)
            {
                index += __instance.CountStops(lineID);
            }
            num = __instance.m_stops;
            for (int i = 0; i < index && num <= NetManager.MAX_NODE_COUNT; i++)
            {
                num = TransportLine.GetNextStop(num);
                if (num == __instance.m_stops)
                {
                    break;
                }
            }
            WTSBuildingData.Instance.CacheData.PurgeStopCache(num);
        }
        public static void AfterRemoveLine(ushort lineID) => WTSBuildingData.Instance.CacheData.PurgeLineCache(lineID);

        public static void PushIntoStackLine(ushort lineID)
        {
            ModInstance.Controller?.m_lineStack.Add(lineID);
            ModInstance.Controller?.ResetLinesCooldown();
        }


        #endregion

        #region Hooking

        public void Awake()
        {
            RedirectorInstance = GameObjectUtils.CreateElement<Redirector>(transform);
            LogUtils.DoLog("Loading Transport Manager Overrides");
            #region Release Line Hooks
            MethodInfo posUpdate = typeof(TransportManagerOverrides).GetMethod("PushIntoStackLine", RedirectorUtils.allFlags);
            MethodInfo posAddStop = typeof(TransportManagerOverrides).GetMethod("PushIntoStackBuilding", RedirectorUtils.allFlags);
            MethodInfo preRemoveStop = typeof(TransportManagerOverrides).GetMethod("BeforeRemoveStop", RedirectorUtils.allFlags);
            MethodInfo posRemoveLine = typeof(TransportManagerOverrides).GetMethod("AfterRemoveLine", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(TransportManager).GetMethod("UpdateLine", RedirectorUtils.allFlags), null, posUpdate);
            RedirectorInstance.AddRedirect(typeof(TransportLine).GetMethod("AddStop", RedirectorUtils.allFlags), null, posAddStop);
            RedirectorInstance.AddRedirect(typeof(TransportLine).GetMethod("RemoveStop", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(int), typeof(Vector3).MakeByRefType() }, null), preRemoveStop);
            RedirectorInstance.AddRedirect(typeof(TransportManager).GetMethod("ReleaseLine", RedirectorUtils.allFlags), null, posRemoveLine);
            #endregion


        }
        #endregion



    }
}
