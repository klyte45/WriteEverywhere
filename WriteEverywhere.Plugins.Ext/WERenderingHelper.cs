using UnityEngine;
using WriteEverywhere.Assets;
using WriteEverywhere.Font.Utility;

namespace WriteEverywhere.Plugins.Ext
{
    public static class WERenderingHelper
    {
        public static readonly int[] kTriangleIndices = new int[]    {
            0,
            1,
            3,
            3,
            1,
            2
        };

        public static readonly Mesh basicMesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(-.5f, -.5f, 0f),
                new Vector3(0.5f, -.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-.5f, 0.5f, 0f),
            },
            uv = new[]
            {
                new Vector2(1, 0),
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            },
            triangles = kTriangleIndices
        };

        static WERenderingHelper()
        {
            basicMesh.RecalculateNormals();
            basicMesh.RecalculateTangents();
            basicMesh.RecalculateBounds();
        }

        public static BasicRenderInformation GenerateBri(Texture2D tex, Vector4 borders = default, float pixelDensity = 1000)
        {
            return new BasicRenderInformation
            {
                m_mesh = WERenderingHelper.basicMesh,
                m_fontBaseLimits = new RangeVector { min = 0, max = 1 },
                m_YAxisOverflows = new RangeVector { min = -.5f, max = .5f },
                m_sizeMetersUnscaled = new Vector2(tex.width / (float)tex.height, 1),
                m_offsetScaleX = tex.width / (float)tex.height,
                m_generatedMaterial = new Material(WEAssetLibrary.instance.FontShader)
                {
                    mainTexture = tex
                },
                m_borders = borders,
                m_pixelDensityMeters = pixelDensity,
                m_lineOffset = .5f,
                m_expandXIfAlone = true
            };

        }
    }
}
