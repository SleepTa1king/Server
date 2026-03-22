using Client;
using UnityEngine;
using Utils;
using Main;

public class ClientMain : MonoBehaviour
{
    public static ClientMain Instance { get; private set; }

    private SocketClient _client;
    private ClientFrameSync _frameSync;
    private int _myPlayerId = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 订阅 UI 发来的登录请求信号
        Signals.Get<LoginRequestSignal>().AddListener(OnLoginRequested);

        _client = new SocketClient("127.0.0.1", 6854);

        _client.OnConnectSuccess += () =>
        {
            Debug.Log("网络连接成功，发送哈希 ID...");
            var req = new CSLogin { PlayerId = _myPlayerId };
            _client.Send((ushort)SocketEvent.cs_login, ProtoHelper.Serialize(req));
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
                    var loginMsg = ProtoHelper.Deserialize<SCLogin>(pack.Data);
                    Debug.Log($"[Login] 登录成功，最终确认 ID: {loginMsg.PlayerId}");

                    // 派发登录成功信号，UI 框架收到后会自动处理（例如关闭登录窗口）
                    Signals.Get<LoginSuccessSignal>().Dispatch();

                    _frameSync = new ClientFrameSync(_client, loginMsg.PlayerId);
                    PlayerManager.Instance.SpawnPlayer(loginMsg.PlayerId, Vector3.zero, isLocal: true);
                    _frameSync.Start();
                    break;

                case SocketEvent.sc_frame:
                    var frame = ProtoHelper.Deserialize<SCFrame>(pack.Data);
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

        _client.OnError += (ex) => Debug.LogFormat("异常 >>> {0}", ex);
    }

    /// <summary>
    /// 当监听到 UI 的登录信号时调用
    /// </summary>
    private void OnLoginRequested(string playerName)
    {
        _myPlayerId = playerName.GetHashCode();
        Debug.Log($"[Client] 收到登录信号，玩家名: {playerName}, Hash: {_myPlayerId}");
        _client.Connect();
    }

    private void Update()
    {
        _client?.Tick(Time.deltaTime);
        _frameSync?.Tick(Time.deltaTime);
    }

    private void OnDestroy()
    {
        Signals.Get<LoginRequestSignal>().RemoveListener(OnLoginRequested);
        _frameSync?.Stop();
        _client?.Close();
    }
}