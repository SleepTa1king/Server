using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Server;

public class SocketInfo
{
    public Socket Client;
    public DataBuffer Buffer;  // 每个客户端独立缓冲区
    public long HeadTime;
}

/// <summary>
/// Socket服务端 —— 非阻塞轮询版，全部运行在主线程
/// </summary>
public class SocketServer
{
    public string IP;
    public int Port;

    private const int HEAD_TIMEOUT = 5000;         // 心跳超时 毫秒
    private const float HEAD_CHECK_INTERVAL = 5f;  // 心跳检测间隔 秒

    public Dictionary<Socket, SocketInfo> ClientInfoDic = new Dictionary<Socket, SocketInfo>();

    private Socket _server;
    private bool _isValid;
    private float _headCheckTimer;

    public event Action<Socket> OnConnect;
    public event Action<Socket> OnDisconnect;
    public event Action<Socket, SocketDataPack> OnReceive;
    public event Action<Socket, SocketDataPack> OnSend;

    public SocketServer(string ip, int port)
    {
        IP = ip;
        Port = port;

        _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _server.Blocking = false;  // 关键：非阻塞模式
        _server.Bind(new IPEndPoint(IPAddress.Parse(IP), Port));
        _server.Listen(10);

        _isValid = true;
        _headCheckTimer = 0f;
    }

    /// <summary>
    /// 每帧调用，驱动整个网络层
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!_isValid) return;

        PollAccept();
        PollReceive();
        TickHeartbeat(deltaTime);
    }

    /// <summary>
    /// 检测新连接
    /// </summary>
    private void PollAccept()
    {
        // 可能一帧内有多个连接到达
        while (_server.Poll(0, SelectMode.SelectRead))
        {
            try
            {
                Socket client = _server.Accept();
                client.Blocking = false;
                var info = new SocketInfo
                {
                    Client = client,
                    Buffer = new DataBuffer(),
                    HeadTime = GetNowTime()
                };
                ClientInfoDic.Add(client, info);
                OnConnect?.Invoke(client);
            }
            catch (SocketException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 轮询所有客户端数据
    /// </summary>
    private void PollReceive()
    {
        // 收集 key 避免遍历时修改字典
        var clients = new List<Socket>(ClientInfoDic.Keys);

        foreach (var client in clients)
        {
            if (!ClientInfoDic.ContainsKey(client)) continue;

            var info = ClientInfoDic[client];

            bool readable;
            try
            {
                readable = client.Poll(0, SelectMode.SelectRead);
            }
            catch
            {
                CloseClient(client);
                continue;
            }

            if (!readable) continue;

            byte[] buffer = new byte[8 * 1024];
            int len;
            try
            {
                len = client.Receive(buffer);
            }
            catch (SocketException)
            {
                CloseClient(client);
                continue;
            }

            if (len <= 0)
            {
                // 对端关闭
                CloseClient(client);
                continue;
            }

            // 添加到该客户端独立的缓冲区
            info.Buffer.AddBuffer(buffer, len);

            // 循环解包（可能一次收到多个包）
            SocketDataPack dataPack;
            while (info.Buffer.TryUnpack(out dataPack))
            {
                HandlePack(client, dataPack);
            }
        }
    }

    /// <summary>
    /// 处理收到的数据包
    /// </summary>
    private void HandlePack(Socket client, SocketDataPack dataPack)
    {
        if (dataPack.Type == (UInt16)SocketEvent.sc_head)
        {
            ReceiveHead(client);
        }
        else if (dataPack.Type == (UInt16)SocketEvent.sc_disconn)
        {
            CloseClient(client);
        }
        else
        {
            OnReceive?.Invoke(client, dataPack);
        }
    }

    /// <summary>
    /// 心跳检测（基于帧计时）
    /// </summary>
    private void TickHeartbeat(float deltaTime)
    {
        _headCheckTimer += deltaTime;
        if (_headCheckTimer < HEAD_CHECK_INTERVAL) return;
        _headCheckTimer = 0f;

        long now = GetNowTime();
        var timeoutClients = new List<Socket>();

        foreach (var kvp in ClientInfoDic)
        {
            if (now - kvp.Value.HeadTime > HEAD_TIMEOUT)
            {
                timeoutClients.Add(kvp.Key);
            }
        }

        foreach (var client in timeoutClients)
        {
            KickOut(client);
        }
    }

    private void ReceiveHead(Socket client)
    {
        if (ClientInfoDic.TryGetValue(client, out var info))
        {
            long now = GetNowTime();
            UnityEngine.Debug.LogFormat("更新心跳时间戳 >>> {0}  间隔 >>> {1}", now, now - info.HeadTime);
            info.HeadTime = now;
        }
    }

    /// <summary>
    /// 发送数据（非阻塞异步发送，回调在主线程下一帧自然处理）
    /// </summary>
    public void Send(Socket client, UInt16 type, byte[] buff = null)
    {
        buff = buff ?? new byte[] { };
        var dataPack = new SocketDataPack(type, buff);
        var data = dataPack.Buff;
        try
        {
            client.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                try { ((Socket)ar.AsyncState).EndSend(ar); }
                catch { /* 发送失败不阻塞 */ }
            }, client);
            OnSend?.Invoke(client, dataPack);
        }
        catch (SocketException)
        {
            CloseClient(client);
        }
    }

    public void KickOut(Socket client)
    {
        Send(client, (UInt16)SocketEvent.sc_kickout);
        CloseClient(client);
    }

    public void KickOutAll()
    {
        var clients = new List<Socket>(ClientInfoDic.Keys);
        foreach (var client in clients)
        {
            KickOut(client);
        }
    }

    private void CloseClient(Socket client)
    {
        if (!ClientInfoDic.ContainsKey(client)) return;
        ClientInfoDic.Remove(client);
        OnDisconnect?.Invoke(client);
        try { client.Close(); } catch { }
    }

    public void Close()
    {
        if (!_isValid) return;
        _isValid = false;
        KickOutAll();
        _server.Close();
    }

    private long GetNowTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
