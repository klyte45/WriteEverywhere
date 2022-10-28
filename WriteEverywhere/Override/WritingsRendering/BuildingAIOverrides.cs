using Kwytto.Utils;
using UnityEngine;
using WriteEverywhere.UI;

namespace WriteEverywhere.Overrides
{
    public class BuildingAIOverrides : Redirector, IRedirectable
    {

        #region Hooking
        public void Awake()
        {
            LogUtils.DoLog("Loading Building AI Overrides");
            AddRedirect(typeof(BuildingAI).GetMethod("RenderMeshes", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterRenderMeshes"));
            AddRedirect(typeof(BuildingDecoration).GetMethod("RenderBuildingMesh", RedirectorUtils.allFlags), null, GetType().GetMethod("AfterRenderBuildingMesh"));
        }
        #endregion

        private static Matrix4x4 defaultAssetEditorReference = default;
        static BuildingAIOverrides()
        {
            defaultAssetEditorReference.SetTRS(new Vector3(0f, 60f, 0f), Quaternion.identity, Vector3.one);
        }

        public static void AfterRenderBuildingMesh(RenderManager.CameraInfo cameraInfo, BuildingInfo info)
        {
            if (!SceneUtils.IsAssetEditor)
            {
                return;
            }
            var singleton = ModInstance.Controller.BuildingPropsSingleton;
            var targetDescriptor = singleton.PrepareTargetDescriptor(ref defaultAssetEditorReference, 0, info, BuildingLiteUI.Instance.CurrentEditingInfo?.name);
            if (targetDescriptor == null)
            {
                return;
            }
            for (int i = 0; i < targetDescriptor.PropInstances.Length; i++)
            {
                if (targetDescriptor.PropInstances[i].SubBuildingPivotReference == 0)
                {
                    singleton.RenderDescriptor(cameraInfo, 0, default, 0, info, AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags1, AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags2, defaultAssetEditorReference, -1, ref targetDescriptor, i);
                }
            }
            if (info.m_subBuildings != null)
            {
                for (ushort s = 0; s < info.m_subBuildings.Length; s++)
                {
                    ushort idx = (ushort)(info.m_subBuildings.Length - s);
                    BuildingInfo.SubInfo subBuilding = info.m_subBuildings[s];
                    for (int i = 0; i < targetDescriptor.PropInstances.Length; i++)
                    {
                        if (targetDescriptor.PropInstances[i].SubBuildingPivotReference == idx)
                        {
                            singleton.RenderDescriptor(cameraInfo, idx, subBuilding.m_position, (subBuilding.m_angle - 90) * Mathf.Deg2Rad, info, AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags1, AssetEditorFlagsToggleLiteUI.Instance.CurrentFlags2, defaultAssetEditorReference * Matrix4x4.Translate(subBuilding.m_position) * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, -subBuilding.m_angle, 0), Vector3.one), -1, ref targetDescriptor, i);
                        }
                    }
                }
            }

        }

        public static void AfterRenderMeshes(RenderManager.CameraInfo cameraInfo, ushort buildingID, ref Building data)
        {
            if (SceneUtils.IsAssetEditor)
            {
                return;
            }
            if (LoadingManager.instance.m_currentlyLoading)
            {
                return;
            }

            if (ModInstance.Controller?.BuildingPropsSingleton == null)
            {
                return;
            }

            RenderManager renderManager = RenderManager.instance;
            BuildingInfo info = data.Info;
            if ((data.m_flags & Building.Flags.Created) == 0 || info.m_mesh == null)
            {
                return;
            }
            Vector3 position = data.m_position;
            float radius = info.m_renderSize + info.m_mesh.bounds.extents.magnitude;
            position.y += (info.m_size.y - data.m_baseHeight) * 0.5f;
            var shallRender = cameraInfo.Intersect(position, radius);
            if (!shallRender && ((!(info.m_buildingAI is TransportStationAI) && !(info.m_buildingAI is OutsideConnectionAI)) || data.m_parentBuilding != 0))
            {
                return;
            }
            if (renderManager.RequireInstance(buildingID, 1u, out uint num))
            {
                ref RenderManager.Instance renderInstance = ref renderManager.m_instances[num];
                if (renderInstance.m_dirty)
                {
                    renderInstance.m_dirty = false;
                    info.m_buildingAI.RefreshInstance(cameraInfo, buildingID, ref data, -1, ref renderInstance);
                }
                if (!shallRender)
                {
                    ModInstance.Controller.BuildingPropsSingleton.UpdateLinesBuilding(buildingID, ref data, ref renderInstance.m_dataMatrix1);
                }
                else
                {
                    ModInstance.Controller.BuildingPropsSingleton.AfterRenderInstanceImpl(cameraInfo, buildingID, -1, ref renderInstance);
                }
            }
        }
    }
}
