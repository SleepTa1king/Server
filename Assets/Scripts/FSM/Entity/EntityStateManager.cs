using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityStateManagers : MonoBehaviour
{
    //状态管理相关事件集合
    public EntityStateManagerEvents events;
}

public abstract class EntityStateManager<T> : EntityStateManagers where T :Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();
    protected Dictionary<Type,EntityState<T>> m_state = new Dictionary<Type, EntityState<T>>();
    
    //当前状态
    public EntityState<T> current { get; protected set; }
    //上一个状态
    public EntityState<T> last { get; protected set; }

    public int index => m_list.IndexOf(current);
    public int lastIndex => m_list.IndexOf(last);
    public T entity { get; protected set; }
    protected virtual void Start()
    {
        InitializeStates();
        InitializeEntity();
    }
    //实体类型赋值
    protected virtual void InitializeEntity() => entity = GetComponent<T>();
    protected abstract List<EntityState<T>> GetStateList();
    //初始化状态列表
    protected virtual void InitializeStates()
    {
        m_list = GetStateList();
        foreach(var state in m_list)
        {
            var type = state.GetType();
            if(!m_state.ContainsKey(type))
            {
                m_state.Add(type, state);
            }
        }

        if(m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    public virtual void Step(float deltaTime)
    {
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity, deltaTime);
        }
    }


    public virtual void Change<TState>() where TState:EntityState<T>
    {
        var type = typeof(TState);

        if(m_state.ContainsKey(type))
        {
            Change(m_state[type]);
        }
    }

    public virtual void Change(EntityState<T> to)
    {
        if(to!=null && Time.timeScale >0)
        {
            if(current !=null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }

            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }

    public virtual void OnContact(Collider other)
    {
        if(current != null && Time.timeScale >0)
        {
            current.OnContact(entity, other);
        }
    }
    public virtual bool ContainsStateOfType(Type type) => m_state.ContainsKey(type); 

    public virtual bool IsCurrentOfState(Type type)
    {
        if(current == null)
        {
            return false;
        }

        return current.GetType() == type;
    }
}