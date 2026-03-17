using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Animator")]
[RequireComponent(typeof(Animator))]
public class UIAnimator : MonoBehaviour
{
    public UnityEvent OnShow;

    public UnityEvent OnHide;

    public bool hideOnAwake;

    public string normalTrigger = "Normal";
    public string showTrigger = "Show";
    public string hideTrigger = "Hide";

    protected Animator m_animator;

    protected virtual void Awake()
    {
        m_animator = GetComponent<Animator>();
        if(hideOnAwake)
        {
            m_animator.Play(hideTrigger, 0, 1);
        }
    }
    public virtual void Show()
    {
        m_animator.SetTrigger(showTrigger);
        OnShow?.Invoke();
    }

    public virtual void Hide()
    {
        m_animator.SetTrigger(hideTrigger);
        OnHide?.Invoke();
    }

    public virtual void SetActive(bool value) => gameObject.SetActive(value);
}

