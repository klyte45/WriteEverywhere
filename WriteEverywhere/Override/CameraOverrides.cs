using Kwytto.Utils;
using System;
using UnityEngine;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Override
{
    public class CameraOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; set; }

        private void Awake()
        {
            RedirectorInstance = GameObjectUtils.CreateElement<Redirector>(transform);
            var overrideNet = typeof(WTSOnNetPropsSingleton).GetMethod("AfterUpdateTransformOverride", ReflectionUtils.allFlags);
            var src = typeof(CameraController).GetMethod("UpdateTransform", ReflectionUtils.allFlags);
            var src2 = Type.GetType("CameraPositions.Detours.CameraControllerDetour, CameraPositions.dll")?.GetMethod("UpdateTransform", ReflectionUtils.allFlags);
            RedirectorInstance.AddRedirect(src, null, overrideNet);
            if (src2 != null)
            {
                LogUtils.DoLog("CameraPositions was found!");
                RedirectorInstance.AddRedirect(src2, null, overrideNet);
            }

        }
    }
}

