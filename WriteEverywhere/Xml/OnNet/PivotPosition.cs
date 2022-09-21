﻿using WriteEverywhere.Localization;

namespace WriteEverywhere.Xml
{
    public enum PivotPosition
    {
        Left,
        Right,
        LeftInvert,
        RightInvert,
        Center,
        CenterInvert,
        CenterLookingLeft,
        CenterLookingRight,
    }

    public static class PivotPositionExtensions
    {
        public static float GetSideOffsetPositionMultiplier(this PivotPosition pivotPosition)
        {
            switch (pivotPosition)
            {
                case PivotPosition.Left:
                case PivotPosition.Right:
                    return 1;
                case PivotPosition.LeftInvert:
                case PivotPosition.RightInvert:
                    return -1;
                default:
                    return 0;
            }
        }
        public static float GetRotationZ(this PivotPosition pivotPosition)
        {
            switch (pivotPosition)
            {
                case PivotPosition.Left:
                case PivotPosition.RightInvert:
                case PivotPosition.Center:
                    return 0;
                case PivotPosition.Right:
                case PivotPosition.LeftInvert:
                case PivotPosition.CenterInvert:
                    return 180;
                case PivotPosition.CenterLookingLeft:
                    return 90;
                default:
                    return 270;
            }
        }
        public static string GetLocalizedName(this PivotPosition pivot)
        {
            switch (pivot)
            {
                case PivotPosition.Left: return Str.we_PivotPosition__Left;
                case PivotPosition.Right: return Str.we_PivotPosition__Right;
                case PivotPosition.Center: return Str.we_PivotPosition__Center;
                case PivotPosition.LeftInvert: return Str.we_PivotPosition__LeftInvert;
                case PivotPosition.RightInvert: return Str.we_PivotPosition__RightInvert;
                case PivotPosition.CenterInvert: return Str.we_PivotPosition__CenterInvert;
                case PivotPosition.CenterLookingLeft: return Str.we_PivotPosition__CenterLookingLeft;
                case PivotPosition.CenterLookingRight: return Str.we_PivotPosition__CenterLookingRight;
            }
            return null;
        }
    }
}