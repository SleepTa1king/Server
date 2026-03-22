using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.ViewAnimation
{
    /// <summary>
    /// 具体动画实现
    /// </summary>
    public class AnimationView :AniComponent
    {
        [SerializeField]
        private AnimationClip clip = null;
        [SerializeField]
        private bool playReverse = false;

        private Action previousCallbackWhenFinished;

        public override void Animate(Transform target, Action callWhenFinished)
        {
            FinishPrevious();
            var targetAnimation = target.GetComponent<Animation>();
            if(targetAnimation == null)
            {
                Debug.LogError($"[LegacyAnimationScreenTransition] No Animation component in{target}");
                if(callWhenFinished != null)
                {
                    callWhenFinished();
                }
                return;
            }

            targetAnimation.clip = clip;
            StartCoroutine(PlayAnimationRoutine(targetAnimation,callWhenFinished)); 
        }

        private IEnumerator PlayAnimationRoutine(Animation targetAnimation,Action callWhenFinished)
        {
            previousCallbackWhenFinished = callWhenFinished;
            foreach(AnimationState state in targetAnimation)
            {
                state.time = playReverse ? state.clip.length : 0f;
                state.speed = playReverse ? -1f : 1f;
            }

            targetAnimation.Play(PlayMode.StopAll);
            yield return new WaitForSeconds(targetAnimation.clip.length);
            FinishPrevious();
        }

        private void FinishPrevious()
        {
            if(previousCallbackWhenFinished != null)
            {
                previousCallbackWhenFinished.Invoke();
                previousCallbackWhenFinished = null;
            }

            StopAllCoroutines();
        }
    }

    // 假设 AniComponent 继承自 MonoBehaviour
    //public class AnimationView_dt : AniComponent
    //{
    //    [SerializeField]
    //    private string triggerName = "PlayAnimation";
    //    [SerializeField]
    //    private bool playReverse = false;

    //    // 使用 Tween 句柄来管理动画中断和回调
    //    private Tween currentTween;

    //    public override void Animate(Transform target, Action callWhenFinished)
    //    {
    //        // 1. 停止并完成旧动画（与 FadeAnimation 的逻辑保持一致）
    //        // 任何正在运行的旧动画都会被立即完成，并触发其回调
    //        if (currentTween != null && currentTween.IsActive())
    //        {
    //            currentTween.Complete(true);
    //            StopAllCoroutines(); // 确保旧的等待协程也停止
    //        }

    //        var targetAnimator = target.GetComponent<Animator>();
    //        if (targetAnimator == null)
    //        {
    //            Debug.LogError($"[DOTween AnimationView] No Animator component in {target}");
    //            callWhenFinished?.Invoke();
    //            return;
    //        }

    //        string finalTrigger = playReverse ? triggerName + "Reverse" : triggerName;

    //        // 2. 触发 Animator 动画
    //        targetAnimator.SetTrigger(finalTrigger);

    //        // 3. 创建一个**虚拟的** DOTween 补间来管理回调和中断
    //        // 我们可以使用一个 DOComplete 补间，它不会对 Animator 产生影响，
    //        // 仅仅充当一个 IDoTween 句柄，时长可以设为 0.1f 只是为了激活它，
    //        // 但更好的方法是利用 DOTween 的静态方法 DOTween.To
    //        currentTween = DOTween.To(
    //                () => 0f, // getter
    //                (x) => { }, // setter (不执行任何操作)
    //                1f,       // 目标值（无意义）
    //                float.MaxValue // 持续时间，设置为最大值，保证它不会自动完成
    //            )
    //            .SetAutoKill(false) // 避免 DOTween 自动回收
    //            .SetUpdate(UpdateType.Normal, true) // 确保它在 MonoBehaviour 停止时也能被 DOTween 追踪
    //            .OnComplete(() =>
    //            {
    //                // 当我们手动调用 currentTween.Complete(true/false) 时，会执行此回调
    //                callWhenFinished?.Invoke();
    //                currentTween = null;
    //            });

    //        // 4. 启动协程等待 Animator 状态播放完成
    //        StartCoroutine(WaitForAnimatorStateCompletion(targetAnimator, finalTrigger, currentTween));
    //    }

    //    private IEnumerator WaitForAnimatorStateCompletion(Animator animator, string trigger, Tween tween)
    //    {
    //        // *** 这里的等待逻辑是关键，必须正确获取动画时长 ***
    //        float clipDuration = 1.0f; // 示例值，实际项目中需获取动画剪辑时长

    //        // 复杂的逻辑：等待 Animator 的状态机切换到指定状态，并获取其播放时长
    //        // ... (此处省略了复杂的 Animator 状态检查代码) ...

    //        // 简单模拟：等待指定时长
    //        yield return new WaitForSeconds(clipDuration);

    //        // 动画播放完成后，手动完成 DOTween 句柄
    //        if (tween != null && tween.IsActive())
    //        {
    //            // Complete(false) 执行回调，但不跳转时间（动画已完成）
    //            tween.Complete(false);
    //        }
    //    }

    //    // FinishPrevious 方法只需确保中断
    //    private void FinishPrevious()
    //    {
    //        // 立即完成虚拟 Tween 并触发回调
    //        if (currentTween != null && currentTween.IsActive())
    //        {
    //            currentTween.Complete(true);
    //        }
    //        // 停止等待的协程
    //        StopAllCoroutines();
    //    }
    //}
}
