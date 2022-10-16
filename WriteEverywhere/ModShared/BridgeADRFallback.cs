extern alias ADR;

using ADR::Bridge_WE2ADR;
using ColossalFramework.Math;
using Kwytto.Utils;
using UnityEngine;

namespace WriteEverywhere.ModShared
{
    public class BridgeADRFallback : IBridge
    {
        public override int Priority { get; } = 1000;

        public override string GetStreetSuffix(ushort idx)
        {
            string result;
            LogUtils.DoLog($"!UpdateMeshStreetSuffix NonCustom {NetManager.instance.m_segments.m_buffer[idx].m_nameSeed}");
            if (NetManager.instance.m_segments.m_buffer[idx].Info.m_netAI is RoadBaseAI ai)
            {
                var randomizer = new Randomizer(NetManager.instance.m_segments.m_buffer[idx].m_nameSeed);
                randomizer.Int32(12);
                result = ReflectionUtils.RunPrivateMethod<string>(ai, "GenerateStreetName", randomizer);
            }
            else
            {
                result = "???";
            }

            return result;
        }

        private Vector2? m_cachedPos;
        private readonly Color[] m_randomColors = { Color.black, Color.gray, Color.white, Color.red, new Color32(0xFF, 0x88, 0, 0xFf), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };

        public override bool AddressesAvailable { get; } = false;

        public override bool IsBridgeEnabled { get; } = true;

        public override bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName) => SegmentUtils.GetBasicAddressStreetAndNumber(sidewalk, midPosBuilding, out number, out streetName);
        public override Color GetDistrictColor(ushort districtId) => m_randomColors[districtId % m_randomColors.Length];
        public override string GetStreetQualifier(ushort idx) => GetStreetFullName(idx).Replace(GetStreetSuffix(idx), "");
        public override byte GetDirection(ushort segmentId) => SegmentUtils.GetCardinalDirectionSegment(segmentId, SegmentUtils.MileageStartSource.DEFAULT);

    }
}

