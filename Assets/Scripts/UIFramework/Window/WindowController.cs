using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Window
{
    /// <summary>
    /// 窗口管理类
    /// </summary>
    public abstract class WindowController : WindowController<WindowProperties>
    {
    }

    public abstract class WindowController<Tprops> : UIScreenController<Tprops>,IWindowController where Tprops:IWindowProperties
    {
        public bool HideOnForegroundLost
        {
            get
            { return Properties.HideOnForegroundLost; }
        }

        public bool IsPopUp
        {
            get { return Properties.IsPopUp; }
        }

        public WindowPriority WindowPriority
        {
            get { return Properties.WindowQueuePriority; }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void UI_Close()
        {
            ScreenClosed?.Invoke(this);
        }

        protected sealed override void SetProperties(Tprops property)
        {
            if(property !=null)
            {
                if(!property.SuppressPrefabProperties)
                {
                    property.HideOnForegroundLost = this.Properties.HideOnForegroundLost;
                    property.WindowQueuePriority = this.Properties.WindowQueuePriority;
                    property.IsPopUp = this.Properties.IsPopUp;
                }
                this.Properties = property;
            }
        }

        protected override void HierarchyFixOnShow()
        {
            transform.SetAsLastSibling();
        }
    }
}
