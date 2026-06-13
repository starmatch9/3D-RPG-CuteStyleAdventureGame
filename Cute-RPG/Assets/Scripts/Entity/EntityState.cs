// 状态类的基类，where将T锁定在实体，只有实体才能绑定到状态基类

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 使用CRTP，这里也直接可以获取到传入的对象类型是什么，不用再子类里面做麻烦的识别再强转
public abstract class EntityState<T> where T : Entity<T>
{
    // 状态进入时触发的事件
    public UnityEvent onEnter;

    // 状态退出时触发的事件
    public UnityEvent onExit;

    // 记录实体进入该状态后经过的时间，单位为秒。
    public float timeSinceEntered { get; protected set; }


    // --下面这几个都是模板方法模式，Enter/Exit/Step定义骨架，OnEnter/OnExit/OnStep由子类实现
    
    // 被激活调用，初始化状态
    public void Enter(T entity)
    {
        // 重置计时
        timeSinceEntered = 0;
        // 触发外部注册的进入事件回调
        onEnter?.Invoke();
        // 调用子类自定义的进入逻辑
        OnEnter(entity);
    }
    
    // 切出去调用，清理状态
    public void Exit(T entity)
    {
        // 触发外部注册的退出事件回调
        onExit?.Invoke();
        // 调用子类自定义的退出逻辑
        OnExit(entity);
    }
    
    // 每帧调用，处理状态的行为
    public void Step(T entity)
    {
        // 调用子类实现的持续运行逻辑
        OnStep(entity);
        // 累计该状态已持续的时间，单位秒
        timeSinceEntered += Time.deltaTime;
    }
    
    protected abstract void OnEnter(T entity);
    
    protected abstract void OnExit(T entity);
    
    protected abstract void OnStep(T entity);

    #region 工具方法 Tool

    public static EntityState<T> CreateFromString(string typeName)
    {
        // System.Activator.CreateInstance(Type)用来创建一个类型的实例
        // System.Type.GetType(typeName)通过类名返回Type
        // 向上类型转换
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

    #endregion
}