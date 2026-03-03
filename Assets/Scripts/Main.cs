using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    SocketServer _server;
    FrameSyncManager _frameSync;

    private void Awake()
    {
        _server = new SocketServer("127.0.0.1", 6854);

        _frameSync = new FrameSyncManager(_server);

        _server.OnConnect += (client) =>
        {
            Debug.LogFormat("连接成功 >> IP:{0}", client.RemoteEndPoint.ToString());
        };
        _server.OnDisconnect += (client) =>
        {
            Debug.LogFormat("连接断开");
            _frameSync.RemoveClient(client);
        };
        _server.OnReceive += (client, data) =>
        {
            switch ((SocketEvent)data.Type)
            {
                case SocketEvent.cs_input:
                    // 收到客户端输入，交给帧同步管理器
                    _frameSync.CollectInput(client, data.Data);
                    break;
                case SocketEvent.sc_test:
                    Debug.LogFormat("测试数据 >>> {0}",
                        System.Text.Encoding.UTF8.GetString(data.Data));
                    break;
            }
        };
    }

    private void Update()
    {
        // 1. 网络收发 —— 每帧都跑，越快越好
        _server?.Tick(Time.deltaTime);

        // 2. 帧同步逻辑 —— 内部自己控制固定步长
        _frameSync?.Tick(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.A))
        {
            _server?.KickOutAll();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            _frameSync?.Start();
            Debug.Log("帧同步开始");
        }
    }

    private void OnDestroy()
    {
        _frameSync?.Stop();
        _server?.Close();
    }
}
