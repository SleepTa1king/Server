using UnityEngine;
using System;

namespace UIFramework.ViewAnimation
{
    /// <summary>
    /// 界面动画组件
    /// </summary>
    public abstract class AniComponent:MonoBehaviour
    {
        /// <summary>
        /// 动画播放，当执行完回调callWhenFinished
        /// </summary>
        /// <param name="target"></param>
        /// <param name="callWhenFinished"></param>
        public abstract void Animate(Transform target, Action callWhenFinished);
    }
}
