/// <summary>
/// 网络事件协议ID枚举
/// </summary>
public enum SocketEvent
{
    // --- 系统级 ---
    sc_head = 0x0001,       // 心跳包
    sc_disconn = 0x0002,    // 客户端主动断开
    sc_kickout = 0x0003,    // 服务端踢出

    // --- 帧同步 ---
    sc_login = 0x1001,      // 服务端 → 客户端：登录并分配/确认 ID
    cs_login = 0x2000,      // 客户端 → 服务端：主动上报 ID
    cs_input = 0x2001,      // 客户端 → 服务端：本帧输入
    sc_frame = 0x2002,      // 服务端 → 客户端：同步帧数据

    // --- 测试 ---
    sc_test = 0xF001,       // 测试用
}
