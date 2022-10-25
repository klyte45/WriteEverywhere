using Kwytto.Utils;
using UnityEngine;

namespace WriteEverywhere.Overrides
{
    public class VehicleAIOverrides : Redirector, IRedirectable
    {

        public void Awake()
        {
            #region Hooks
            System.Reflection.MethodInfo postRenderExtraStuff = GetType().GetMethod("AfterRenderExtraStuff", RedirectorUtils.allFlags);
            LogUtils.DoLog($"Patching=> {postRenderExtraStuff}");
            AddRedirect(typeof(VehicleAI).GetMethod("RenderExtraStuff", RedirectorUtils.allFlags), null, postRenderExtraStuff);

            #endregion
        }
        public static void AfterRenderExtraStuff(VehicleAI __instance, ushort vehicleID, ref Vehicle data, RenderManager.CameraInfo cameraInfo, ref Vector3 position, ref Quaternion rotation, ref Vector3 scale, ref Vector3 swayPosition)
        {
            ModInstance.Controller?.VehicleTextsSingleton?.AfterRenderExtraStuff(__instance, vehicleID, ref data, cameraInfo, ref position, ref rotation, ref scale, ref swayPosition);
        }
    }
}
