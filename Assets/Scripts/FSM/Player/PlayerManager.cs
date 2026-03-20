using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家管理器 —— 维护 ID 与玩家实体的映射，处理生命周期
/// </summary>
public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PlayerManager");
                    _instance = go.AddComponent<PlayerManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Prefabs")]
    public Player playerPrefab; // 玩家预制体，需在 Inspector 赋值

    private Dictionary<int, Player> _playerDic = new Dictionary<int, Player>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// 根据服务器 ID 获取玩家脚本
    /// </summary>
    public Player GetPlayer(int id)
    {
        _playerDic.TryGetValue(id, out var player);
        return player;
    }

    /// <summary>
    /// 生成并注册一个玩家
    /// </summary>
    /// <param name="id">服务器分配的 PlayerId</param>
    /// <param name="position">初始位置</param>
    /// <param name="isLocal">是否为本地控制的玩家</param>
    public Player SpawnPlayer(int id, Vector3 position, bool isLocal)
    {
        if (_playerDic.ContainsKey(id))
        {
            Debug.LogWarning($"[PlayerManager] 玩家 {id} 已存在，跳过创建。");
            return _playerDic[id];
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerManager] PlayerPrefab 未分配！请在 Inspector 中设置。");
            return null;
        }

        Player newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        newPlayer.name = $"Player_{id}{(isLocal ? " (Local)" : "")}";
        
        _playerDic.Add(id, newPlayer);

        // 如果是本地玩家，关联相机
        if (isLocal)
        {
            var cam = FindObjectOfType<PlayerCamera>();
            if (cam != null)
            {
                cam.player = newPlayer;
                cam.Reset(); // 立即重新对齐位置
            }
        }

        return newPlayer;
    }

    /// <summary>
    /// 移除玩家
    /// </summary>
    public void RemovePlayer(int id)
    {
        if (_playerDic.TryGetValue(id, out var player))
        {
            if (player != null) Destroy(player.gameObject);
            _playerDic.Remove(id);
        }
    }

    /// <summary>
    /// 获取所有在线玩家
    /// </summary>
    public IEnumerable<Player> GetAllPlayers()
    {
        return _playerDic.Values;
    }

    /// <summary>
    /// 清空所有玩家
    /// </summary>
    public void ClearAll()
    {
        foreach (var p in _playerDic.Values)
        {
            if (p != null) Destroy(p.gameObject);
        }
        _playerDic.Clear();
    }
}
