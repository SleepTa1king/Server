using UIFramework.Core;
using UnityEngine;

namespace UIFramework.Panel
{
    /// <summary>
    /// 面板属性类
    /// </summary>
    [System.Serializable]
    public class PanelProperties : IPanelProperties
    {
        [SerializeField]
        [Tooltip("面板根据其优先级进入不同的副层级，可以再“面板层级”设置副层级。")]
        private PanelPriority priority;

        public PanelPriority Priority
        {
            get { return priority; }
            set { priority = value; }   
        }
    }
}
