using Kwytto.Interfaces;
using Kwytto.Utils;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

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
            set { m_propPosition = value; BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData()); }
        }
        [XmlElement("rotation")]
        public Vector3Xml PropRotation
        {
            get => m_propRotation;
            set { m_propRotation = value; BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData()); }
        }
        [XmlElement("scale")]
        public Vector3Xml Scale
        {
            get => m_scale;
            set { m_scale = value; BasicIUserMod.Instance.RequireRunCoroutine("OnChangeMatrixData", OnChangeMatrixData()); }
        }

        public virtual IEnumerator OnChangeMatrixData()
        {
            yield return null;
            for (int i = 0; i < 32; i++)
            {
                RenderManager.instance.UpdateGroups(i);
                yield return null;
            }
        }

        public abstract PrefabInfo TargetAssetParameter { get; }
        public abstract TextRenderingClass RenderingClass { get; }
        public abstract string DescriptorOverrideFont { get; }
        [XmlIgnore]
        public string lastLayoutVersion = null;
        [XmlAttribute("WE_layoutVersion")]
        public string LayoutVersion
        {
            get => BasicIUserMod.FullVersion;
            set => lastLayoutVersion = value;
        }
    }

}
