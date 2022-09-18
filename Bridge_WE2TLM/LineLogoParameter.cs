using UnityEngine;

namespace Bridge_WE2TLM
{
    public class LineLogoParameter
    {
        public string fileName;
        public Color color;
        public string text;
        public LineLogoParameter() { }
        public LineLogoParameter(string fileName, Color color, string text)
        {
            this.fileName = fileName;
            this.color = color;
            this.text = text;
        }
    }
}
