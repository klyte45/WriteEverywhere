using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Overrides
{
    public class NetToolOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            #region Hooks
            System.Reflection.MethodInfo postCreateNodeImpl = typeof(NetToolOverrides).GetMethod("PostCreateNodeImpl", RedirectorUtils.allFlags);
            AddRedirect(typeof(NetTool).GetMethod("CreateNodeImpl", RedirectorUtils.allFlags, null, new[]
            {
                typeof(NetInfo), typeof(bool ), typeof(bool ), typeof(NetTool.ControlPoint ), typeof(NetTool.ControlPoint ), typeof(NetTool.ControlPoint )
            }, null), null, postCreateNodeImpl);
            #endregion 
        }

        protected static void PostCreateNodeImpl(NetTool.ControlPoint middlePoint, NetTool __instance, bool __result)
        {
            if (__result && ReflectionUtils.GetPrivateField<bool>(__instance, "m_upgrading"))
            {
                var srcSegment = middlePoint.m_segment;
                var dstSegment = ReflectionUtils.GetPrivateField<HashSet<ushort>>(__instance, "m_upgradedSegments").First();
                ModInstance.Controller.OnNetPropsSingleton.TransferData(srcSegment, dstSegment);
            }
        }
    }
}

