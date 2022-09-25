using UnityEngine;

namespace WriteEverywhere.Utils
{
    public static class MeshExtensions
    {
        public static Mesh Copy(this Mesh mesh)
        {
            var copy = new Mesh();
            foreach (var property in typeof(Mesh).GetProperties())
            {
                if (property.GetSetMethod() != null && property.GetGetMethod() != null)
                {
                    var val = property.GetValue(mesh, null);
                    property.SetValue(copy, val, null);
                }
            }
            return copy;
        }
    }
}

