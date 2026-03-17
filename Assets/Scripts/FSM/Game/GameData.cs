using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameData
{
    /// <summary>
    /// 当前游戏剩余的重试次数。
    /// </summary>
    public int retries;

    /// <summary>
    /// 所有关卡的存档数据数组。
    /// </summary>
    public LevelData[] levels;

    /// <summary>
    /// 存档创建时间，字符串格式。
    /// </summary>
    public string createdAt;

    /// <summary>
    /// 存档最后更新时间，字符串格式。
    /// </summary>
    public string updatedAt;

    /// <summary>
    /// 将当前 GameData 对象序列化为 JSON 字符串。
    /// </summary>
    /// <returns>JSON 格式字符串。</returns>
    public virtual string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// 从 JSON 字符串反序列化得到 GameData 对象。
    /// </summary>
    /// <param name="json">包含游戏数据的 JSON 字符串。</param>
    /// <returns>反序列化后的 GameData 对象。</returns>
    public static GameData FromJson(string json)
    {
        return JsonUtility.FromJson<GameData>(json);
    }

    public static GameData Create()
    {
        return new GameData()
        {
            // 初始重试次数使用当前游戏实例的默认值
            retries = Game.Instance.initialRetries,
            // 创建和更新时间均设为当前UTC时间字符串
            createdAt = DateTime.UtcNow.ToString(),
            updatedAt = DateTime.UtcNow.ToString(),
            // 生成所有关卡的默认存档数据（锁定状态）
            levels = Game.Instance.levels.Select((level) =>
            {
                return new LevelData()
                {
                    locked = level.locked
                };
            }).ToArray()
        };
    }

    public virtual int TotalStars()
    {
        // 通过 Aggregate 方法累加所有关卡的CollectedStars()返回值
        return levels.Aggregate(0, (acc, level) =>
        {
            var total = level.CollectedStars();
            return acc + total;
        });
    }

    /// <summary>
    /// 计算并返回所有关卡中已收集的金币总数。
    /// </summary>
    /// <returns>金币总数。</returns>
    public virtual int TotalCoins()
    {
        // 通过 Aggregate 方法累加所有关卡的coins字段
        return levels.Aggregate(0, (acc, level) => acc + level.coins);
    }

}

