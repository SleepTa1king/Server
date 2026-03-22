using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class NavigationPanelButton:MonoBehaviour
{
    [SerializeField]
    private Text buttonLabel = null;
    [SerializeField]
    private Image icon = null;

    public event Action<NavigationPanelButton> ButtonClicked;

    private NavigationPanelEntry navigationData = null;
    private Button _button = null;
    private Button Button
    {
        get
        {
            if(_button == null)
                _button = GetComponent<Button>();
            return _button;
        }
    }

    public string Target
    {
        get { return navigationData.TargetScreen; }
    }

    public void SetData(NavigationPanelEntry target)
    {
        navigationData = target;
        buttonLabel.text = target.ButtonText;
        icon.sprite = target.Sprite;
    }

    public void SetCurrentNavigationTarget(NavigationPanelButton selectedButton)
    {
        Button.interactable = selectedButton != this;
    }

    public void SetCurrentNavigationTarget(string screenId)
    {
        if(navigationData !=null)
        {
            Button.interactable = navigationData.TargetScreen == screenId;
        }
    }

    public void UI_Click()
    {
        if(ButtonClicked !=null)
        {
            ButtonClicked?.Invoke(this);
        }
    }
}

