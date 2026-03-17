using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
public class Game:Singleton<Game>
{
    /// <summary>
		/// 当重试次数改变时触发，带有当前重试次数参数。
		/// </summary>
		public UnityEvent<int> OnRetriesSet;

		/// <summary>
		/// 当请求保存游戏时触发。
		/// </summary>
		public UnityEvent OnSavingRequested;

		/// <summary>
		/// 初始重试次数，游戏开始时赋值给 retries。
		/// </summary>
		public int initialRetries = 3;

		/// <summary>
		/// 游戏包含的所有关卡列表。
		/// </summary>
		public List<GameLevel> levels;

		/// <summary>
		/// 当前剩余的重试次数，封装字段。
		/// </summary>
		protected int m_retries;

		/// <summary>
		/// 当前游戏数据索引，标识加载或保存的存档槽。
		/// </summary>
		protected int m_dataIndex;

		/// <summary>
		/// 游戏存档创建时间。
		/// </summary>
		protected DateTime m_createdAt;

		/// <summary>
		/// 游戏存档最后更新时间。
		/// </summary>
		protected DateTime m_updatedAt;

		/// <summary>
		/// 当前游戏剩余的重试次数属性，设置时会触发 OnRetriesSet 事件。
		/// </summary>
		public int retries
		{
			get { return m_retries; }
			set
			{
				m_retries = value;
				// 通知监听者重试次数已改变
				OnRetriesSet?.Invoke(m_retries);
			}
		}

    

    public virtual void LoadState(int index, GameData data)
    {
        m_dataIndex = index;
        m_retries = data.retries;
        m_createdAt = DateTime.Parse(data.createdAt);
        m_updatedAt = DateTime.Parse(data.updatedAt);

        // 依次将各关卡加载对应的存档数据
        for (int i = 0; i < data.levels.Length; i++)
        {
            levels[i].LoadState(data.levels[i]);
        }
    }
    public static void LockCursor(bool value = true)
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        Cursor.visible = true;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
    }
}

