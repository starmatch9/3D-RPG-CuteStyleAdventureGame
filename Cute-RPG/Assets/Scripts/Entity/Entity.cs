using UnityEngine;

// 非泛型基础层，存放不需要知道具体子类类型的功能
// 使用时可以统一使用List<EntityBase>来批量管理类所有实体（规避了CRTP的弱点）
public abstract class EntityBase : MonoBehaviour
{
    // 用来注册委托的
    public EntityEvents entityEvents;

    // 角色控制器，自带碰撞检测的胶囊体物理组件，自带Move()等方法
    public CharacterController controller { get; protected set; } // 角色控制器组件

    // 胶囊体的高
    public float height => controller.height;

    // 胶囊体的粗
    public float radius => controller.radius;

    // 胶囊体相对物体远点的偏移量（只是便宜量）
    public Vector3 center => controller.center;

    // 在Camera模块会用到，表示物体的原点（而非胶囊体的原点）
    public Vector3 unsizedPosition => transform.position;

    // 胶囊体原点的世界位置
    public Vector3 position => transform.position + center;

    // 注意get和set这个语法，不是帮你写新方法，是限制get和set分别的访问权限
    // get和set前面没表权限那就是{}前面的权限
    // 比如以下这行：public get，protected set。
    // 当使用set的时候，只有当前类和子类可以访问
    public bool isGrounded { get; protected set; } = true; // 是否在地面上

    // 记录距离上一次离开地面经过的时间
    public float lastGroundTime { get; protected set; }

    // 进行地面探测时多探测一点距离
    protected readonly float m_groundOffset = 0.1f;

    // 使用Physics的SphereCast射线检测
    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        // 计算球形检测的有效距离，确保球形的检测范围符合预期
        var castDistance = Mathf.Abs(distance - radius);

        // 使用物理引擎进行球形碰撞检测
        return Physics.SphereCast(position, radius, direction,
            out hit, castDistance, layer, queryTriggerInteraction);
    }
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
    public float accelerationMultiplier { get; set; } = 1f; // 加速度倍率

    public float gravityMultiplier { get; set; } = 1f; // 重力倍率

    public float topSpeedMultiplier { get; set; } = 1f; // 最高速度倍率

    public float turningDragMultiplier { get; set; } = 1f; // 转向阻力倍率

    public float decelerationMultiplier { get; set; } = 1f; // 减速度倍率


    #region 生命周期

    protected virtual void Awake()
    {
        InitializeController();
        InitializeStateManager();
    }

    protected virtual void Update()
    {
        if (!controller.enabled)
        {
            return;
        }

        HandleStates(); // 一个是状态逻辑更新
        HandleGround();
        HandleController(); // 一个物理逻辑更新（目前只有物理位置）
    }

    #endregion

    // 转为初始化服务

    #region Initialize方法

    // 初始化角色控制器组件（CharacterController）
    // 负责角色的基本移动、碰撞等物理交互
    protected virtual void InitializeController()
    {
        // 获取当前物体上的 CharacterController 组件
        controller = GetComponent<CharacterController>();
        // 如果没有，就动态添加一个 CharacterController
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        // skinWidth 表示碰撞器表面到实际碰撞检测边界的距离（防止卡住用的小偏移）
        controller.skinWidth = 0.005f;
        // minMoveDistance 为最小移动距离（设为 0 表示即使移动非常小也会被检测到）
        controller.minMoveDistance = 0;
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

    #endregion

    // 每帧执行，做一件事，动作清晰

    #region Handle方法

    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        transform.position += velocity * Time.deltaTime;
    }


    protected virtual void HandleStates() => states.Step();

    protected virtual void HandleGround()
    {
        // 距离计算：角色半高 + 地面检测的额外偏移量
        var distance = (height * 0.5f) + m_groundOffset;

        // 向下发射球体射线检测地面，并且角色的垂直速度 ≤ 0（下落或静止状态）
        if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            // 如果之前不在地面状态
            if (!isGrounded)
            {
                EnterGround();
            }
        }
        else
        {
            // 射线未检测到地面，则视为离开地面
            ExitGround();
        }
    }

    #endregion


    #region 人物移动

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

    // 让角色立即朝向某个方向（瞬间转向）
    public virtual void FaceDirection(Vector3 direction)
    {
        // 向量长度的平方，计算更快
        // 如果方向向量有效（不是零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 生成一个面向 direction 方向的旋转（保持世界Y轴为上）
            // 让旋转的Z轴朝向direction，正上方Y轴指向世界向上的方向
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            // 直接设置物体的旋转
            transform.rotation = rotation;
        }
    }

    // 让角色按一定旋转速度朝向某个方向（平滑转向）（第二个参数）（函数重载）
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

    // 平滑减速，速度逐渐趋近于 0（水平速度减速）
    public virtual void Decelerate(float deceleration)
    {
        // 计算本帧的减速度（decelerationMultiplier 可用于调节全局减速效果）
        var delta = deceleration * decelerationMultiplier * Time.deltaTime;
        // 将 lateralVelocity（水平速度向量）逐渐插值到 Vector3.zero（完全停止）
        // 第三个参数是本帧允许的最大速度变化量
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }

    #endregion


    #region 地面检测

    // 进入地面状态（角色刚刚落地时调用）
    protected virtual void EnterGround()
    {
        // 只有当前不是地面状态时才执行（防止重复触发）
        if (!isGrounded)
        {
            // 标记角色已经在地面上
            isGrounded = true;
            // 触发“进入地面”的事件（例如播放落地动画、音效）
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    // 离开地面状态（角色刚刚离开地面时调用）
    protected virtual void ExitGround()
    {
        // 只有当前在地面状态时才执行
        if (isGrounded)
        {
            // 标记角色不在地面
            isGrounded = false;
            // 解除与地面的父子关系（如果站在移动平台上，需要解绑）
            transform.parent = null;
            // 记录离开地面的时间（可能用于跳跃缓冲或着陆判断）
            lastGroundTime = Time.time;
            // 限制垂直速度：如果正在向下运动，不改变；如果有向上的速度，则保留（防止离地瞬间速度异常）
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            // 触发“离开地面”的事件（例如播放起跳动画）
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    // 将角色吸附到地面（防止悬空）
    public virtual void SnapToGround(float force)
    {
        // 只有接触地面，且垂直速度是向下的（y <= 0）才生效
        if (isGrounded && (verticalVelocity.y <= 0))
        {
            // 将垂直速度设置为一个恒定向下的力（防止离地浮空）
            verticalVelocity = Vector3.down * force;
        }
    }

    #endregion
}