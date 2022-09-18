using Kwytto.Utils;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Rendering;

namespace WriteEverywhere.Xml
{
    public abstract class BaseWriteOnXml
    {
        private Vector3Xml m_propPosition = new Vector3Xml();
        private Vector3Xml m_propRotation = new Vector3Xml();
        private Vector3Xml m_scale = (Vector3Xml)Vector3.one;

        [XmlIgnore]
        public Vector3 PropScale
        {
            get => Scale;
            set => Scale = (Vector3Xml)value;
        }

        [XmlElement("position")]
        public Vector3Xml PropPosition
        {
            get => m_propPosition;
            set { m_propPosition = value; OnChangeMatrixData(); }
        }
        [XmlElement("rotation")]
        public Vector3Xml PropRotation
        {
            get => m_propRotation;
            set { m_propRotation = value; OnChangeMatrixData(); }
        }
        [XmlElement("scale")]
        public Vector3Xml Scale
        {
            get => m_scale;
            set { m_scale = value; OnChangeMatrixData(); }
        }

        public virtual void OnChangeMatrixData()
        {
            for (int i = 0; i < 32; i++)
            {
                RenderManager.instance.UpdateGroups(i);
            }
        }

        public abstract TextParameterWrapper GetParameter(int idx);
        public abstract PrefabInfo TargetAssetParameter { get; }
        public abstract TextRenderingClass RenderingClass { get; }
        public abstract string DescriptorOverrideFont { get; }
        [XmlIgnore]
        internal string lastLayoutVersion = null;
        [XmlAttribute("WE_layoutVersion")]
        public string LayoutVersion
        {
            get => ModInstance.FullVersion;
            set => lastLayoutVersion = value;
        }
    }

}
