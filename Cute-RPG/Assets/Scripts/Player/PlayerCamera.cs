using Cinemachine;
using UnityEngine;

// 强制要求必须CinemachineVirtualCamera组件，否则会自动添加
[RequireComponent(typeof(CinemachineVirtualCamera))]
[AddComponentMenu("Player/Player Camera")]
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")] // 相机设置
    public Player player;
    public float maxDistance = 15f;    // 相机与目标的最大距离
    public float initialAngle = 20f;   // 初始的俯仰角，只是仰俯角，偏航角是玩家朝向默认的
    public float heightOffset = 1f;    // 相机相对玩家的垂直偏移量

    [Header("Following Settings")] // 跟随设置
    public float verticalUpDeadZone = 0.15f;      // 在地面时，相机向上跟随的死区
    public float verticalDownDeadZone = 0.15f;    // 在地面时，相机向下跟随的死区
    public float verticalAirUpDeadZone = 0.15f;   // 在空中时，相机向上跟随的死区
    public float verticalAirDownDeadZone = 0.15f; // 在空中时，相机向下跟随的死区
    public float maxVerticalSpeed = 10f;          // 相机在地面时的最大垂直跟随速度
    public float maxAirVerticalSpeed = 100f;      // 相机在空中时的最大垂直跟随速度

    [Header("Orbit Settings")] // 相机环绕设置
    public bool canOrbit = true; // 是否允许相机跟随鼠标环绕
    public bool canOrbitWithVelocity = true; // 是否允许通过速度带动相机旋转
    public float orbitVelocityMultiplier = 5; // 角色速度带动相机旋转的倍率

    
    //限制仰俯角
    [Range(0, 90)]
    public float verticalMaxRotation = 80; // 相机俯仰角最大值

    [Range(-90, 0)]
    public float verticalMinRotation = -20; // 相机俯仰角最小值

    // 内部变量
    
    // 前缀m就是成员的意思
    // Brain是大脑，挂载在原生Camera下，找到激活的VirtualCamera，让原生相机显示这个激活虚拟相机的画面
    // 这个第三人称跟随组件是一个算法组件，除了这个还有第一人称第二人称，没有算法组件虚拟相机只能固定在原地
    protected CinemachineBrain m_brain; // Cinemachine控制大脑
    protected CinemachineVirtualCamera m_camera;  // 相机对象
    protected Cinemachine3rdPersonFollow m_cameraBody; // 3D跟随组件

    // 这个就是目标组件
    protected Transform m_target; // 相机跟随的目标点（在玩家上方一点）
    
    // 重温一下：
    // Yaw：偏航角
    // Pitch：仰俯角
    // Roll：翻滚角
    protected float m_cameraDistance;
    protected float m_cameraTargetYaw;
    protected float m_cameraTargetPitch;
    protected Vector3 m_cameraTargetPosition; // 相机目标位置（用于插值过渡）

    // 前缀k表示常量
    protected string k_targetName = "Player Follower Camera Target"; // 临时目标对象的名称
    
    #region 生命周期
    
    protected virtual void Start()
    {
        InitializeComponents();
        InitializeFollower();
        InitializeCamera();
    }
    
    protected virtual void LateUpdate()
    {
        HandleOrbit();// 输入环绕
        HandleVelocityOrbit();// 速度驱动环绕
        HandleOffset();// 高度跟随
        MoveTarget();// 单独每帧刷新Target空物体的位置和旋转
    }
    
    #endregion

    #region 初始化方法

    protected virtual void InitializeComponents()
    {
        if (!player)
        {
            // 如果没有指定 player，则在场景中自动寻找
            player = FindObjectOfType<Player>();
        }

        m_camera = GetComponent<CinemachineVirtualCamera>();
        m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>(); // 如果没有这个算法组件会固定在原地
        m_brain = Camera.main.GetComponent<CinemachineBrain>();
    }
    
    protected virtual void InitializeFollower()
    {
        // 创建一个名为的空物体（后续的方法会设为跟随对象）
        m_target = new GameObject(k_targetName).transform;
        m_target.position = player.transform.position; // 先让被跟随目标的位置为玩家位置
    }
    
    protected virtual void InitializeCamera()
    {
        m_camera.Follow = m_target.transform; //Follow：决定相机移动跟随哪个物体
        m_camera.LookAt = player.transform;   //LookAt：决定相机朝向看向哪个物体

        Reset(); // 刷新一遍成员变量。更新虚拟相机的状态
    }

    #endregion

    #region 帧处理方法

    // Unity是左手坐标系
    // 食指y，中指x，拇指z
    // 摄像机朝向+z
    protected virtual void HandleOrbit()
    {
        // 判断是否允许手动环绕相机
        if (canOrbit)
        {
            // 从玩家输入系统获取“视角方向输入”
            // 鼠标移动或摇杆输入会返回一个二维向量
            //   x左右（控制Yaw，水平旋转）
            //   z上下（控制Pitch，垂直旋转）
            var direction = player.inputs.GetLookDirection();

            // sqrMagnitude 表示向量的平方长度，用于判断是否有输入
            // 如果输入为零向量（没有移动鼠标/摇杆），就不需要修改相机
            if (direction.sqrMagnitude > 0)
            {
                // 判断玩家是否正在使用鼠标作为输入设备
                // -使用鼠标时：输入是“即时的”，不需要乘 Time.deltaTime
                // -使用手柄时：输入是“按帧累积的”，需要乘 Time.deltaTime 保持平滑
                var usingMouse = player.inputs.IsLookingWithMouse();

                // 根据输入设备选择不同的时间因子
                // -鼠标：乘 Time.timeScale（鼠标本身就是绝对位移，不是连续的速度值，直接使用timeScale，正常就是1）
                // -手柄：乘 Time.deltaTime（当前帧的秒数，保证旋转平滑、与帧率无关， Time.deltaTime = Time.timeScale × 真实帧时间）
                float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;

                // 每帧变化偏航角（Yaw）
                // direction.x -> 鼠标左右移动
                // 改变偏航角让相机左右旋转
                m_cameraTargetYaw += direction.x * deltaTimeMultiplier;   // 目前调节不了灵敏度，可以在这里加

                // 每帧变化仰俯角（Pitch）
                // direction.x -> 鼠标上下移动
                // direction.z > 0，鼠标往上移动相机Pitch就减小（往上看）
                // direction.z < 0，鼠标往下移动相机Pitch就增大（往下看）
                m_cameraTargetPitch -= direction.z * deltaTimeMultiplier; //用-=符合习惯

                // 将相机的垂直旋转角度限制在一定范围内
                // 避免玩家把相机拉到头顶或者穿透地面
                m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
            }
        }
    }
    
    
    // 处理偏移（带死区的跟随逻辑）
    protected virtual void HandleOffset()
    {
        var target = player.unsizedPosition + Vector3.up * heightOffset;
        var previousPosition = m_cameraTargetPosition;
        var targetHeight = previousPosition.y;
        // 地面跟随（超过死区才跟随）
        if (player.isGrounded /* || VerticalFollowingStates()*/ )
        {
            if (target.y > previousPosition.y + verticalUpDeadZone)
            {
                // 玩家上升时，相机缓慢跟随
                var offset = target.y - previousPosition.y - verticalUpDeadZone;  // 超出死区的部分
                targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);  // 速度够快才能瞬间跟上，否则平滑跟上
            }
            else if (target.y < previousPosition.y - verticalDownDeadZone)
            {
                // 玩家下降时，相机缓慢跟随
                var offset = target.y - previousPosition.y + verticalDownDeadZone;
                targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
            }
        }
        // 空中跟随，和上面一样
        else if (target.y > previousPosition.y + verticalAirUpDeadZone)
        {
            var offset = target.y - previousPosition.y - verticalAirUpDeadZone;
            targetHeight += Mathf.Min(offset, maxAirVerticalSpeed * Time.deltaTime);
        }
        else if (target.y < previousPosition.y - verticalAirDownDeadZone)
        {
            var offset = target.y - previousPosition.y + verticalAirDownDeadZone;
            targetHeight += Mathf.Max(offset, -maxAirVerticalSpeed * Time.deltaTime);
        }
        // 根据新高度计算Target的位置，后面再刷新
        m_cameraTargetPosition = new Vector3(target.x, targetHeight, target.z);
    }
    
    // 据玩家的速度方向（左右横向速度）来调整相机的偏航角度，营造“相机跟随运动方向”的效果
    protected virtual void HandleVelocityOrbit()
    {
        // 玩家必须在地面上（避免空中漂浮时乱转相机）
        if (canOrbitWithVelocity && player.isGrounded)
        {
            // 把相对于世界坐标的player.velocity转为相当于target的局部坐标
            // 可以理解为三个分量合成一个在新坐标系里重新分为三个，和向量都是同一个
            var localVelocity = m_target.InverseTransformVector(player.velocity);

            // 此时localVelocity三个轴的分量就都可以用于计算相机的偏移了
            // 这时只让相机的左右跟着转
            m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
        }
    }
    #endregion
    
    // 刷新一下相机的状态
    public virtual void Reset()
    {
        m_cameraDistance = maxDistance;  // 默认距离就是最大距离
        m_cameraTargetPitch = initialAngle;  // 默认仰俯角
        m_cameraTargetYaw = player.transform.rotation.eulerAngles.y;   // 默认偏航角
        m_cameraTargetPosition = player.unsizedPosition + Vector3.up * heightOffset; // 初始位置
        // m_cameraTargetPosition = player.transform.position + Vector3.up * heightOffset; // 初始位置
        MoveTarget();
        m_brain.ManualUpdate(); // 根据虚拟相机的Follow强制刷新相机，大脑会刷新所有的相机，是管理者
    }
    
    protected virtual void MoveTarget()
    {
        // 根据当前成员变量刷新Target空物体的位置和旋转
        m_target.position = m_cameraTargetPosition;
        m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
        m_cameraBody.CameraDistance = m_cameraDistance;  // 设置相机与跟随目标之间的距离（是第三人称算法组件）
    }
    
    // 让某些场景仍然采用地面上的跟随模式
    /*protected virtual bool VerticalFollowingStates()
    {
        return true;
    }*/
    
    // 限制角度在给定区间内
    protected virtual float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
}