extern alias TLM;

using ColossalFramework.Math;
using System.Linq;
using TLM::Bridge_WE2TLM;
using UnityEngine;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;

namespace WriteEverywhere.Rendering
{
    internal static class WEDynamicTextRenderingRules
    {

        #region Main flow
        public static void PropInstancePopulateGroupData(PropInfo info, int layer, InstanceID id, Vector3 position, Vector3 scale, Vector3 angle, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance)
        {
            LightSystem lightSystem = RenderManager.instance.lightSystem;
            if (info.m_prefabDataLayer == layer)
            {
                float y = info.m_generatedInfo.m_size.y * scale.y;
                float num = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale.y * 0.5f;
                min = Vector3.Min(min, position - new Vector3(num, 0f, num));
                max = Vector3.Max(max, position + new Vector3(num, y, num));
                maxRenderDistance = Mathf.Max(maxRenderDistance, info.m_maxRenderDistance);
                maxInstanceDistance = Mathf.Max(maxInstanceDistance, info.m_maxRenderDistance);
            }
            else if (info.m_effectLayer == layer || (info.m_effectLayer == lightSystem.m_lightLayer && layer == lightSystem.m_lightLayerFloating))
            {
                Matrix4x4 matrix4x = default;
                matrix4x.SetTRS(position, Quaternion.AngleAxis(angle.x, Vector3.left) * Quaternion.AngleAxis(angle.y, Vector3.down) * Quaternion.AngleAxis(angle.z, Vector3.back), scale);
                for (int i = 0; i < info.m_effects.Length; i++)
                {
                    Vector3 pos = matrix4x.MultiplyPoint(info.m_effects[i].m_position);
                    Vector3 dir = matrix4x.MultiplyVector(info.m_effects[i].m_direction);
                    info.m_effects[i].m_effect.PopulateGroupData(layer, id, pos, dir, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                }
            }
        }

        public static Color RenderPropMesh<DESC>(PropInfo propInfo, RenderManager.CameraInfo cameraInfo, ushort refId, int boardIdx, int secIdx,
            int layerMask, float refAngleRad, Vector3 position, Vector4 dataVector, Vector3 propAngle, Vector3 propScale,
            DESC descriptor, out Matrix4x4 propMatrix,
            out bool rendered, InstanceID propRenderID) where DESC : BaseWriteOnXml
        {
            Color propColor = GetColorForRule(refId, boardIdx, secIdx, descriptor, out rendered);
            if (!rendered)
            {
                propMatrix = new Matrix4x4();
                return propColor;
            }
            propMatrix = RenderProp(refId, refAngleRad, cameraInfo, propInfo, propColor, position, dataVector, boardIdx, propAngle, propScale, layerMask, out rendered, propRenderID);
            return propColor;
        }

        private static Matrix4x4 RenderProp(ushort refId, float refAngleRad, RenderManager.CameraInfo cameraInfo,
                                    PropInfo propInfo, Color propColor, Vector3 position, Vector4 dataVector, int idx,
                                    Vector3 rotation, Vector3 scale, int layerMask, out bool rendered, InstanceID propRenderID2)
        {
            rendered = false;
            var randomizer = new Randomizer((refId << 6) | (idx + 32));
            Matrix4x4 matrix = default;
            matrix.SetTRS(position, Quaternion.AngleAxis(rotation.y + (refAngleRad * Mathf.Rad2Deg), Vector3.down) * Quaternion.AngleAxis(rotation.x, Vector3.left) * Quaternion.AngleAxis(rotation.z, Vector3.back), scale);
            if (propInfo != null)
            {
                propInfo = propInfo.GetVariation(ref randomizer);
                if (cameraInfo.CheckRenderDistance(position, propInfo.m_maxRenderDistance * scale.sqrMagnitude))
                {
                    int oldLayerMask = cameraInfo.m_layerMask;
                    float oldRenderDist = propInfo.m_lodRenderDistance;
                    propInfo.m_lodRenderDistance *= scale.sqrMagnitude;
                    cameraInfo.m_layerMask = 0x7FFFFFFF;
                    try
                    {
                        PropInstance.RenderInstance(cameraInfo, propInfo, propRenderID2, matrix, position, scale.y, refAngleRad + (rotation.y * Mathf.Deg2Rad), propColor, dataVector, true);
                    }
                    finally
                    {
                        propInfo.m_lodRenderDistance = oldRenderDist;
                        cameraInfo.m_layerMask = oldLayerMask;
                    }
                    rendered = true;
                }
            }
            return matrix;
        }

        public static Color GetColorForRule<DESC>(ushort refId, int boardIdx, int secIdx, DESC descriptor, out bool rendered) where DESC : BaseWriteOnXml
        {
            Color propColor = GetPropColor(refId, boardIdx, secIdx, descriptor, out bool colorFound);
            if (!colorFound)
            {
                rendered = false;
                return propColor;
            }
            propColor.a = 1;
            rendered = true;
            return propColor;
        }


        #endregion

        #region Text handling

        #endregion
        #region Color rules

        private static Randomizer rand = new Randomizer(0);
        public static Color GetPropColor(ushort refId, int boardIdx, int secIdx, BaseWriteOnXml instance, out bool found)
        {
            if (instance is LayoutDescriptorVehicleXml vehicleDescriptor)
            {
                found = true;
                ref Vehicle targetVehicle = ref VehicleManager.instance.m_vehicles.m_buffer[refId];
                return targetVehicle.Info.m_vehicleAI.GetColor(refId, ref targetVehicle, InfoManager.InfoMode.None);
            }
            else if (instance is WriteOnBuildingPropXml buildingDescriptor)
            {
                found = true;
                switch (buildingDescriptor.ColorModeProp)
                {
                    case ColoringMode.Fixed:
                        rand.seed = refId * (1u + (uint)boardIdx);
                        return buildingDescriptor?.FixedColor ?? buildingDescriptor.SimpleProp?.GetColor(ref rand) ?? Color.white;
                    case ColoringMode.ByPlatform:
                        var stops = WTSStopUtils.GetAllTargetStopInfo(buildingDescriptor, refId).Where(x => x.m_lineId != 0);
                        if (buildingDescriptor.UseFixedIfMultiline && stops.GroupBy(x => x.m_lineId).Count() > 1)
                        {
                            rand.seed = refId * (1u + (uint)boardIdx);
                            return buildingDescriptor?.FixedColor ?? buildingDescriptor.SimpleProp?.GetColor(ref rand) ?? Color.white;
                        }
                        if (stops.Count() != 0)
                        {
                            var line = new WTSLine(stops.FirstOrDefault());
                            if (!line.ZeroLine)
                            {
                                return ModInstance.Controller.ConnectorTLM.GetLineColor(line);
                            }
                        }
                        if (!buildingDescriptor.m_showIfNoLine)
                        {
                            found = false;
                            return default;
                        }
                        return Color.white;
                    case ColoringMode.ByDistrict:
                        byte districtId = DistrictManager.instance.GetDistrict(BuildingManager.instance.m_buildings.m_buffer[refId].m_position);
                        return ModInstance.Controller.ConnectorADR.GetDistrictColor(districtId);
                    case ColoringMode.FromBuilding:
                        return BuildingManager.instance.m_buildings.m_buffer[refId].Info.m_buildingAI.GetColor(refId, ref BuildingManager.instance.m_buildings.m_buffer[refId], InfoManager.InfoMode.None);
                }
            }
            else if (instance is WriteOnNetXml n)
            {
                found = n.SimpleProp != null;
                return n.FixedColor ?? n.SimpleProp?.m_color0 ?? Color.white;
            }
            found = false;
            return default;
        }

        internal static readonly Color[] m_spectreSteps = new Color[]
        {
            new Color32(170,170,170,255),
            Color.white,
            Color.red,
            Color.yellow ,
            Color.green  ,
            Color.cyan   ,
            Color.blue   ,
            Color.magenta,
            new Color32(128,0,0,255),
            new Color32(128,128,0,255),
            new Color32(0,128,0,255),
            new Color32(0,128,128,255),
            new Color32(0,0,128,255),
            new Color32(128,0,128,255),
            Color.black,
            new Color32(85,85,85,255),
        };
        #endregion
    }

}
