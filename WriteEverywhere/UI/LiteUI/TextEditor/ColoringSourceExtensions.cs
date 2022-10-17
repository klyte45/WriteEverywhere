using System;
using System.Linq;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    internal static class ColoringSourceExtensions
    {
        public static ColoringSource[] AvailableAtClass(TextRenderingClass srcClass) => Enum.GetValues(typeof(ColoringSource))
            .Cast<ColoringSource>()
            .Where(x =>
                (srcClass != TextRenderingClass.Vehicle || (x != ColoringSource.District && x != ColoringSource.ContrastDistrict))
                && (srcClass == TextRenderingClass.Buildings || srcClass == TextRenderingClass.Vehicle || (x != ColoringSource.PlatformLine && x != ColoringSource.ContrastPlatformLine))
            )
            .ToArray();
    }
}
