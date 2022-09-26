using Kwytto.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace WriteEverywhere.Overrides
{
    public class InstanceManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; private set; }


        public static void OnInstanceRenamed(ref InstanceID id)
        {
            if (id.Building > 0)
            {
                CallBuildRenamedEvent(id.Building);
            }
            if (id.NetSegment > 0)
            {
                CallBuildRenamedEvent(id.NetSegment);
            }

        }

        #region Hooking

        public void Awake()
        {
            RedirectorInstance = GameObjectUtils.CreateElement<Redirector>(transform);
            LogUtils.DoLog("Loading Instance Manager Overrides");
            #region Release Line Hooks
            MethodInfo posRename = typeof(InstanceManagerOverrides).GetMethod("OnInstanceRenamed", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(InstanceManager).GetMethod("SetName", RedirectorUtils.allFlags), null, posRename);
            #endregion

        }
        #endregion

        public static void CallBuildRenamedEvent(ushort building) => BuildingManager.instance.StartCoroutine(CallBuildRenamedEvent_impl(building));
        private static IEnumerator CallBuildRenamedEvent_impl(ushort building)
        {
            yield return new WaitForSeconds(1);
            ModInstance.Controller.OnBuildingNameChanged(building);
            ModInstance.Controller.ConnectorTLM.OnAutoNameParameterChanged();
        }
        public static void CallSegmentRenamedEvent(ushort segment) => BuildingManager.instance.StartCoroutine(CallSegmentRenamedEvent_impl(segment));
        private static IEnumerator CallSegmentRenamedEvent_impl(ushort segment)
        {
            yield return new WaitForSeconds(1);
            ModInstance.Controller.OnSegmentNameChanged(segment);
            ModInstance.Controller.ConnectorTLM.OnAutoNameParameterChanged();
        }

    }
}

