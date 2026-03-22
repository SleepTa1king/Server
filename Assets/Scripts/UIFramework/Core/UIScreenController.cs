using System;
using System.ComponentModel;
using UnityEngine;
using UIFramework.ViewAnimation;

namespace UIFramework.Core
{
    /// <summary>
    /// UI面板的基类，窗口，面板这些都继承他，例如AIWindowController，PanelController
    /// </summary>
    /// <typeparam name="TProps"></typeparam>
    public abstract class UIScreenController<TProps> : MonoBehaviour, IScreenController where TProps : IScreenProperties
    {
        [Header("Screen Animations")]
        [Tooltip("界面显示的动画")]
        [SerializeField]
        private AniComponent animIn;

        [Tooltip("界面隐藏的动画")]
        [SerializeField]
        private AniComponent animOut;

        [Header("Screen Properties")]
        [Tooltip("界面的参数属性")]
        [SerializeField]
        private TProps properties;

        /// <summary>
        /// 界面Id，用字符串形式保存
        /// </summary>
        public string ScreenId { get; set; }

        /// <summary>
        /// 动画组件，确保界面有统一的弹出效果
        /// </summary>
        public AniComponent AnimIn
        {
            get { return animIn; }
            set { animIn = value; }
        }

        /// <summary>
        /// 动画组件，确保界面有统一的隐藏效果
        /// </summary>
        public AniComponent AnimOut
        {
            get { return animOut; }
            set { animOut = value; }
        }

        /// <summary>
        /// 弹出（渐入）动画时回调
        /// </summary>
        public Action<IScreenController> InTransitionFinished { get; set; }
        /// <summary>
        /// 隐藏（渐出）动画时回调
        /// </summary>
        public Action<IScreenController> OutTransitionFinished { get; set; }
        /// <summary>
        /// 界面关闭时回调
        /// </summary>
        public Action<IScreenController> ScreenClosed { get; set; }
        /// <summary>
        /// 界面销毁时回调
        /// </summary>
        public Action<IScreenController> ScreenDestroyed { get; set; }

        /// <summary>
        /// 界面是否显示中
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 界面的参数属性
        /// </summary>
        public TProps Properties
        {
            get { return properties;}
            set { properties = value; }
        }

     

        protected virtual void Awake()
        {
            AddListeners();
        }

        protected virtual void OnDestroy()
        {
            if(ScreenDestroyed != null)
            {
                ScreenDestroyed?.Invoke(this);
            }
            InTransitionFinished = null;
            OutTransitionFinished = null;
            ScreenClosed = null;
            ScreenDestroyed = null;
            RemoveListeners();

        }

        /// <summary>
        /// 监听事件，由Awake自动调用
        /// </summary>
        protected virtual void AddListeners()
        {

        }

        /// <summary>
        /// 取消监听事件，当界面销毁时调用
        /// </summary>
        protected virtual void RemoveListeners()
        {

        }

        /// <summary>
        /// 当属性参数设置到面板时触发，在SetProperties时触发，比较安全地取到值
        /// </summary>
        protected virtual void OnPropertiesSet()
        {

        }

        /// <summary>
        /// 界面关闭时触发
        /// </summary>
        protected virtual void WhileHiding()
        {

        }

        /// <summary>
        /// 设置属性参数
        /// </summary>
        /// <param name="property"></param>
        protected virtual void SetProperties(TProps property)
        {
            this.properties = property;
        }

        /// <summary>
        /// 显示的时候处理一些层级，或者处理属性等，具体看继承者重写
        /// </summary>
        protected virtual void HierarchyFixOnShow()
        {

        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <param name="animate"></param>
        public void Hide(bool animate = true)
        {
            DoAnimation(animate ? animOut : null, OnTransitionOutFinished, false);
            WhileHiding();
        }

        /// <summary>
        /// 显示界面
        /// </summary>
        /// <param name="props"></param>
        public void Show(IScreenProperties props = null)
        {
            if(props != null)
            {
                if(props is TProps)
                {
                    SetProperties((TProps)props);
                }
                else
                {
                    Debug.Log($"Properties passed have wrong type! {props.GetType()} + instead of {typeof(TProps)}");
                    return;
                }
            }

            HierarchyFixOnShow();
            OnPropertiesSet();

            if(!gameObject.activeSelf)
            {
                DoAnimation(animIn, OnTransitionInFinished, true);
            }
            else
            {
                if(InTransitionFinished!=null)
                {
                    InTransitionFinished?.Invoke(this);
                }
            }
        }

        private void DoAnimation(AniComponent caller,Action callWhenFinished,bool isVisible)
        {
            if(caller == null)
            {
                gameObject.SetActive(isVisible);
                if(callWhenFinished != null)
                {
                    callWhenFinished?.Invoke();
                }
            }
            else
            {
                if(isVisible && !gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

                caller.Animate(transform, callWhenFinished);
            }
        }

        private void OnTransitionInFinished()
        {
            IsVisible = true;
            InTransitionFinished?.Invoke(this);
        }

        private void OnTransitionOutFinished()
        {
            IsVisible = false;
            gameObject.SetActive(false);
            OutTransitionFinished?.Invoke(this);
        }
    }
}
