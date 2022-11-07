using ICities;
using Kwytto.Data;
using Kwytto.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Singleton;

namespace WriteEverywhere.Data
{

    [XmlRoot("OnNetData")]
    public class WTSOnNetData : DataExtensionBase<WTSOnNetData>
    {
        [XmlIgnore]
        public WriteOnNetGroupXml[] m_boardsContainers = new WriteOnNetGroupXml[NetManager.MAX_SEGMENT_COUNT];
        [XmlElement("BoardContainers")]
        public SimpleNonSequentialList<WriteOnNetGroupXml> BoardContainersExport
        {
            get
            {
                var res = new SimpleNonSequentialList<WriteOnNetGroupXml>();
                for (int i = 0; i < m_boardsContainers.Length; i++)
                {
                    if (m_boardsContainers[i] != null && m_boardsContainers[i].HasAnyBoard())
                    {
                        if ((NetManager.instance.m_segments.m_buffer[i].m_flags & NetSegment.Flags.Created) != 0)
                        {
                            res[i] = m_boardsContainers[i];
                        }
                        else
                        {
                            m_boardsContainers[i] = null;
                        }
                    }
                }
                return res;
            }

            set
            {
                LoadDefaults(null);
                foreach (var kv in value.Keys)
                {
                    m_boardsContainers[kv] = value[kv];
                }
            }
        }

        public override string SaveId => "K45_WE_OnNetData";



        public override WTSOnNetData LoadDefaults(ISerializableData serializableData)
        {
            base.LoadDefaults(serializableData);
            m_boardsContainers = new WriteOnNetGroupXml[NetManager.MAX_SEGMENT_COUNT];
            return null;
        }

        [XmlAttribute("defaultFont")]
        public virtual string DefaultFont { get; set; }

        public void OnSegmentChanged(ushort segmentId)
        {
            clearCacheQueue.Add(segmentId);

            if (currentCacheCoroutine is null)
            {
                currentCacheCoroutine = ModInstance.Controller?.StartCoroutine(ClearCacheQueue());
            }
        }
        private readonly HashSet<ushort> clearCacheQueue = new HashSet<ushort>();
        private Coroutine currentCacheCoroutine;
        private IEnumerator ClearCacheQueue()
        {
            do
            {
                var list = clearCacheQueue.ToList();
                foreach (var segmentId in list)
                {
                    if (BoardContainersExport.TryGetValue(segmentId, out WriteOnNetGroupXml descriptorXml))
                    {
                        foreach (var board in descriptorXml.BoardsData)
                        {
                            board.m_cachedPositions = null;
                            board.m_cachedRotations = null;
                        }
                    }
                    WTSCacheSingleton.ClearCacheSegmentSize(segmentId);
                    WTSCacheSingleton.ClearCacheSegmentNameParam(segmentId);
                    clearCacheQueue.Remove(segmentId);
                    yield return 0;
                }
            } while (clearCacheQueue.Count > 0);
            currentCacheCoroutine = null;
        }
    }

}
