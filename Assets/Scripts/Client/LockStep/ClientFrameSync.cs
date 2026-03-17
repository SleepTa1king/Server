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

    private float _accumulator = 0f;
    private bool _isRunning = false;

    private SocketClient _client;
    private int _playerId;
    
    // 引用本地玩家，用于采集输入
    private Player _localPlayer;
    // 引用所有玩家（包括本地和远程），用于执行同步逻辑
    private Dictionary<int, Player> _playerDic = new Dictionary<int, Player>();

    // 缓存收到的服务端帧，按帧号排序执行
    private Queue<SCFrame> _frameQueue = new Queue<SCFrame>();

    public event System.Action<SCFrame> OnFrameExecute;  // 每执行一帧通知外部

    public ClientFrameSync(SocketClient client, int playerId)
    {
        _client = client;
        _playerId = playerId;
    }

    /// <summary>
    /// 注册场景中的玩家实体
    /// </summary>
    public void RegisterPlayer(int id, Player player)
    {
        _playerDic[id] = player;
        
        // 开启网络输入模式（由帧同步驱动）
        if (player.inputs != null)
        {
            player.inputs.UseNetworkInput = true;
        }

        // 如果是本地玩家，保存引用用于采集输入
        if (id == _playerId)
        {
            _localPlayer = player;
        }
    }

    public void Start()
    {
        _isRunning = true;
        _accumulator = 0f;
        _frameQueue.Clear();
    }

    public void Stop()
    {
        _isRunning = false;
        // 恢复所有实体的硬件输入模式（可选）
        foreach(var p in _playerDic.Values)
        {
            if (p != null && p.inputs != null)
                p.inputs.UseNetworkInput = false;
        }
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
            if(_playerDic.TryGetValue(input.PlayerId, out var player))
            {
                // 1. 注入同步后的输入
                if (player.inputs != null)
                {
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
        if (_localPlayer == null || _localPlayer.inputs == null) return;

        // 注意：即便开启了 UseNetworkInput，GetMovementDirection 内部会自动切换
        // 这里我们需要在逻辑帧采样原始意图
        
        var input = new CSInput
        {
            PlayerId = _playerId,
            MoveX = Input.GetAxis("Horizontal"), // 或者通过 _localPlayer.inputs 获得
            MoveZ = Input.GetAxis("Vertical"),
            ActionType = GetActionMask(),
            RotationY = _localPlayer.transform.eulerAngles.y
        };

        byte[] data = ProtoHelper.Serialize(input);
        _client.Send((ushort)SocketEvent.cs_input, data);
    }
    
    private int GetActionMask()
    {
        int mask = 0;
        // 示例：用位运算存储按键状态
        if (Input.GetButton("Jump")) mask |= 1;
        if (Input.GetKeyDown(KeyCode.LeftShift)) mask |= 2;
        return mask;
    }
}
