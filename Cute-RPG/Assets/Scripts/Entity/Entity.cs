using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 非泛型基础层，存放不需要知道具体子类类型的功能
// 使用时可以统一使用List<EntityBase>来批量管理类所有实体
public abstract class EntityBase : MonoBehaviour
{
}

// 泛型实例层，实体类的基类
// CRTP模式：奇异递归模板模式
// 父类是一个泛型类，子类把自己作为泛型参数传给父类。
// 主要就是在父类中编写方法时，可以直接使用或者返回子类本身的类型，比如避免手动强转等操作
// where语法：约束，表示T必须继承自Entity<T>，子类可以使用自身类的泛型的父类
// 避免了Player : Entity<Enemy>这种写法
public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    public EntityStateManager<T> states { get; protected set; }

    protected virtual void Awake()
    {
        InitializeStateManager();
    }

    // =>是C#函数体内只有一行表达式时候的使用方法，相当于{states = GetComponent<EntityStateManager<T>>()}
    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    protected virtual void HandleStates() => states.Step();

    protected virtual void Update()
    {
        HandleStates();
    }
}