using System.Collections.Generic;
using Client;
using UnityEngine;

/// <summary>
/// 帧同步管理器 —— 客户端用
/// </summary>
public class ClientFrameSync
{
    public const float LOGIC_DELTA = 1f / 15f;  // 和服务端保持一致

    private float _accumulator = 0f;
    private bool _isRunning = false;

    private SocketClient _client;
    private int _playerId;

    // 缓存收到的服务端帧，按帧号排序执行
    private Queue<SCFrame> _frameQueue = new Queue<SCFrame>();

    public event System.Action<SCFrame> OnFrameExecute;  // 每执行一帧通知外部

    public ClientFrameSync(SocketClient client, int playerId)
    {
        _client = client;
        _playerId = playerId;
    }

    public void Start()
    {
        _isRunning = true;
        _accumulator = 0f;
    }

    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// 收到服务端帧数据时调用
    /// </summary>
    public void ReceiveFrame(byte[] data)
    {
        var frame = ProtoHelper.Deserialize<SCFrame>(data);
        _frameQueue.Enqueue(frame);
    }

    /// <summary>
    /// 每帧调用
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!_isRunning) return;

        // 1. 收集并发送本地输入（每逻辑帧发一次）
        _accumulator += deltaTime;
        while (_accumulator >= LOGIC_DELTA)
        {
            _accumulator -= LOGIC_DELTA;
            SendInput();
        }

        // 2. 执行服务端下发的帧
        while (_frameQueue.Count > 0)
        {
            var frame = _frameQueue.Dequeue();
            OnFrameExecute?.Invoke(frame);
        }
    }

    /// <summary>
    /// 收集本地输入并发送给服务端
    /// </summary>
    private void SendInput()
    {
        // 这里从你的 3C 控制器 / InputManager 获取输入
        var input = new CSInput
        {
            PlayerId = _playerId,
            MoveX = Input.GetAxis("Horizontal"),
            MoveZ = Input.GetAxis("Vertical"),
            ActionType = 0,  // 从你的 FSM 获取当前状态ID
            RotationY = 0f   // 从角色 Transform 获取
        };

        byte[] data = ProtoHelper.Serialize(input);
        _client.Send((ushort)SocketEvent.cs_input, data);
    }
}