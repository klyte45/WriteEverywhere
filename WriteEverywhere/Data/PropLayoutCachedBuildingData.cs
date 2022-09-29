﻿extern alias TLM;
using UnityEngine;

namespace WriteEverywhere.Data
{
    public struct PropLayoutCachedBuildingData
    {
        public uint m_linesUpdateFrame;

        public Vector3 m_buildingPositionWhenGenerated;

        public Vector3 m_cachedPosition;
        public Vector3 m_cachedRotation;
        public Vector3 m_cachedScale;
        public Matrix4x4 m_cachedMatrix;

        public int m_cachedArrayRepeatTimes;
        public Vector3 m_cachedOriginalPosition;
        public Vector3 m_cachedArrayItemPace;

        public bool HasAnyBoard() => true;
        internal void ClearCache()
        {
            m_buildingPositionWhenGenerated = default;
        }
    }

}