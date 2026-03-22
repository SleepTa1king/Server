using UnityEngine;
using System;
using DG.Tweening;

namespace UIFramework.ViewAnimation
{
    /// <summary>
    /// 渐入动画，可用dotween
    /// </summary>
    public class FadeAnimation:AniComponent
    {
        [SerializeField]
        private float fadeDuration = 0.5f;
        [SerializeField]
        private bool fadeOut = false;

        private CanvasGroup canvasGroup;
        private float timer;
        private Action currentAction;
        private Transform currentTarget;

        private float starValue;
        private float endValue;
        private bool shouldAnimate;

        public override void Animate(Transform target, Action callWhenFinished)
        {
           if(currentAction !=null)
            {
                canvasGroup.alpha = endValue;
                Debug.Log(canvasGroup.alpha);
                currentAction.Invoke();
            }

            canvasGroup = target.GetComponent<CanvasGroup>();
            if(canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            if(fadeOut)
            {
                starValue = 1f;
                endValue = 0f;
            }

            else
            {
                starValue = 0f;
                endValue = 1f;
            }

            currentAction = callWhenFinished;
            timer = fadeDuration;
            Debug.Log(canvasGroup.alpha);
            canvasGroup.alpha = starValue;
            shouldAnimate = true;
        }

        private void Update()
        {
            if(!shouldAnimate)
            {
                return;
            }

            if(timer > 0f)
            {
                timer -= Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(endValue,starValue,timer/fadeDuration);
            }
            else
            {
                canvasGroup.alpha = 1f;
                if(currentAction != null)
                {
                    currentAction();
                }
                currentAction = null;
                shouldAnimate = false;
            }
            
        }

    }
    //// 假设 AniComponent 是一个 MonoBehaviour，以便使用 Coroutines 或 Update，
    //// 但 DOTween 动画本身不需要继承 MonoBehaviour 来运行。
    //public class FadeAnimation_dt : AniComponent
    //{
    //    [SerializeField]
    //    private float fadeDuration = 0.5f;
    //    [SerializeField]
    //    private bool fadeOut = false;

    //    // 存储当前动画Tween，用于在开始新动画时停止旧动画
    //    private Tween currentTween;

    //    // CanvasGroup 变量仍然保留，但可以只在 Animate 中局部使用
    //    // private CanvasGroup canvasGroup; 

    //    // 省略了 timer, currentAction, starValue, endValue, shouldAnimate, currentTarget 等手动管理动画的状态变量

    //    public override void Animate(Transform target, Action callWhenFinished)
    //    {
    //        // 1. 停止并完成当前正在进行的动画
    //        // 这比原始代码更平滑：DOTween 的 Kill(true) 会立即完成并调用所有回调，
    //        // 或者 Kill() 直接停止（如果不需要立即完成旧动画，也可以只用 Kill()）。
    //        // 如果你需要像原代码那样“立即完成”旧动画，使用 currentTween.Complete()。
    //        // 为了保持逻辑一致性，我们选择 Complete()。
    //        if (currentTween != null && currentTween.IsActive())
    //        {
    //            // Complete(true) 立即跳转到终点并触发 OnComplete 回调
    //            currentTween.Complete(true);
    //            // 注意：如果原代码的 currentAction 是上一个动画的回调，
    //            // 那么使用 Complete(true) 即可触发它。这里我们假设 DOTween 的回调机制能取代它。
    //        }

    //        // 2. 获取 CanvasGroup 组件 (与原始代码逻辑相同)
    //        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
    //        if (canvasGroup == null)
    //        {
    //            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
    //        }

    //        // 3. 确定目标 alpha 值
    //        float targetAlpha = fadeOut ? 0f : 1f;

    //        // 4. 使用 DOTween 创建和启动动画
    //        // .DOFade() 是 CanvasGroup 专用的扩展方法，可以将 alpha 值在指定时间内平滑过渡到 targetAlpha。
    //        currentTween = canvasGroup.DOFade(targetAlpha, fadeDuration)
    //            // 设置动画完成后执行的回调函数
    //            .OnComplete(() =>
    //            {
    //                // 动画完成后，调用外部传入的 Action
    //                callWhenFinished?.Invoke();
    //                currentTween = null; // 动画结束，清除 Tween 引用
    //            })
    //            // 可以选择性地添加 Easeing (动画曲线)
    //            .SetEase(Ease.Linear); // 保持线性，或选择 Ease.InOutQuad 等更自然的效果

    //        // 5. 将 CanvasGroup 的 alpha 立即设置为起始值 (可选，DOFade 会从当前值开始)
    //        // 为了和原代码的 canvasGroup.alpha = starValue; 保持一致，我们可以这样做：
    //        // canvasGroup.alpha = fadeOut ? 1f : 0f; 
    //        // 但通常 DOTween 会直接从组件的当前 alpha 值开始过渡，无需手动设置起始值。
    //    }
    //}

}
