using UIFramework.Core;
using UIFramework.Window;
using UnityEngine;

namespace UIFramework.Window
{
    /// <summary>
    /// 窗口的通用属性
    /// </summary>
    [System.Serializable]
    public class WindowProperties : IWindowProperties
    {
        [SerializeField]
        protected WindowPriority windowQueuePriority = WindowPriority.ForceForeground;

        [SerializeField]
        protected bool hideOnForeground = true;

        [SerializeField]
        protected bool isPopUp = false;

        public WindowProperties()
        {
            hideOnForeground = true;
            windowQueuePriority = WindowPriority.ForceForeground;
            isPopUp = false;
        }

        /// <summary>
        /// 如果另一个窗口已经打开，此窗口如何表现？
        /// </summary>
        /// value:Foreground 立即打开，Enqueue会将其纳入队列，在当前窗口关闭后打开
        public WindowPriority WindowQueuePriority
        {
            get { return windowQueuePriority; }
            set { windowQueuePriority = value; }
        }
        
        /// <summary>
        /// 当其他窗口被置前时，自己是否被隐藏
        /// </summary>
        public bool HideOnForegroundLost
        {
            get { return hideOnForeground; }
            set { hideOnForeground = value; }   
        }

        /// <summary>
        /// 当在Open调用中传递属性时，是否应该覆盖在viewPrefab中配置的属性
        /// </summary>
        public bool SuppressPrefabProperties { get; set; }

        /// <summary>
        /// 弹出窗口在他们后面显示一个黑色背景，并在所有窗口之前显示
        /// </summary>
        public bool IsPopUp
        {
            get { return isPopUp; }
            set { isPopUp = value; }
        }

        public WindowProperties(bool suppressPrefabProperties = false)
        {
            WindowQueuePriority = WindowPriority.ForceForeground;
            HideOnForegroundLost = false;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        public WindowProperties(WindowPriority priority, bool hideOnForegroundLost = false,bool suppressPrefabProperties =false)
        {
            WindowQueuePriority = priority;
            HideOnForegroundLost = hideOnForegroundLost;
            SuppressPrefabProperties = suppressPrefabProperties;
        }
    }
}
