using Utils;

namespace Main
{
    /// <summary>
    /// 登录请求信号：参数为玩家输入的角色名字字符串
    /// </summary>
    public class LoginRequestSignal : ASignal<string> { }

    /// <summary>
    /// 登录成功信号：当服务端确认 ID 后触发，用于通知 UI 切换或关闭
    /// </summary>
    public class LoginSuccessSignal : ASignal { }
}
