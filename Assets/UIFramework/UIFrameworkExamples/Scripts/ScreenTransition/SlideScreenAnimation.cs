using DG.Tweening;
using System;
using UIFramework;
using UIFramework.ViewAnimation;
using UnityEngine;

public class SlideScreenAnimation : AniComponent
{
    public enum Position
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 4,
    }

    [SerializeField]
    protected Position origin = Position.Left;
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

    public Position Origin
    {
        get { return origin; }
        set { origin = value; }
    }

    public override void Animate(Transform target, Action callWhenFinished)
    {
        RectTransform rectTransform = target as RectTransform;
        var originAnchoredPos = rectTransform.anchoredPosition;
        Vector3 startPosition = Vector3.zero;

        switch(origin)
        {
            case Position.Left:
                startPosition = new Vector3(-rectTransform.rect.width, 0.0f, 0.0f);
                break;
            case Position.Right:
                startPosition = new Vector3(rectTransform.rect.width, 0.0f, 0.0f);
                break;
            case Position.Top:
                startPosition = new Vector3(0.0f, rectTransform.rect.height, 0.0f);
                break;
            case Position.Bottom:
                startPosition = new Vector3(0.0f, -rectTransform.rect.height, 0.0f);
                break;
        }
        rectTransform.anchoredPosition = isOutAnimation ? Vector3.zero : startPosition;

        rectTransform.DOKill();

        CanvasGroup canvasGroup = null;
        if(doFade)
        {
            canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if(canvasGroup == null)
            {
                canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.DOFade(isOutAnimation ? 0f : 1f, duration * fadeDurationPercent);
        }

        rectTransform.DOAnchorPos(isOutAnimation ? startPosition : Vector3.zero,duration,true).SetEase(ease).OnComplete(
           () =>
           {
               callWhenFinished();
               rectTransform.anchoredPosition = originAnchoredPos;
           if (canvasGroup != null)
               {
                   canvasGroup.alpha = 1f;
               }

           } ).SetUpdate(true);
    }
}

