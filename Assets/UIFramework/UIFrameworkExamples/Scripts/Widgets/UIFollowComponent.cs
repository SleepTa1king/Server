using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI跟随组件，仅在模板UI元素锚定到左下角才有效
/// </summary>
public class UIFollowComponent : MonoBehaviour
{
    [SerializeField]
    private Text label = null;
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private bool clampAtBorders = true;
    [SerializeField]
    private bool rotateWhenClamped = true;
    [SerializeField]
    private RectTransform rotatingElement = null;


    public event Action<UIFollowComponent> LabelDestroyed;
    private Transform currentFollow;
    private RectTransform rectTransform;
    private CanvasScaler parentScaler;
    private RectTransform mainCanvasRectTransform;

    private void Start()
    {
        mainCanvasRectTransform = transform.root as RectTransform;
        rectTransform = transform as RectTransform;
        parentScaler = mainCanvasRectTransform.GetComponent<CanvasScaler>();

        if (rotatingElement == null)
        {
            rotatingElement = rectTransform;
        }
    }

    private void OnDestroy()
    {
        if(LabelDestroyed != null)
        {
            LabelDestroyed?.Invoke(this);
        }
    }

    public static Vector2 GetAnchoredPosition(Camera viewingCamera,Transform followTransform,CanvasScaler canvasScaler,Rect followElementRect)
    {
        var relativePositon = viewingCamera.transform.InverseTransformPoint(followTransform.position);
        relativePositon.z = Mathf.Max(relativePositon.z, 1f);
        var vieportPos = viewingCamera.WorldToViewportPoint(viewingCamera.transform.TransformPoint(relativePositon));

        return new Vector2(vieportPos.x * canvasScaler.referenceResolution.x - followElementRect.size.x / 2f,
                           vieportPos.y * canvasScaler.referenceResolution.y - followElementRect.size.y / 2f);
    }

    public static Vector2 GetClampedOnScreenPosition(Vector2 onScreenPosition,Rect followElementRect,RectTransform mainCanvasRectTransform)
    {
        return new Vector2(Mathf.Clamp(onScreenPosition.x, 0f, mainCanvasRectTransform.sizeDelta.x - followElementRect.size.x),
                           Mathf.Clamp(onScreenPosition.y, 0f, mainCanvasRectTransform.sizeDelta.y - followElementRect.size.y));
    }

    public void UpdatePoisiton(Camera camera)
    {
        if(currentFollow != null)
        {
            var onScreenPosition = GetAnchoredPosition(camera, currentFollow.transform, parentScaler, rectTransform.rect);
            if(!clampAtBorders)
            {
                rectTransform.anchoredPosition = onScreenPosition;
                return;
            }

            var clampPosition = GetClampedOnScreenPosition(onScreenPosition, rectTransform.rect, mainCanvasRectTransform);
            rectTransform.anchoredPosition = clampPosition;

            if(!rotateWhenClamped)
            {
                return;
            }

            if (onScreenPosition != clampPosition)
            {
                var delta = clampPosition - onScreenPosition;
                var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                rotatingElement.localRotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
            else
            {
                rotatingElement.localRotation = Quaternion.identity;  
            }
        }
    }
    public void SetFollow(Transform toFollow)
    {
        currentFollow = toFollow;
    }

    public void SetText(string label)
    {
        this.label.text = label;
    }

    public void SetIcon(Sprite icon)
    {
        this.icon.sprite = icon;
    }
}

