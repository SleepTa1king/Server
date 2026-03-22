
using UIFramework.Panel;
using UIFramework.Window;

namespace UIFramework.Core
{
    /// <summary>
    /// 界面属性接口
    /// </summary>
    public interface IScreenProperties
    {
    }

    public interface IPanelProperties:IScreenProperties
    {
        PanelPriority Priority { get; set; }
    }

    /// <summary>
    /// 窗口属性接口
    /// </summary>
    public interface IWindowProperties:IScreenProperties
    {
        WindowPriority WindowQueuePriority { get; set; }
        bool HideOnForegroundLost { get; set; }
        bool IsPopUp { get; set; }
        bool SuppressPrefabProperties { get; set; }

    }
}
