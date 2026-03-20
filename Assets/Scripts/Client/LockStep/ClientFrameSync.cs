using System;
using System.Collections.Generic;
using System.Linq;
using Client;
using UnityEngine;

/// <summary>
/// 帧同步管理器 —— 客户端用
/// </summary>
public class ClientFrameSync
{
    public const float LOGIC_DELTA = 1f / 15f;  // 和服务端保持一致

    // 输入掩码定义
    public const int MASK_JUMP = 1;
    public const int MASK_DASH = 2;
    public const int MASK_SPIN = 4;
    public const int MASK_PICK = 8;

    private float _accumulator = 0f;
    private bool _isRunning = false;

    private SocketClient _client;
    private int _playerId;
    
    // 引用本地玩家，用于采集输入
    private Player _localPlayer;

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
        _frameQueue.Clear();
        
        // 自动获取本地玩家引用（如果已经由 PlayerManager 生成）
        if (_localPlayer == null)
        {
            _localPlayer = PlayerManager.Instance.GetPlayer(_playerId);
        }
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
            ExecuteFrame(frame);
            OnFrameExecute?.Invoke(frame);
        }
    }
    
    private void ExecuteFrame(SCFrame frame)
    {
        // 这一帧包含了所有玩家的操作
        foreach(var input in frame.Inputs)
        {
            var player = PlayerManager.Instance.GetPlayer(input.PlayerId);
            if(player != null)
            {
                // 1. 开启网络同步模式
                if (player.inputs != null)
                {
                    player.inputs.UseNetworkInput = true;
                    player.inputs.NetworkInput = input;
                }
                
                // 2. 执行逻辑步进（确定性逻辑）
                player.LogicUpdate(LOGIC_DELTA);
            }
        }
    }

    /// <summary>
    /// 收集本地硬件输入并打包发送给服务端
    /// </summary>
    private void SendInput()
    {
        if (_localPlayer == null) 
        {
            _localPlayer = PlayerManager.Instance.GetPlayer(_playerId);
            if (_localPlayer == null) return;
        }

        var rawDir = _localPlayer.inputs.GetRawInputDirection();

        // 防止 NaN 导致 Socket 崩溃
        if (float.IsNaN(rawDir.x)) rawDir.x = 0;
        if (float.IsNaN(rawDir.z)) rawDir.z = 0;

        var input = new CSInput
        {
            PlayerId = _playerId,
            MoveX = rawDir.x,
            MoveZ = rawDir.z,
            Buttons = _localPlayer.inputs.GetRawActionMask(),
            RotationY = _localPlayer.transform.eulerAngles.y
        };

        if (input.MoveX < -0.1f) 
        {
            Debug.Log($"[FrameSync] 发送向左输入: MoveX={input.MoveX}, PlayerId={input.PlayerId}");
        }

        byte[] data = ProtoHelper.Serialize(input);
        _client.Send((ushort)SocketEvent.cs_input, data);
    }
    
    private int GetActionMask()
    {
        if (_localPlayer == null || _localPlayer.inputs == null) return 0;
        return _localPlayer.inputs.GetRawActionMask();
    }
}
