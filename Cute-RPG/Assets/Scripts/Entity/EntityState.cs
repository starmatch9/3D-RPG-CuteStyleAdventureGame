// 状态类的基类，where将T锁定在实体，只有实体才能绑定到状态基类

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class EntityState<T> where T : Entity<T>
{
    // 状态进入时触发的事件
    public UnityEvent onEnter;

    // 状态退出时触发的事件
    public UnityEvent onExit;

    // 记录实体进入该状态后经过的时间，单位为秒。
    public float timeSinceEntered { get; protected set; }


    // --下面这几个都是模板方法模式，Enter/Exit/Step定义骨架，OnEnter/OnExit/OnStep由子类实现

    // 进入该状态时调用，重置计时，触发进入事件，并调用子类实现的 OnEnter。
    public void Enter(T entity)
    {
        // 重置计时
        timeSinceEntered = 0;
        // 触发外部注册的进入事件回调
        onEnter?.Invoke();
        // 调用子类自定义的进入逻辑
        OnEnter(entity);
    }

    // 退出该状态时调用，触发退出事件，并调用子类实现的 OnExit。
    public void Exit(T entity)
    {
        // 触发外部注册的退出事件回调
        onExit?.Invoke();
        // 调用子类自定义的退出逻辑
        OnExit(entity);
    }

    // 每帧调用一次，执行状态持续期间的逻辑，并更新状态持续时间。
    public void Step(T entity)
    {
        // 调用子类实现的持续运行逻辑
        OnStep(entity);
        // 累计该状态已持续的时间，单位秒
        timeSinceEntered += Time.deltaTime;
    }

    // 当状态被激活时调用，用于初始化该状态相关逻辑。
    protected abstract void OnEnter(T entity);

    // 当状态被切换出去时调用，用于清理该状态相关逻辑。
    protected abstract void OnExit(T entity);

    // 每帧调用，用于处理该状态下的持续逻辑。
    protected abstract void OnStep(T entity);

    public abstract void OnContact(T entity, Collider other);

    public static EntityState<T> CreateFromString(string typeName)
    {
        return (EntityState<T>)System.Activator.CreateInstance(System.Type.GetType(typeName));
    }


    // 自然数组转为状态类的List
    public static List<EntityState<T>> CreateListFromStringArray(string[] array)
    {
        var list = new List<EntityState<T>>();

        foreach (var typeName in array)
        {
            list.Add(CreateFromString(typeName));
        }

        return list;
    }
}