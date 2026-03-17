using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

/// <summary>
/// 用于管理实体状态机时间的序列化类
/// 可以在Inspector中绑定事件
/// </summary>
[Serializable]
public class EntityStateManagerEvents
{
    public UnityEvent onChange;

    public UnityEvent<Type> onEnter;

    public UnityEvent<Type> onExit;

}
