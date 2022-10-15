using Kwytto.Utils;
using UnityEngine;

namespace WriteEverywhere.Layout
{
    public class WEImageInfo
    {
        public WEImageInfo(string xmlPath)
        {
            this.xmlPath = xmlPath;
        }
        public readonly string xmlPath;
        public Vector4 Borders { get; set; }
        public string Name { get; set; }
        public Texture2D Texture { get; set; }
        public float PixelsPerMeter { get; set; }
        public RectOffset OffsetBorders => new RectOffset(Mathf.RoundToInt(Borders.x * Texture.width), Mathf.RoundToInt(Borders.y * Texture.width), Mathf.RoundToInt(Borders.z * Texture.height), Mathf.RoundToInt(Borders.w * Texture.height));

    }
}
