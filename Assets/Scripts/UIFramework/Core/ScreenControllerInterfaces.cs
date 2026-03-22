using System;
using UIFramework.Panel;
using UIFramework.Window;
using UnityEngine;

namespace UIFramework.Core
{
    /// <summary>
    /// 所有UI界面需要实现的接口
    /// </summary>
    public interface IScreenController
    {
        bool IsVisible { get; set; } 
        string ScreenId { get;set; }
        void Show(IScreenProperties screenProps = null);
        void Hide(bool animate = true);

        Action<IScreenController> InTransitionFinished { get; set; }
        Action<IScreenController> OutTransitionFinished { get; set; }
        Action<IScreenController> ScreenClosed { get; set; }
        Action<IScreenController> ScreenDestroyed { get; set; }
    }

    /// <summary>
    /// 所有面板必须实现的接口
    /// </summary>
    public interface IPanelController : IScreenController
    {
        PanelPriority Priority { get; }
    }

    /// <summary>
    /// 所有窗口必须实现的接口
    /// </summary>
    public interface IWindowController : IScreenController
    {
        bool HideOnForegroundLost { get; }
        bool IsPopUp { get; }
        WindowPriority WindowPriority { get; }
    }
}
