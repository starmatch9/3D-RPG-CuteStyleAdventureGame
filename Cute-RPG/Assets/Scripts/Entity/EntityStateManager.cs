using System;
using System.Collections.Generic;
using UnityEngine;

// Entity依旧只是接口
public abstract class EntityStateManager : MonoBehaviour
{
    public EntityStateManagerEvents events;
}

// 
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();

    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    // 要求子类实现获取List的接口
    protected abstract List<EntityState<T>> GetStateList();

    public EntityState<T> current { get; protected set; }
    public EntityState<T> last { get; protected set; }
    
    public int index => m_list.IndexOf(current);
    
    public int lastIndex => m_list.IndexOf(last);

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
    
    public virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

   public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);

        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }
    
    public virtual void Change(EntityState<T> to)
    {
        // 确保目标状态不为空且游戏未暂停（Time.timeScale > 0）
        if (to != null && Time.timeScale > 0)
        {
            // 如果有当前状态，调用退出逻辑并触发退出事件
            if (current != null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }

            // 切换到目标状态，调用进入逻辑并触发进入事件和状态切换事件
            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }
}