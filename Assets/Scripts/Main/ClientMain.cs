using Client;
using UnityEngine;

public class ClientMain : MonoBehaviour
{
    SocketClient _client;
    ClientFrameSync _frameSync;
    int _myPlayerId = 0; // 等待服务端分配

    private void Awake()
    {
        _client = new SocketClient("127.0.0.1", 6854);

        _client.OnConnectSuccess += () =>
        {
            Debug.Log("连接服务端成功，等待分配 ID...");
        };
        _client.OnDisconnect += () =>
        {
            Debug.Log("断开连接");
            _frameSync?.Stop();
            PlayerManager.Instance.ClearAll();
        };
        _client.OnReceive += (pack) =>
        {
            switch ((SocketEvent)pack.Type)
            {
                case SocketEvent.sc_login:
                    // 1. 收到服务端分配的 ID
                    var loginMsg = ProtoHelper.Deserialize<SCLogin>(pack.Data);
                    _myPlayerId = loginMsg.PlayerId;
                    Debug.Log($"[Login] 分配到的 ID 是: {_myPlayerId}");

                    // 2. 初始化帧同步并生成本地角色
                    _frameSync = new ClientFrameSync(_client, _myPlayerId);
                    PlayerManager.Instance.SpawnPlayer(_myPlayerId, Vector3.zero, isLocal: true);

                    // 3. 开启同步
                    _frameSync.Start();
                    break;

                case SocketEvent.sc_frame:
                    // 收到服务端帧数据
                    var frame = ProtoHelper.Deserialize<SCFrame>(pack.Data);

                    // 在执行帧之前，检查是否有新玩家需要生成
                    foreach (var input in frame.Inputs)
                    {
                        if (PlayerManager.Instance.GetPlayer(input.PlayerId) == null)
                        {
                            Debug.Log($"[ClientMain] 发现新玩家 {input.PlayerId}，正在生成...");
                            PlayerManager.Instance.SpawnPlayer(input.PlayerId, Vector3.zero, isLocal: false);
                        }
                    }

                    _frameSync?.ReceiveFrame(pack.Data);
                    break;
            }
        };
        _client.OnError += (ex) =>
        {
            Debug.LogFormat("异常 >>> {0}", ex);
        };

        _client.Connect();
    }

    private void Update()
    {
        // 1. 网络收发
        _client?.Tick(Time.deltaTime);

        // 2. 帧同步
        _frameSync?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _frameSync?.Stop();
        _client?.Close();
    }
}