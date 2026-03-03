using Client;
using UnityEngine;

public class ClientMain : MonoBehaviour
{
    SocketClient _client;
    ClientFrameSync _frameSync;

    private void Awake()
    {
        _client = new SocketClient("127.0.0.1", 6854);

        _frameSync = new ClientFrameSync(_client, playerId: 1);

        _client.OnConnectSuccess += () =>
        {
            Debug.Log("连接成功");
            _frameSync.Start();
        };
        _client.OnDisconnect += () =>
        {
            Debug.Log("断开连接");
            _frameSync.Stop();
        };
        _client.OnReceive += (pack) =>
        {
            switch ((SocketEvent)pack.Type)
            {
                case SocketEvent.sc_frame:
                    // 收到服务端帧数据
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