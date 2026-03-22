using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间
using UIFramework.Window;
using Utils;
using Main;

/// <summary>
/// 登录窗口 —— 适配 UI 框架 + 信号系统 (TextMeshPro 版)
/// </summary>
public class UILoginWindow : WindowController
{
    [Header("Login Components")]
    public TMP_InputField idInputField; // 使用 TMP 版本的输入框
    public Button connectButton;

    protected override void Awake()
    {
        base.Awake();
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectClicked);
        }

        // 监听登录成功信号，成功后自动关闭界面
        Signals.Get<LoginSuccessSignal>().AddListener(CloseLogin);
    }

    private void OnConnectClicked()
    {
        if (idInputField == null || string.IsNullOrEmpty(idInputField.text))
        {
            Debug.LogWarning("请输入有效的角色名字或 ID");
            return;
        }

        string rawInput = idInputField.text;
        
        // 发射登录请求信号
        Debug.Log($"[UI] 发射登录信号: {rawInput}");
        Signals.Get<LoginRequestSignal>().Dispatch(rawInput);
        
        // 这里暂时不调用 UI_Close，等到 LoginSuccessSignal 回来再关
    }

    public void CloseLogin()
    {
        Debug.Log("[UI] 收到登录成功信号，正在关闭登录窗口...");
        UI_Close();
        
        // 保险：如果 UI 框架没响应 UI_Close，强制隐藏
        gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 记得移除监听，防止内存泄漏和空引用报错
        Signals.Get<LoginSuccessSignal>().RemoveListener(CloseLogin);
    }
}

