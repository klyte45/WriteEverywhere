using ColossalFramework.UI;
using SpriteFontPlus;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.Utils
{
    internal class WTSUtils
    {

        public static void ReloadFontsOf(UIDropDown target, string targetValue, bool hasDefaultOption = false, bool force = false)
        {
            try
            {
                if (force)
                {
                    MainController.ReloadFontsFromPath();
                }
                var items = FontServer.instance.GetAllFonts().ToList();
                items.Sort();
                items.Remove(MainController.DEFAULT_FONT_KEY);
                items.Insert(0, Str.WTS_DEFAULT_FONT_LABEL);
                if (hasDefaultOption)
                {
                    items.Insert(0, Str.WTS_USE_GROUP_SETTING_FONT);
                }
                target.items = items.ToArray();
                if (items.Contains(targetValue))
                {
                    target.selectedIndex = items.IndexOf(targetValue);
                }
                else
                {
                    target.selectedIndex = 0;
                }
            }
            catch
            {
                GameObject.DestroyImmediate(target);
            }
        }
        public static Vector3 axisRotationTG = new Vector3(0, 0, -1);
        public static float degRotationTG = 90;
        public static Vector3 axisRotationN = new Vector3(1, 1, -1);
        public static float degRotationN = 120;
        public static void SolveTangents(Mesh mesh, bool recalculateAfterGenerate)
        {
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            if (recalculateAfterGenerate)
            {
                var normals = mesh.normals.ToArray();
                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = Quaternion.AngleAxis(degRotationN, axisRotationN * Mathf.Deg2Rad) * normals[i];
                }
                mesh.normals = normals;
                var tangents = mesh.tangents.ToArray();
                for (int i = 0; i < tangents.Length; i++)
                {
                    tangents[i] = Quaternion.AngleAxis(degRotationTG, axisRotationTG * Mathf.Deg2Rad) * tangents[i];
                }
                mesh.tangents = tangents;
            }
        }


    }
}

