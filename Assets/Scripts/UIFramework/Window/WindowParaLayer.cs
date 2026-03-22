using UIFramework.Core;
using UIFramework.Panel;
using UnityEngine;
using System.Collections.Generic;

namespace UIFramework.Window
{
    /// <summary>
    /// 辅助层级，以便显示优先级更高的窗口
    /// 默认情况下包含所有标记为弹出窗口的小窗口，由WindowUILayer控制
    /// </summary>
    public class WindowParaLayer:MonoBehaviour
    {
        [SerializeField]
        private GameObject darkenBgObject = null;

        private List<GameObject> containedScreens  = new List<GameObject>();

        public void AddScreen (Transform screenRectTransform)
        {
            screenRectTransform.SetParent(transform, false);
            containedScreens.Add(screenRectTransform.gameObject);
        }

        public void RefreshDarken()
        {
            for(int i = 0; i < containedScreens.Count; i++)
            {
                if(containedScreens[i] != null)
                {
                    if (containedScreens[i].activeSelf)
                    {
                        darkenBgObject.SetActive(true);
                        return;
                    }
                }
            }
            darkenBgObject.SetActive(false);
        }

        public void DarkenBg()
        {
            darkenBgObject.SetActive(true);
            darkenBgObject.transform.SetAsLastSibling();
        }
    }
}
