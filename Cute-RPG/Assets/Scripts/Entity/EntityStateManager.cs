using System;
using System.Collections.Generic;
using UnityEngine;

// 依旧拆成两层，这里可以把所有不依赖泛型的系统或者模块都挂载在这里
public abstract class EntityStateManager : MonoBehaviour
{
    public EntityStateManagerEvents events;
}

// 专门用来放管状态机的操作（大量用到泛型）
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();

    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    public EntityState<T> current { get; protected set; }
    
    //public EntityState<T> last { get; protected set; }  // 主要让动画机知道从那一帧过来，可以插播过渡，这里好像用不到
    
    public int index => m_list.IndexOf(current);
    
    //public int lastIndex => m_list.IndexOf(last);

    public T entity { get; protected set; }

    #region 生命周期 LifeCycle

    // 这里只初始化管理器需要的entity、状态列表，状态本身的神秘周期由Entity控制Update->step()
    protected virtual void Start()
    {
        InitializeStates();
        InitializeEntity();
    }
    
    #endregion
    
    #region Initialize Functions 初始化方法
    
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
    
    #endregion
    
    // 要求子类实现获取List的接口
    protected abstract List<EntityState<T>> GetStateList();
    
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

    // 这个方法不用传实例，只用写个名儿就行，整个状态机的每个状态只有表里面一份实例
   public virtual void Change<TState>() where TState : EntityState<T>
    {
        // Type类型，同一个类的实例typeof获取的Type对象一样
        var type = typeof(TState);

        // 把PlayerState类型转为EntityState<PlayerState>类型
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
                //last = current;
            }

            // 切换到目标状态，调用进入逻辑并触发进入事件和状态切换事件
            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }
}