using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UIFramework;
using Utils;

public class UIDemoController:MonoBehaviour
{
    

    [SerializeField]
    private UISetting defaultUISettings = null;
    [SerializeField]
    private FakePlayerData fakePlayerData = null;
    [SerializeField]
    private Camera cam = null;
    [SerializeField]
    private Transform transformToFollow = null;

    private UIFrame uiFrame;

    private void Awake()
    {
        uiFrame = defaultUISettings.CreateUIInstance();
        Signals.Get<StartDemoSiganl>().AddListener(OnStartDemo);
        Signals.Get<NavigateToWindowSignal>().AddListener(OnNavigateToWindow);
        Signals.Get<ShowConfirmationPopupSignal>().AddListener(OnShowConfirmationPopup);
    }

    private void OnDestroy()
    {
        Signals.Get<StartDemoSiganl>().RemoveListener(OnStartDemo);
        Signals.Get<NavigateToWindowSignal>().RemoveListener(OnNavigateToWindow);
        Signals.Get<ShowConfirmationPopupSignal>().RemoveListener(OnShowConfirmationPopup);
    }

    private void OnShowConfirmationPopup(ConfirmationPopupProperties popupPayload)
    {
        uiFrame.OpenWindow(ScreenIds.ConfirmationPopup, popupPayload);
    }

    private void OnNavigateToWindow(string windowId)
    {
        uiFrame.CloseCurrentWindow();

        switch (windowId)
        {
            case ScreenIds.PlayerWindow:
                uiFrame.OpenWindow(windowId, new PlayerWindowProperties(fakePlayerData.LevelProgress));
                break;
            case ScreenIds.CameraProjectionWindow:
                transformToFollow.parent.gameObject.SetActive(true);
                uiFrame.OpenWindow(windowId, new CameraProjectionWindowProperties(cam, transformToFollow));
                break;
            default:
                uiFrame.OpenWindow(windowId);
                break;
        }
    }
    private void Start()
    {
        uiFrame.OpenWindow(ScreenIds.StartGameWindow);
    }

    private void OnStartDemo()
    {
        uiFrame.ShowPanel(ScreenIds.NavigationPanel);
        uiFrame.ShowPanel(ScreenIds.ToastPanel);
    }
}

