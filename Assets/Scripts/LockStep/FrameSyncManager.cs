using System;
using System.Collections.Generic;
using System.Net.Sockets;

/// <summary>
/// 帧同步管理器 —— 服务端用
/// </summary>
public class FrameSyncManager
{
    // 逻辑帧率：每秒15帧（每帧约66ms）
    // 社交场景可以低一些，对抗类游戏一般用 15~30
    public const int LOGIC_FPS = 15;
    public const float LOGIC_DELTA = 1f / LOGIC_FPS;  // 每逻辑帧的固定时长

    private float _accumulator = 0f;   // 时间累加器
    private int _currentFrame = 0;     // 当前逻辑帧号
    private bool _isRunning = false;

    private SocketServer _server;

    // 改为存储 Protobuf 对象
    private Dictionary<Socket, CSInput> _frameInputs = new Dictionary<Socket, CSInput>();

    public int CurrentFrame => _currentFrame;

    public FrameSyncManager(SocketServer server)
    {
        _server = server;
    }

    public void Start()
    {
        _isRunning = true;
        _accumulator = 0f;
        _currentFrame = 0;
    }

    public void Stop()
    {
        _isRunning = false;
    }
    /// <summary>
    /// 玩家断开时清理
    /// </summary>
    public void RemoveClient(Socket client)
    {
        _frameInputs.Remove(client);
    }

    /// <summary>
    /// 收到客户端输入（从 byte[] 反序列化为 CSInput）
    /// </summary>
    public void CollectInput(Socket client, byte[] inputData)
    {
        var input = ProtoHelper.Deserialize<CSInput>(inputData);
        _frameInputs[client] = input;
    }

    public void Tick(float deltaTime)
    {
        if (!_isRunning) return;

        _accumulator += deltaTime;
        while (_accumulator >= LOGIC_DELTA)
        {
            _accumulator -= LOGIC_DELTA;
            Step();
        }
    }

    private void Step()
    {
        _currentFrame++;

        // 1. 构建帧数据
        var frame = new SCFrame
        {
            FrameId = _currentFrame,
            Inputs = new List<CSInput>(_frameInputs.Values)
        };

        // 2. 序列化为 byte[]
        byte[] frameData = ProtoHelper.Serialize(frame);

        // 3. 广播
        foreach (var client in _server.ClientInfoDic.Keys)
        {
            _server.Send(client, (ushort)SocketEvent.sc_frame, frameData);
        }

        // 4. 清空
        _frameInputs.Clear();
    }
}