using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework.Core;

namespace UIFramework.Panel
{
    /// <summary>
    /// 这个Layer层用来控制面板
    /// 面板是界面的一种
    /// 直接简单显示在界面中
    /// 例如小地图，血条等
    /// </summary>
    public class PanelUILayer : UILayer<IPanelController>
    {
        [SerializeField]
        [Tooltip("优先级并行层的设置。注册到此层的面板将根据其优先级重新归属到不同的并行层对象。")]
        private PanelPriorityLayerList priorityLayers = null;
        public override void HideScreen(IPanelController screen)
        {
            screen.Hide();
        }

        public override void ShowScreen(IPanelController screen)
        {
            screen.Show();
        }

        public override void ShowScreen<Tprops>(IPanelController screen, Tprops properties)
        {
            screen.Show(properties); 
        }

        public bool IsPanelVisible(string panelId)
        {
            IPanelController panel;
            if(registeredScreens.TryGetValue(panelId,out panel))
            {
                return panel.IsVisible;
            }
            return false;
        }
        public override void ReparentScreen(IScreenController controller, Transform screenTransform)
        {
            var ctl = controller as IPanelController;
            if(ctl != null)
            {
                ReparentToParaLayer(ctl.Priority, screenTransform);
            }
            else
            {
                base.ReparentScreen(controller, screenTransform);
            }
        }

        private void ReparentToParaLayer(PanelPriority priority,Transform screenTransform)
        {
            Transform trans;
            if(!priorityLayers.ParaLayerLookUp.TryGetValue(priority,out trans))
            {
                trans = transform;
            }

            screenTransform.SetParent(trans,false);
        }
    }

}
