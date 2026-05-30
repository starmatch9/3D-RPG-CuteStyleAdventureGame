using System;
using System.Collections.Generic;
using UnityEngine;

// Entity依旧只是接口
public abstract class EntityStateManager : MonoBehaviour
{
}

// 
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();

    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    // 要求子类实现获取List的接口
    protected abstract List<EntityState<T>> GetStateList();

    public EntityState<T> current { get; protected set; }

    public T entity { get; protected set; }

    protected virtual void Start()
    {
        InitializeStates();
        InitializeEntity();
    }

    protected virtual void InitializeEntity() => entity = GetComponent<T>();

    protected virtual void InitializeStates()
    {
        m_list = GetStateList();
        foreach (var state in m_list)
        {
            var type = state.GetType();
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }
        }

        if (m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    // 轮询
    public virtual void Step()
    {
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }
}