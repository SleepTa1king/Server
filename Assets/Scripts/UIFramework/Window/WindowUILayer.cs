using UnityEngine;
using UIFramework.Core;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace UIFramework.Window
{
    public class WindowUILayer : UILayer<IWindowController>
    {
        [SerializeField]
        private WindowParaLayer priorityParaLayer = null;

        public IWindowController CurrentWindow { get; private set; }

        private Queue<WindowHistoryEntry> windowQueue;
        private Stack<WindowHistoryEntry> windowHistory;
        private HashSet<IScreenController> screenTransitioning;

        public event Action RequestScreenBlock;
        public event Action RequestScreenUnBlock;

        private bool IsScreenTransitionInProgress
        {
            get { return screenTransitioning.Count != 0; }
        }
        public override void Initialize()
        {
            base.Initialize();
            registeredScreens = new Dictionary<string, IWindowController>();
            windowQueue = new Queue<WindowHistoryEntry>();
            windowHistory = new Stack<WindowHistoryEntry>();
            screenTransitioning = new HashSet<IScreenController>();
        }

        protected override void ProcessScreenRegister(string screenId, IWindowController screen)
        {
            base.ProcessScreenRegister(screenId, screen);
            screen.InTransitionFinished += OnInAnimationFinished;
            screen.OutTransitionFinished += OnOutAnimationFinished;
            screen.ScreenClosed += OnCloseRequestedByWindow;
        }

        protected override void ProcessScreenUnRegister(string screenId, IWindowController screen)
        {
            base.ProcessScreenUnRegister(screenId, screen);
            screen.InTransitionFinished -= OnInAnimationFinished;
            screen.OutTransitionFinished -= OnOutAnimationFinished;
            screen.ScreenClosed -= OnCloseRequestedByWindow;
        }
        public override void HideScreen(IWindowController screen)
        {
            if(screen == CurrentWindow)
            {
                windowHistory.Pop();
                AddTransition(screen);
                screen.Hide();
                CurrentWindow = null;

                if(windowQueue.Count > 0)
                {
                    ShowNextInQueue();
                }
                else if(windowHistory.Count>0)
                {
                    ShowPreviousInHistory();
                }

            }
            else
            {
                Debug.LogError(string.Format("[WindowUILayer] Hide requested {0} but that's not the current open one(({1}))!",
                                screen.ScreenId, CurrentWindow != null ? CurrentWindow.ScreenId : "current is null"));
            }
        }

        public override void HideAllScreens(bool shouldAnimateWhenHiding = true)
        {
            base.HideAllScreens(shouldAnimateWhenHiding);
            CurrentWindow = null;
            priorityParaLayer.RefreshDarken();
            windowHistory.Clear();
        }
        public override void ShowScreen(IWindowController screen)
        {
            ShowScreen<IWindowProperties>(screen, null);
        }

        public override void ShowScreen<Tprops>(IWindowController screen, Tprops properties)
        {
            WindowProperties windowProp = properties as WindowProperties;
            if(ShouldEnqueue(screen, windowProp))
            {
                EnqueueWindow(screen, properties);
            }
            else
            {
                DoShow(screen, windowProp);
            }
        }

        public override void ReparentScreen(IScreenController controller, Transform screenTransform)
        {
            IWindowController window = controller as IWindowController;

            if(window == null)
            {
                Debug.LogError($"[WindowUILayer]Screen {screenTransform.name} is not a Window");
            }
            else
            {
                if(window.IsPopUp)
                {
                    priorityParaLayer.AddScreen(screenTransform);
                    return;
                }
            }
            base.ReparentScreen(controller, screenTransform);
        }

        private void EnqueueWindow<Tprop> (IWindowController screen,Tprop properties) where Tprop : IScreenProperties
        {
            windowQueue.Enqueue(new WindowHistoryEntry(screen, (IWindowProperties)properties));
        }

        private bool ShouldEnqueue(IWindowController screen,WindowProperties properties)
        {
            if(CurrentWindow == null && windowQueue.Count == 0)
            {
                return false;
            }

            if(properties !=null && properties.SuppressPrefabProperties)
            {
                return properties.WindowQueuePriority != WindowPriority.ForceForeground;
            }

            if(screen.WindowPriority != WindowPriority.ForceForeground)
            {
                return true;
            }

            return false;
        }

        private void ShowPreviousInHistory()
        {
            if(windowHistory.Count>0)
            {
                WindowHistoryEntry window = windowHistory.Pop();
                DoShow(window);
            }
        }

        private void ShowNextInQueue()
        {
            if (windowHistory.Count > 0)
            {
                WindowHistoryEntry window = windowQueue.Dequeue();
                DoShow(window);
            }
        }

        private void DoShow(IWindowController screen, IWindowProperties properties)
        {
            DoShow(new WindowHistoryEntry(screen, properties));
        }

        private void DoShow(WindowHistoryEntry windowEntry)
        {
            if(CurrentWindow == windowEntry.Screen)
            {
                Debug.LogError($"[WindowLayer] the requested window{CurrentWindow.ScreenId} has already open!");
            }
            else if(CurrentWindow != null && CurrentWindow.HideOnForegroundLost && !windowEntry.Screen.IsPopUp)
            {
                CurrentWindow.Hide();
            }

            windowHistory.Push(windowEntry);
            AddTransition(windowEntry.Screen);

            if(windowEntry.Screen.IsPopUp)
            {
                priorityParaLayer.DarkenBg();
            }

            windowEntry.Show();

            CurrentWindow = windowEntry.Screen;
        }

        private void OnInAnimationFinished(IScreenController screen)
        {
            RemoveTransition(screen);
        }

        private void OnOutAnimationFinished(IScreenController screen)
        {
            RemoveTransition(screen);
            var window = screen as IWindowController;
            if(window.IsPopUp)
            {
                priorityParaLayer.RefreshDarken();
            }
        }

        private void OnCloseRequestedByWindow(IScreenController screen)
        {
            HideScreen(screen as IWindowController);
        }

        private void AddTransition(IScreenController screen)
        {
            screenTransitioning.Add(screen);
            if(RequestScreenBlock !=null)
            {
                RequestScreenBlock();
            }
        }

        private void RemoveTransition(IScreenController screen)
        {
            screenTransitioning.Remove(screen);
            if (!IsScreenTransitionInProgress)
            {
                if (RequestScreenUnBlock != null)
                {
                    RequestScreenUnBlock();
                }
            }
        }
    }
}
