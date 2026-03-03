using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Client;

/// <summary>
/// Socket客户端 —— 非阻塞轮询版，全部运行在主线程
/// </summary>
namespace Client
{
    public class SocketClient
{
    public string IP;
    public int Port;

    private Socket _client;
    private DataBuffer _dataBuffer = new DataBuffer();

    private bool _isConnected;
    private bool _isConnecting;
    private bool _isValid;

    // 心跳
    private const float HEART_INTERVAL = 2f;
    private float _heartTimer;

    // 连接超时
    private const float CONNECT_TIMEOUT = 3f;
    private float _connectTimer;

    // 断线重连
    private const int RECONN_MAX_SUM = 3;
    private int _reconnectCount;
    private bool _isReconnecting;
    private bool _autoReconnect = true;

    public event Action OnConnectSuccess;
    public event Action OnConnectError;
    public event Action OnDisconnect;
    public event Action<SocketDataPack> OnReceive;
    public event Action<SocketDataPack> OnSend;
    public event Action<SocketException> OnError;
    public event Action<int> OnReConnectSuccess;
    public event Action<int> OnReConnectError;
    public event Action<int> OnReconnecting;

    public bool IsConnected => _isConnected;

    public SocketClient(string ip, int port)
    {
        IP = ip;
        Port = port;
    }

    public void Connect()
    {
        if (_isConnected || _isConnecting) return;

        _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _client.Blocking = false;
        _isValid = true;
        _isConnected = false;
        _isConnecting = true;
        _connectTimer = 0f;
        _heartTimer = 0f;

        try
        {
            _client.Connect(new IPEndPoint(IPAddress.Parse(IP), Port));
            // 非阻塞下如果立即连上（本地回环）
            _isConnecting = false;
            _isConnected = true;
            OnConnectSuccess?.Invoke();
        }
        catch (SocketException e)
        {
            if (e.SocketErrorCode != SocketError.WouldBlock &&
                e.SocketErrorCode != SocketError.InProgress)
            {
                _isConnecting = false;
                _isValid = false;
                OnConnectError?.Invoke();
            }
            // WouldBlock / InProgress 是正常的，等 PollConnect 检测
        }
    }

    /// <summary>
    /// 每帧调用，驱动客户端网络
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!_isValid) return;

        if (_isConnecting)
        {
            PollConnect(deltaTime);
            return;
        }

        if (!_isConnected) return;

        PollReceive();
        TickHeartbeat(deltaTime);
    }

    private void PollConnect(float deltaTime)
    {
        _connectTimer += deltaTime;

        // 超时检测
        if (_connectTimer >= CONNECT_TIMEOUT)
        {
            _isConnecting = false;
            _isValid = false;
            try { _client?.Close(); } catch { }
            _client = null;

            if (_isReconnecting)
            {
                OnReConnectError?.Invoke(_reconnectCount);
                TryReconnectNext();
            }
            else
            {
                OnConnectError?.Invoke();
            }
            return;
        }

        try
        {
            if (_client.Poll(0, SelectMode.SelectWrite))
            {
                _isConnecting = false;
                _isConnected = true;
                Debug.Log("连接服务器成功");

                if (_isReconnecting)
                {
                    OnReConnectSuccess?.Invoke(_reconnectCount);
                    _isReconnecting = false;
                    _reconnectCount = 0;
                }
                else
                {
                    OnConnectSuccess?.Invoke();
                }
            }
            else if (_client.Poll(0, SelectMode.SelectError))
            {
                _isConnecting = false;
                _isValid = false;

                if (_isReconnecting)
                {
                    OnReConnectError?.Invoke(_reconnectCount);
                    TryReconnectNext();
                }
                else
                {
                    OnConnectError?.Invoke();
                }
            }
        }
        catch
        {
            _isConnecting = false;
            _isValid = false;
            OnConnectError?.Invoke();
        }
    }

    private void PollReceive()
    {
        bool readable;
        try
        {
            readable = _client.Poll(0, SelectMode.SelectRead);
        }
        catch
        {
            HandleError(null);
            return;
        }

        if (!readable) return;

        byte[] temp = new byte[8 * 1024];
        int len;
        try
        {
            len = _client.Receive(temp);
        }
        catch (SocketException ex)
        {
            HandleError(ex);
            return;
        }

        if (len <= 0)
        {
            HandleError(null);
            return;
        }

        _dataBuffer.AddBuffer(temp, len);

        SocketDataPack pack;
        while (_dataBuffer.TryUnpack(out pack))
        {
            if (pack.Type == (ushort)SocketEvent.sc_kickout)
            {
                Debug.Log("被服务端踢出");
                HandleDisconnect();
                return;
            }
            OnReceive?.Invoke(pack);
        }
    }

    private void TickHeartbeat(float deltaTime)
    {
        _heartTimer += deltaTime;
        if (_heartTimer >= HEART_INTERVAL)
        {
            _heartTimer = 0f;
            Send((ushort)SocketEvent.sc_head);
        }
    }

    public void Send(ushort type, byte[] data = null)
    {
        if (!_isConnected || _client == null) return;
        data = data ?? new byte[0];
        var pack = new SocketDataPack(type, data);
        try
        {
            _client.BeginSend(pack.Buff, 0, pack.Buff.Length, SocketFlags.None, (ar) =>
            {
                try { ((Socket)ar.AsyncState).EndSend(ar); } catch { }
            }, _client);
            OnSend?.Invoke(pack);
        }
        catch (SocketException ex)
        {
            HandleError(ex);
        }
    }

    /// <summary>
    /// 客户端主动断开
    /// </summary>
    public void DisConnect()
    {
        _autoReconnect = false; // 主动断开不触发重连
        Send((ushort)SocketEvent.sc_disconn);
        HandleDisconnect();
    }

    private void HandleError(SocketException ex)
    {
        CleanUp();
        OnError?.Invoke(ex);

        if (_autoReconnect && !_isReconnecting)
        {
            _reconnectCount = 0;
            _isReconnecting = true;
            TryReconnectNext();
        }
    }

    private void HandleDisconnect()
    {
        CleanUp();
        OnDisconnect?.Invoke();
    }

    private void TryReconnectNext()
    {
        _reconnectCount++;
        if (_reconnectCount > RECONN_MAX_SUM)
        {
            _isReconnecting = false;
            _reconnectCount = 0;
            OnDisconnect?.Invoke();
            return;
        }
        OnReconnecting?.Invoke(_reconnectCount);
        Connect(); // 重新发起连接，下一帧 Tick 中会自动检测
    }

    private void CleanUp()
    {
        _isConnected = false;
        _isConnecting = false;
        _isValid = false;
        try { _client?.Close(); } catch { }
        _client = null;
    }

    public void Close()
    {
        _autoReconnect = false;
        _isReconnecting = false;
        if (_isConnected)
        {
            Send((ushort)SocketEvent.sc_disconn);
        }
        CleanUp();
    }
}
}
