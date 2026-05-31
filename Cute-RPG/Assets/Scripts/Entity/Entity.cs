using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 非泛型基础层，存放不需要知道具体子类类型的功能
// 使用时可以统一使用List<EntityBase>来批量管理类所有实体
public abstract class EntityBase : MonoBehaviour
{
    public Vector3 unsizedPosition => transform.position;
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
    
    // 速度所有实体都通用
    public Vector3 velocity { get; protected set; }
    
    // 加速度函数需要用到的数值
    public float accelerationMultiplier { get; set; } = 1f;  // 加速度倍率

    public float gravityMultiplier { get; set; } = 1f;  // 重力倍率

    public float topSpeedMultiplier { get; set; } = 1f;  // 最高速度倍率

    public float turningDragMultiplier { get; set; } = 1f;  // 转向阻力倍率
 
    public float decelerationMultiplier { get; set; } = 1f;  // 减速度倍率
    
    // 当前水平速度（XZ平面速度）
    public Vector3 lateralVelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z); }
    }

    // 当前垂直速度（Y轴速度）
    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
    }

    // =>是C#函数体内只有一行表达式时候的使用方法，相当于{states = GetComponent<EntityStateManager<T>>()}
    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
    {
        // 判断方向是否有效（不为零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 计算当前速度在目标方向上的投影速度（标量）
            var speed = Vector3.Dot(direction, lateralVelocity);
            // 计算当前速度在目标方向上的向量部分
            var velocity = direction * speed;
            // 计算当前速度中垂直于目标方向的部分（转向速度）
            var turningVelocity = lateralVelocity - velocity;
            // 计算转向阻力对应的速度变化量（根据转向阻力系数和时间增量）
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            // 计算最大允许速度（考虑速度倍率）
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            // 如果当前速度未达最大速度，或当前速度与目标方向相反，则加速
            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                // 增加速度，受加速度倍率和时间影响
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                // 限制速度在[-最大速度, 最大速度]范围内
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }

            // 重新计算目标方向速度向量
            velocity = direction * speed;
            // 将转向速度平滑减小到0，实现自然转向过渡
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            // 更新横向速度为目标方向速度与转向速度之和
            lateralVelocity = velocity + turningVelocity;
        }
    }

    protected virtual void HandleController()
    {
        transform.position += velocity * Time.deltaTime;
    }
    
    
    protected virtual void HandleStates() => states.Step();

    protected virtual void Update()
    {
        HandleStates();
        HandleController();
    }
    
    // 让角色立即朝向某个方向（瞬间转向）
    public virtual void FaceDirection(Vector3 direction)
    {
        // 如果方向向量有效（不是零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 生成一个面向 direction 方向的旋转（保持世界Y轴为上）
            var rotation = Quaternion.LookRotation(direction, Vector3.up);

            // 直接设置物体的旋转
            transform.rotation = rotation;
        }
    }
    // 让角色按一定旋转速度朝向某个方向（平滑转向）
    public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
    {
        // 必须是有效的方向
        if (direction != Vector3.zero)
        {
            // 当前旋转
            var rotation = transform.rotation;
            // 本帧允许的最大旋转角度（受 Time.deltaTime 影响）
            var rotationDelta = degreesPerSecond * Time.deltaTime;
            // 目标旋转
            var target = Quaternion.LookRotation(direction, Vector3.up);
            // 按最大旋转速度逐渐逼近目标旋转
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
        }
    }
}