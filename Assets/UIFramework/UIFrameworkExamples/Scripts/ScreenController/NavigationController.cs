using UIFramework.Panel;
using Utils;
using UnityEngine;
using System;
using System.Collections.Generic;

public class NavigateToWindowSignal : ASignal<string>
{

}

[Serializable]
public class NavigationPanelEntry
{
    [SerializeField]
    private Sprite sprite = null;
    [SerializeField]
    private string buttonText = "";
    [SerializeField]
    private string targetScreen = "";

    public Sprite Sprite
    {
        get { return sprite; }
    }

    public string ButtonText
    {
        get { return buttonText; }
    }

    public string TargetScreen
    {
        get { return targetScreen; }    
    }
}

public class NavigationController : PanelController
{
    [SerializeField]
    private List<NavigationPanelEntry> navigationTargets = new List<NavigationPanelEntry>();
    [SerializeField]
    private NavigationPanelButton templateButton = null;

    private readonly List<NavigationPanelButton> currentButtons = new List<NavigationPanelButton>();

    protected override void AddListeners()
    {
        Signals.Get<NavigateToWindowSignal>().AddListener(OnExternalNavigation);
    }

    protected override void RemoveListeners()
    {
        Signals.Get<NavigateToWindowSignal>().RemoveListener(OnExternalNavigation);
    }

    /// <summary>
    /// 当界面打开时调用
    /// </summary>
    protected override void OnPropertiesSet()
    {
        ClearEntries();
        foreach(var target in navigationTargets)
        {
            var newBtn = Instantiate(templateButton);
            newBtn.transform.SetParent(templateButton.transform.parent, false);
            newBtn.SetData(target);
            newBtn.gameObject.SetActive(true);
            newBtn.ButtonClicked += OnNavigationButtonClicked;
            currentButtons.Add(newBtn);
        }

        OnNavigationButtonClicked(currentButtons[0]);
    }

    private void OnNavigationButtonClicked(NavigationPanelButton currentlyClickedButton)
    {
        Signals.Get<NavigateToWindowSignal>().Dispatch(currentlyClickedButton.Target);
        foreach(var button in currentButtons)
        {
            button.SetCurrentNavigationTarget(currentlyClickedButton);
        }

    }

    private void OnExternalNavigation(string screenId)
    {
        foreach(var button in currentButtons)
        {
            button.SetCurrentNavigationTarget(screenId);
        }
    }

    private void ClearEntries()
    {
        foreach(var button in currentButtons)
        {
            button.ButtonClicked -= OnNavigationButtonClicked;
            Destroy(button.gameObject);
        }
    }
}


