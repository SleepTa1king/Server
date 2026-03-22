using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFramework.Core
{
    public abstract class UILayer<TScreen> : MonoBehaviour where TScreen : IScreenController
    {

        public Dictionary<string, TScreen> registeredScreens;
        
        /// <summary>
        /// 显示界面 
        /// </summary>
        /// <param name="screen">TScreen:Window和Panel</param>
        public abstract void ShowScreen(TScreen screen);

        /// <summary>
        /// 显示界面，带一些参数
        /// </summary>
        /// <param name="screen">界面类型:Window和Panel</param>
        /// <param name="properties">属性参数</param>
        /// <typeparam name="Tprops">属性类型</typeparam>
        public abstract void ShowScreen<Tprops>(TScreen screen, Tprops properties) where Tprops : IScreenProperties;

        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <param name="screen">界面类型:Window和Panel</param>
        public abstract void HideScreen(TScreen screen);

        /// <summary>
        /// 初始化Layer层
        /// </summary>
        public virtual void Initialize()
        {
            registeredScreens = new Dictionary<string, TScreen>();
        }

        /// <summary>
        /// 将传入的界面作为层的子节点
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="screenTransform"></param>
        public virtual void ReparentScreen(IScreenController controller,Transform screenTransform)
        {
            screenTransform.SetParent(transform, false); 
        }

        /// <summary>
        /// 注册界面的controller带上明确的screenId
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="screen"></param>
        public virtual void RegiserScreen(string screenId,TScreen screen)
        {
            if(!registeredScreens.ContainsKey(screenId))
            {
                ProcessScreenRegister(screenId, screen);
            }
            else
            {
                Debug.LogError($"[AUIScreenController] Screen Controller has already registered:{screenId}");
            }
        }

        /// <summary>
        /// 根据Id注销注册界面的controller
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="screen"></param>
        public virtual void UnRegisteredScreen(string screenId,TScreen screen)
        {
            if(registeredScreens.ContainsKey(screenId))
            {
                ProcessScreenUnRegister(screenId, screen);
            }
            else
            {
                Debug.LogError($"[AUIScreenController] Screen Controller has not registered:{screenId}");
            }
        }

        /// <summary>
        /// 根据界面Id显示对应界面
        /// </summary>
        /// <param name="screenId"></param>
        public void ShowScreenById(string screenId)
        {
            TScreen screen;
            if(registeredScreens.TryGetValue(screenId, out screen))
            {
                ShowScreen(screen);
            }
            else
            {
                Debug.LogError($"[AUIScreenController] Screen Controller has not registered:{screenId}+{screen},could not show");
            }
        }

        /// <summary>
        /// 根据界面Id显示对应界面，带上具体参数
        /// </summary>
        /// <param name="screenId"></param>
        public void ShowScreenById<Tprops>(string screenId,Tprops properties)where Tprops:IScreenProperties
        {
            TScreen screen;
            if (registeredScreens.TryGetValue(screenId, out screen))
            {
                ShowScreen(screen,properties);
            }
            else
            {
                Debug.LogError($"[AUIScreenController] Screen Controller has not registered:{screenId}+{screen},could not show");
            }
        }

        /// <summary>
        /// 根据Id隐藏界面
        /// </summary>
        /// <param name="screenId"></param>
        public void HideScreenById(string screenId)
        {
            TScreen screen;
            if (registeredScreens.TryGetValue(screenId, out screen))
            {
                HideScreen(screen);
            }
            else
            {
                Debug.LogError($"[AUIScreenController] Screen Controller has not registered:{screenId}+{screen},could not hide");
            }
        }

        /// <summary>
        /// 查看Id是否已经注册了
        /// </summary>
        /// <param name="screenId"></param>
        /// <returns></returns>
        public bool IsScreenRegistered(string screenId)
        {
            return (registeredScreens.ContainsKey(screenId));
        }

       /// <summary>
       /// 隐藏全部界面
       /// </summary>
       /// <param name="shouldAnimateWhenHiding">隐藏的时候是否需要动画</param>
        public virtual void HideAllScreens(bool shouldAnimateWhenHiding = true)
        {
            foreach(TScreen screen in registeredScreens.Values)
            {
                screen.Hide(shouldAnimateWhenHiding);
            }
        }


        /// <summary>
        /// 根据给定Id进行具体注册逻辑
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="screen"></param>
        protected virtual void ProcessScreenRegister(string screenId,TScreen screen)
        {
            screen.ScreenId = screenId;
            registeredScreens.Add(screenId, screen);
            screen.ScreenDestroyed += OnScreenDestroyed;
        }

        /// <summary>
        /// 根据给定Id进行具体注销逻辑
        /// </summary>
        /// <param name="screenId"></param>
        /// <param name="screen"></param>
        protected virtual void ProcessScreenUnRegister(string screenId, TScreen screen)
        {
            screen.ScreenDestroyed -= OnScreenDestroyed;
            registeredScreens.Remove(screenId);
        }

        /// <summary>
        /// 界面销毁时委托的销毁
        /// </summary>
        /// <param name="screen"></param>
        private void OnScreenDestroyed(IScreenController screen)
        {
            if(!string.IsNullOrEmpty(screen.ScreenId) && registeredScreens.ContainsKey(screen.ScreenId))
            {
                UnRegisteredScreen(screen.ScreenId, (TScreen)screen);
            }
        }


    }

}
