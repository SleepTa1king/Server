using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UIFramework;
using UIFramework.Core;

namespace UIFramework
{
    /// <summary>
    /// UI的模板
    /// </summary>
    [CreateAssetMenu(fileName = "UISetting", menuName = ("UI/UI Settings"))]
    public class UISetting : ScriptableObject
    {
        [Tooltip("UI Frame的预制体")]
        [SerializeField]
        private UIFrame templateUIPrefab = null;
        [Tooltip("界面的预制体(包括面板和窗口)")]
        [SerializeField]
        private List<GameObject> screenToRegister = null;
        [Tooltip("实例化时是否停用")]
        [SerializeField]
        private bool deactivateScreenGOs = true;


        /// <summary>
        /// 创建一个UI Frame对象
        /// </summary>
        /// <param name="instanceAndRegisterScreens"></param>
        /// <returns></returns>
        public UIFrame CreateUIInstance(bool instanceAndRegisterScreens = true)
        {
            var newUI = Instantiate(templateUIPrefab);

            if(instanceAndRegisterScreens)
            {
                foreach(var screen in screenToRegister)
                {
                    var screenInstance = Instantiate(screen);
                    var screenController = screenInstance.GetComponent<IScreenController>();

                    if(screenController != null)
                    {
                        newUI.RegisterScreen(screen.name, screenController, screenInstance.transform);
                        if(deactivateScreenGOs && screenInstance.activeSelf)
                        {
                            screenInstance.SetActive(false);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[UIConfig] screen doesn't contain a ScreenController! Skipping{screen.name}");
                    }
                }
            }
            return newUI;
        }

        private void OnValidate()
        {
            List<GameObject> objectsToRemove = new List<GameObject>();
            for(int i=0;i<screenToRegister.Count;i++)
            {
                var screenCtl = screenToRegister[i].GetComponent<IScreenController>();
                if(screenCtl ==null)
                {
                    objectsToRemove.Add(screenToRegister[i]);
                }
            }

            if (objectsToRemove.Count > 0)
            {
                Debug.LogError("[UISetiings] Some GameObjects that were added to the Screen Prefab List didn't have Screen");
                foreach(var obj in objectsToRemove)
                {
                    Debug.LogError($"[UISetiings] Removed {obj.name}  from {name}  as it has no Screen Controller");
                    screenToRegister.Remove(obj);
                }
            }
        }

    }
}
