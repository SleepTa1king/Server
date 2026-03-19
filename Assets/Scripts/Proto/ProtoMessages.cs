using ProtoBuf;
using System.Collections.Generic;

/// <summary>
/// 客户端 → 服务端：玩家输入
/// </summary>
[ProtoContract]
public class CSInput
{
    [ProtoMember(1)] public int PlayerId;
    [ProtoMember(2)] public float MoveX;     // 移动方向X
    [ProtoMember(3)] public float MoveZ;     // 移动方向Z
    [ProtoMember(4)] public int ActionType;  // 动作类型（FSM状态ID）
    [ProtoMember(5)] public float RotationY; // 朝向
    [ProtoMember(6)] public int Buttons;    // 位掩码按钮状态：1-Jump, 2-Dash, 4-Spin, 8-Pick
}

/// <summary>
/// 服务端 → 客户端：一帧的同步数据
/// </summary>
[ProtoContract]
public class SCFrame
{
    [ProtoMember(1)] public int FrameId;
    [ProtoMember(2)] public List<CSInput> Inputs = new List<CSInput>();
}

/// <summary>
/// 聊天消息
/// </summary>
[ProtoContract]
public class CSChat
{
    [ProtoMember(1)] public int PlayerId;
    [ProtoMember(2)] public string Message;
}

/// <summary>
/// 房间加入请求
/// </summary>
[ProtoContract]
public class CSRoomJoin
{
    [ProtoMember(1)] public int PlayerId;
    [ProtoMember(2)] public string RoomId;
}