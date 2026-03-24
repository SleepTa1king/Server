using UnityEngine;
using UIFramework.Panel;
using UnityEngine.UI;
using UIFramework.Core;
using UIFramework.Window;
using System;

namespace UIFramework
{
    public class UIFrame : MonoBehaviour
    {
        [Tooltip("如果你想手动初始化此UI框架，请将其设置为False")]
        [SerializeField]
        private bool initializeOnAwake = true;

        private PanelUILayer panelLayer;
        private WindowUILayer windowLayer;
        private Canvas mainCanvas;
        private GraphicRaycaster graphicRaycaster;

        /// <summary>
        /// 主Canvas
        /// </summary>
        public Canvas MainCanvas
        {
            get
            {
                if(mainCanvas == null)
                {
                    mainCanvas = GetComponent<Canvas>();
                }
                return mainCanvas;
            }
        }

        /// <summary>
        /// 主Canvas的相机
        /// </summary>
        public Camera MainCamera
        {
            get
            {
                return mainCanvas.worldCamera;
            }
        }

        private void Awake()
        {
            if(initializeOnAwake)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize()
        {
            if(panelLayer == null)
            {
                panelLayer = gameObject.GetComponentInChildren<PanelUILayer>(true);
                if(panelLayer == null)
                {
                    Debug.Log($"[UI Frame] UI Frame lacks PanelLayer");
                }
                else
                {
                    panelLayer.Initialize();
                }
            }
            

            if(windowLayer == null)
            {
                windowLayer = gameObject.GetComponentInChildren<WindowUILayer>(true);
                if (windowLayer == null)
                {
                    Debug.Log($"[UI Frame] UI Frame lacks WindowLayer");
                }

                else
                {
                    windowLayer.Initialize();
                    windowLayer.RequestScreenBlock += OnRequestScreenBlock;
                    windowLayer.RequestScreenUnBlock += OnRequestScreenUnBlock;
                }
            }
            
            graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 显示面板
        /// </summary>
        /// <param name="screenId"></param>
        public void ShowPanel(string screenId)
        {
            panelLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// 通过Id和属性显示面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="properties"></param>
        public void ShowPanel<T>(string screenId,T properties) where T :IPanelProperties
        {
            panelLayer.ShowScreenById<T>(screenId, properties);
        }

        /// <summary>
        /// 通过Id隐藏面板
        /// </summary>
        /// <param name="screenId"></param>
        public void HidePanel(string screenId)
        {
            panelLayer.HideScreenById(screenId);
        }

        /// <summary>
        /// 通过Id关闭窗口
        /// </summary>
        /// <param name="screenId"></param>
        public void CloseWindow(string screenId)
        {
            windowLayer.HideScreenById(screenId);
        }

        /// <summary>
        /// 通过Id显示窗口
        /// </summary>
        /// <param name="screenId"></param>
        public void OpenWindow(string screenId)
        {
            windowLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// 通过Id和参数显示窗口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="properties"></param>
        public void OpenWindow<T>(string screenId,T properties) where T:IWindowProperties
        {
            windowLayer.ShowScreenById<T>(screenId, properties);
        }

        public void CloseCurrentWindow()
        {
            if(windowLayer.CurrentWindow!= null)
            {
                CloseWindow(windowLayer.CurrentWindow.ScreenId);
            }
        }

        /// <summary>
        /// 二次包装，给id就显示
        /// </summary>
        /// <param name="screenId"></param>
        public void ShowScreen(string screenId)
        {
            Type type;
            if(IsScreenRegistered(screenId,out type))
            {
                if (type == typeof(IWindowController))
                    OpenWindow(screenId);
                else if (type == typeof(IPanelController))
                    ShowPanel(screenId);
            }
            else
            {
                Debug.LogError(string.Format($"Tried to open Screen id {screenId} but it's not registered as Window or Panel"));
            }
        }

        /// <summary>
        /// 注册一个界面，如果传入了screenTransform，就相当于制定了父节点
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="controller"></param>
        /// <param name="screenTransform"></param>
        public void RegisterScreen(string screenId,IScreenController controller,Transform screenTransform)
        {
            IWindowController window = controller as IWindowController;
            if(window != null)
            {
                windowLayer.RegiserScreen(screenId, window);
                if(screenTransform !=null)
                {
                    windowLayer.ReparentScreen(window, screenTransform);
                }
                return;
            }
            IPanelController panel = controller as IPanelController;
            if(panel != null)
            {
                panelLayer.RegiserScreen(screenId, panel);
                if(screenTransform != null)
                {
                    panelLayer.ReparentScreen(controller, screenTransform);
                }
            }
        }

        /// <summary>
        /// 注册一个面板
        /// </summary>
        /// <typeparam name="TPanel"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="controller"></param>
        public void RegisterPanel<TPanel>(string screenId, TPanel controller) where TPanel:IPanelController
        {
            panelLayer.RegiserScreen(screenId, controller);
        }

        /// <summary>
        /// 注销面板
        /// </summary>
        /// <typeparam name="TPanel"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="controller"></param>
        public void UnRegisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController
        {
            panelLayer.UnRegisteredScreen(screenId, controller);
        }

        /// <summary>
        /// 注册一个窗口
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="controller"></param>
        public void RegisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            windowLayer.RegiserScreen(screenId, controller);
        }

        /// <summary>
        /// 注销一个窗口
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <param name="screenId"></param>
        /// <param name="controller"></param>
        public void UnRegisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            windowLayer.UnRegisteredScreen(screenId, controller);
        }

        /// <summary>
        /// 根据面板Id检测是否开启
        /// </summary>
        /// <param name="panelId"></param>
        /// <returns></returns>
        public bool IsPanelOpen(string panelId)
        {
            return panelLayer.IsPanelVisible(panelId);
        }

        /// <summary>
        /// 隐藏所有界面
        /// </summary>
        /// <param name="animate"></param>
        public void HideAll(bool animate = true)
        {
            CloseAllWindows(animate);
            HideAllPanels(animate);
        }

        /// <summary>
        /// 隐藏所有窗口
        /// </summary>
        /// <param name="animate"></param>
        public void CloseAllWindows(bool animate = true)
        {
            windowLayer.HideAllScreens(animate);
        }

        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        /// <param name="animate"></param>
        public void HideAllPanels(bool animate = true)
        {
            windowLayer.HideAllScreens(animate);
        }

        /// <summary>
        /// 检查界面是否被注册过
        /// </summary>
        /// <param name="screenId"></param>
        /// <returns></returns>
        public bool IsScreenRegistered(string screenId)
        {
            if (windowLayer == null || panelLayer == null)
            {
                Debug.LogError("[UI Frame] PanelLayer or WindowLayer is missing. Please check the UIFrame hierarchy.");
                return false;
            }

            if (windowLayer.IsScreenRegistered(screenId))
                return true;
            if (panelLayer.IsScreenRegistered(screenId))
                return true;
            return false;
        }

        public bool IsScreenRegistered(string screenId,out Type type)
        {
            if (windowLayer == null || panelLayer == null)
            {
                Debug.LogError("[UI Frame] PanelLayer or WindowLayer is missing. Please check the UIFrame hierarchy.");
                type = null;
                return false;
            }

            if (windowLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IWindowController);
                return true;
            }
            if (panelLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IPanelController);
                return true;
            }
            type = null;
            return false;
        }

        private void OnRequestScreenBlock()
        {
            if(graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }
        }

        private void OnRequestScreenUnBlock()
        {
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = true;
            }
        }

    }
}
