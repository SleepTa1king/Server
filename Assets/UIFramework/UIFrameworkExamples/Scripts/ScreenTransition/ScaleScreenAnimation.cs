using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIFramework.ViewAnimation;
using UnityEngine;

public class ScaleScreenAnimation :AniComponent
{
    [SerializeField]
    protected bool isOutAnimation;
    [SerializeField]
    protected float duration = 0.5f;
    [SerializeField] 
    protected bool doFade;
    [SerializeField]
    protected float fadeDurationPercent = 0.5f;
    [SerializeField]
    protected Ease ease = Ease.Linear;
    [SerializeField]
    [Range(0f, 1f)]
    protected float xYSplit = 0.25f;

    public override void Animate(Transform target, Action callWhenFinished)
    {
        RectTransform rectTransform = target as RectTransform;
        CanvasGroup canvasGroup = null;
        if(doFade)
        {
            canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if(canvasGroup == null)
            {
                canvasGroup=rectTransform.gameObject.GetComponent<CanvasGroup>();
            }
            canvasGroup.DOFade(isOutAnimation ? 0f:1f,duration*fadeDurationPercent);
        }

        rectTransform.DOKill();
        if(isOutAnimation)
        {
            rectTransform.DOScale(0f, duration).SetEase(ease)
            .OnComplete(()=>Cleanup(callWhenFinished,rectTransform,canvasGroup))
            .SetUpdate(true);
        }
        else
        {
            Sequence scaleSequence = DOTween.Sequence();
            scaleSequence.SetUpdate(true);
            rectTransform.localScale = new Vector3(0f, 0.02f, 0f);

            var xScale = rectTransform.DOScaleX(1f, duration * xYSplit).SetEase(ease);
            var yScale = rectTransform.DOScaleY(1f, duration * 1f - xYSplit).SetEase(ease);
            scaleSequence.Append(xScale).Append(yScale).OnComplete(
            () => Cleanup(callWhenFinished, rectTransform, canvasGroup)).SetUpdate(true);

            scaleSequence.Play();
        }
    }

    private void Cleanup(Action callWhenFinished,RectTransform rectTransform,CanvasGroup canvasGroup)
    {
        callWhenFinished?.Invoke();
        rectTransform.localScale = Vector3.one;
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

}

