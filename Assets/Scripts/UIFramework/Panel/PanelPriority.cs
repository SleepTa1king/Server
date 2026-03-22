using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Panel
{
    /// <summary>
    /// 规定面板属于哪个层
    /// </summary>
    public enum PanelPriority
    {
        None = 0,
        Priority = 1,
        Tutorial = 2,
        Blocker = 3,
    }

    [System.Serializable]
    public class PanelPriorityLayerEntry
    {

        [SerializeField]
        [Tooltip("指定面板层的优先级")]
        private PanelPriority priority;

        [SerializeField]
        [Tooltip("此优先级下所有面板的父节点")]
        private Transform targetParent;

        public PanelPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }
        public Transform TargetParent
        {
            get { return targetParent; }
            set { targetParent = value; }
        }

        PanelPriorityLayerEntry(PanelPriority priority,Transform parent)
        {
            this.priority = priority;
            this.targetParent = parent;
        }
    }

    [System.Serializable]
    public class PanelPriorityLayerList
    {
        [SerializeField]
        [Tooltip("根据面板优先级查找并存储对应GameObject。渲染优先级由这些GameObject在层级结构中的顺序决定")]

        private List<PanelPriorityLayerEntry> paraLayers = null;

        private Dictionary<PanelPriority, Transform> lookUp;

        public Dictionary<PanelPriority, Transform> ParaLayerLookUp
        {
            get
            {
                if(lookUp == null || lookUp.Count == 0)
                {
                    CacheLookUp();
                }

                return lookUp;
            }
          
        }

        private void CacheLookUp()
        {
            lookUp = new Dictionary<PanelPriority, Transform>();
            for(int i=0;i<paraLayers.Count;i++)
            {
                lookUp.Add(paraLayers[i].Priority, paraLayers[i].TargetParent);
            }
        }

        public PanelPriorityLayerList(List<PanelPriorityLayerEntry> entries)
        {
            paraLayers = entries;
        }
    }
}
