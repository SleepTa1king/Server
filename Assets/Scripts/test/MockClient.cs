using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// 模拟客户端测试脚本 —— 挂到服务端场景的另一个 GameObject 上
/// </summary>
public class MockClient : MonoBehaviour
{
    private Socket _socket;
    private DataBuffer _buffer = new DataBuffer();
    private bool _connected;
    private float _heartTimer;
    private float _inputTimer;

    private void Start()
    {
        // 延迟一帧，确保服务端已启动
        Invoke(nameof(DoConnect), 0.5f);
    }

    private void DoConnect()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Blocking = false;

        try
        {
            _socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6854));
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock &&
                e.SocketErrorCode != SocketError.InProgress)
            {
                Debug.LogError($"[MockClient] 连接失败: {e.SocketErrorCode}");
                return;
            }
        }

        Debug.Log("[MockClient] 正在连接...");
    }

    private void Update()
    {
        if (_socket == null) return;

        // 检测连接
        if (!_connected)
        {
            if (_socket.Poll(0, SelectMode.SelectWrite))
            {
                _connected = true;
                Debug.Log("[MockClient] ✅ 连接成功！");
            }
            return;
        }

        // 发心跳
        _heartTimer += Time.deltaTime;
        if (_heartTimer >= 2f)
        {
            _heartTimer = 0f;
            SendPack((ushort)SocketEvent.sc_head);
        }

        // 模拟每秒发一次输入
        _inputTimer += Time.deltaTime;
        if (_inputTimer >= 1f)
        {
            _inputTimer = 0f;
            SendInput();
        }

        // 接收数据
        PollReceive();
    }

    private void SendInput()
    {
        var input = new CSInput
        {
            PlayerId = 999,
            MoveX = UnityEngine.Random.Range(-1f, 1f),
            MoveZ = UnityEngine.Random.Range(-1f, 1f),
            ActionType = 0,
            RotationY = 0f
        };

        byte[] data = ProtoHelper.Serialize(input);
        SendPack((ushort)SocketEvent.cs_input, data);
        Debug.LogFormat("[MockClient] 发送输入 >>> MoveX:{0:F2} MoveZ:{1:F2}",
            input.MoveX, input.MoveZ);
    }

    private void PollReceive()
    {
        if (!_socket.Poll(0, SelectMode.SelectRead)) return;

        byte[] temp = new byte[8 * 1024];
        int len;
        try { len = _socket.Receive(temp); }
        catch { return; }

        if (len <= 0) return;

        _buffer.AddBuffer(temp, len);

        SocketDataPack pack;
        while (_buffer.TryUnpack(out pack))
        {
            switch ((SocketEvent)pack.Type)
            {
                case SocketEvent.sc_frame:
                    var frame = ProtoHelper.Deserialize<SCFrame>(pack.Data);
                    Debug.LogFormat("[MockClient] 收到帧数据 >>> 帧号:{0} 输入数:{1}",
                        frame.FrameId, frame.Inputs.Count);
                    foreach (var inp in frame.Inputs)
                    {
                        Debug.LogFormat("  玩家{0}: ({1:F2}, {2:F2})",
                            inp.PlayerId, inp.MoveX, inp.MoveZ);
                    }
                    break;
                case SocketEvent.sc_kickout:
                    Debug.Log("[MockClient] 被踢出");
                    break;
            }
        }
    }

    private void SendPack(ushort type, byte[] data = null)
    {
        data = data ?? new byte[0];
        var pack = new SocketDataPack(type, data);
        try
        {
            _socket.BeginSend(pack.Buff, 0, pack.Buff.Length, SocketFlags.None,
                (ar) => { try { ((Socket)ar.AsyncState).EndSend(ar); } catch { } },
                _socket);
        }
        catch { }
    }

    private void OnDestroy()
    {
        try { _socket?.Close(); } catch { }
    }
}