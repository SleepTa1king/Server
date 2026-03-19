using Client;
using UnityEngine;

public class ClientMain : MonoBehaviour
{
    SocketClient _client;
    ClientFrameSync _frameSync;

    private void Awake()
    {
        _client = new SocketClient("127.0.0.1", 6854);

        // TODO: 实际应用中应该根据服务器返回的消息来确定 playerId
        int myPlayerId = 1;
        _frameSync = new ClientFrameSync(_client, playerId: myPlayerId);

        _client.OnConnectSuccess += () =>
        {
            Debug.Log("连接成功");
            
            // 1. 生成本地玩家
            PlayerManager.Instance.SpawnPlayer(myPlayerId, Vector3.zero, isLocal: true);
            
            // 2. 开启同步循环
            _frameSync.Start();
        };
        _client.OnDisconnect += () =>
        {
            Debug.Log("断开连接");
            _frameSync.Stop();
            PlayerManager.Instance.ClearAll();
        };
        _client.OnReceive += (pack) =>
        {
            switch ((SocketEvent)pack.Type)
            {
                case SocketEvent.sc_frame:
                    // 收到服务端帧数据
                    var frame = ProtoHelper.Deserialize<SCFrame>(pack.Data);
                    
                    // 在执行帧之前，检查是否有新玩家需要生成
                    foreach(var input in frame.Inputs)
                    {
                        if (PlayerManager.Instance.GetPlayer(input.PlayerId) == null)
                        {
                            Debug.Log($"[ClientMain] 发现新玩家 {input.PlayerId}，正在生成...");
                            PlayerManager.Instance.SpawnPlayer(input.PlayerId, Vector3.zero, isLocal: false);
                        }
                    }

                    _frameSync.ReceiveFrame(pack.Data);
                    break;
            }
        };
        _client.OnError += (ex) =>
        {
            Debug.LogFormat("异常 >>> {0}", ex);
        };

        // 帧数据到达时，执行游戏逻辑
        _frameSync.OnFrameExecute += (frame) =>
        {
            foreach (var input in frame.Inputs)
            {
                Debug.LogFormat("帧{0} 玩家{1} 移动({2},{3})",
                    frame.FrameId, input.PlayerId, input.MoveX, input.MoveZ);

                // TODO: 根据 input 驱动对应角色
                // var player = PlayerManager.GetPlayer(input.PlayerId);
                // player.ApplyInput(input);
            }
        };

        _client.Connect();
    }

    private void Update()
    {
        // 1. 网络收发
        _client?.Tick(Time.deltaTime);

        // 2. 帧同步（内部控制发送频率 + 执行服务端帧）
        _frameSync?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        _frameSync?.Stop();
        _client?.Close();
    }
}