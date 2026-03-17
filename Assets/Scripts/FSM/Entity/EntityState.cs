

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public abstract class EntityState<T> where T : Entity<T>
{

    public UnityEvent onEnter;
    public UnityEvent onExit;

    public float timeSinceEntered { get; protected set; }

    public void Enter(T entity)
    {
        timeSinceEntered = 0;
        onEnter?.Invoke();
        OnEnter(entity);
    }

    public void Step(T entity, float deltaTime)
    {
        OnStep(entity, deltaTime);
        timeSinceEntered += deltaTime;
    }

    public void Exit(T entity)
    {
        onExit?.Invoke();
        OnExit(entity);
    }
    protected abstract void OnEnter(T entity);
    protected abstract void OnStep(T entity, float deltaTime);   
    protected abstract void OnExit(T entity);
    public abstract void OnContact(T entity,Collider other);

    private static EntityState<T> CreateEntityStateFromString(string typeName)
    {
        return (EntityState<T>)Activator.CreateInstance(Type.GetType(typeName));
    }
    public static List<EntityState<T>> CreateListFromStringArray(string[] array)
    {
        List<EntityState<T>> list = new List<EntityState<T>>();

        foreach(var typeName in array)
        {
            list.Add(CreateEntityStateFromString(typeName));
        }
        return list;
    }



}

