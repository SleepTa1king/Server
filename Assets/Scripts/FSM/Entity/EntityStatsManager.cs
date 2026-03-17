using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class EntityStatsManager<T> : MonoBehaviour where T:EntityStats<T>
{
    public T[] stats;
    /// <summary>
    /// 当前激活的属性
    /// </summary>
    public T current { get; protected set; }

    public virtual void Change(int to)
    {
        if (to >= 0 && to < stats.Length)
        {
            if (current != stats[to])
            {
                current = stats[to];
            }
        }
    }

    protected virtual void Start()
    {
        if(stats.Length >0)
        {
            current = stats[0];
        }
    }


}

