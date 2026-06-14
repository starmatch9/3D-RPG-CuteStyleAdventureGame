using Cinemachine;
using UnityEngine;

// 强制要求必须CinemachineVirtualCamera组件，否则会自动添加。
[RequireComponent(typeof(CinemachineVirtualCamera))]
[AddComponentMenu("Player/Player Camera")]
public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")] // 相机设置
    public Player player;
    public float maxDistance = 15f;    // 相机与目标的最大距离
    public float initialAngle = 20f;   // 初始俯仰角（相机上下角度）
    public float heightOffset = 1f;    // 相机相对玩家的垂直偏移量

    [Header("Following Settings")] // 跟随设置
    public float verticalUpDeadZone = 0.15f;      // 在地面时，相机向上跟随的死区
    public float verticalDownDeadZone = 0.15f;    // 在地面时，相机向下跟随的死区
    public float verticalAirUpDeadZone = 4f;      // 在空中时，相机向上跟随的死区
    public float verticalAirDownDeadZone = 0;     // 在空中时，相机向下跟随的死区
    public float maxVerticalSpeed = 10f;          // 相机在地面时的最大垂直跟随速度
    public float maxAirVerticalSpeed = 100f;      // 相机在空中时的最大垂直跟随速度

    [Header("Orbit Settings")] // 相机环绕设置
    public bool canOrbit = true;                  // 是否允许手动环绕相机
    public bool canOrbitWithVelocity = true;      // 是否允许通过速度带动相机旋转
    public float orbitVelocityMultiplier = 5;     // 速度驱动相机旋转的倍率

    [Range(0, 90)]
    public float verticalMaxRotation = 80;        // 相机俯仰角最大值

    [Range(-90, 0)]
    public float verticalMinRotation = -20;       // 相机俯仰角最小值

    // 内部变量
    protected float m_cameraDistance;             // 当前相机与玩家的距离
    protected float m_cameraTargetYaw;            // 相机目标的水平角度
    protected float m_cameraTargetPitch;          // 相机目标的俯仰角

    protected Vector3 m_cameraTargetPosition;     // 相机目标位置（用于插值过渡）
    
    protected CinemachineVirtualCamera m_camera;  // 相机对象
    protected Cinemachine3rdPersonFollow m_cameraBody; // 3D 跟随组件
    protected CinemachineBrain m_brain; // Cinemachine控制大脑

    protected Transform m_target;  // 相机跟随的目标点（在玩家上方一点）

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
        HandleOrbit();        // 输入环绕
        HandleVelocityOrbit(); // 速度驱动环绕
        HandleOffset();       // 高度跟随
        MoveTarget();         // 更新相机目标
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
        m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_brain = Camera.main.GetComponent<CinemachineBrain>();
    }
    
    protected virtual void InitializeFollower()
    {
        m_target = new GameObject(k_targetName).transform;
        m_target.position = player.transform.position;
    }
    
    protected virtual void InitializeCamera()
    {
        m_camera.Follow = m_target.transform; // 相机跟随目标点
        m_camera.LookAt = player.transform;   // 相机始终看向玩家

        Reset();
    }

    #endregion

    #region 帧处理方法

        protected virtual void HandleOrbit()
    {
        // 判断是否允许手动环绕相机
        if (canOrbit)
        {
            // 从玩家输入系统获取“视角方向输入”
            // 通常鼠标移动或右摇杆输入会返回一个二维向量：
            //   x -> 左右（控制Yaw，水平旋转）
            //   z -> 上下（控制Pitch，垂直旋转）
            var direction = player.inputs.GetLookDirection();

            // sqrMagnitude 表示向量的平方长度，用于判断是否有输入
            // 如果输入为零向量（没有移动鼠标/摇杆），就不需要修改相机
            if (direction.sqrMagnitude > 0)
            {
                // 判断玩家是否正在使用鼠标作为输入设备
                //   - 使用鼠标时：输入是“即时的”，不需要乘 Time.deltaTime
                //   - 使用手柄时：输入是“按帧累积的”，需要乘 Time.deltaTime 保持平滑
                var usingMouse = player.inputs.IsLookingWithMouse();

                // 根据输入设备选择不同的时间因子
                //   - 鼠标：乘 Time.timeScale（保持和游戏速度一致）
                //   - 手柄：乘 Time.deltaTime（保证旋转平滑、与帧率无关）
                float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;

                // 修改相机的水平旋转角度（Yaw）
                // direction.x -> 鼠标/摇杆的左右输入
                // yaw 正负 -> 相机往左右旋转
                m_cameraTargetYaw += direction.x * deltaTimeMultiplier;

                // 修改相机的垂直旋转角度（Pitch）
                // direction.z -> 鼠标/摇杆的上下输入
                // pitch 正负 -> 相机往上/下旋转
                //鼠标/手柄往上推：direction.z > 0，相机 Pitch 就减小（往上看）。
                //鼠标/手柄往下拉：direction.z < 0，相机 Pitch 就增大（往下看）。
                m_cameraTargetPitch -= direction.z * deltaTimeMultiplier;

                // 将相机的垂直旋转角度限制在一定范围内
                // 避免玩家把相机拉到头顶或者穿透地面
                m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
            }
        }
    }
    
    
    // 处理偏移
    protected virtual void HandleOffset()
    {
        var target = player.unsizedPosition + Vector3.up * heightOffset;
        var previousPosition = m_cameraTargetPosition;
        var targetHeight = previousPosition.y;
        // 地面跟随
        if (player.isGrounded  || VerticalFollowingStates() )
        {
            if (target.y > previousPosition.y + verticalUpDeadZone)
            {
                // 玩家上升时，相机缓慢跟随
                var offset = target.y - previousPosition.y - verticalUpDeadZone;
                targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
            }
            else if (target.y < previousPosition.y - verticalDownDeadZone)
            {
                // 玩家下降时，相机缓慢跟随
                var offset = target.y - previousPosition.y + verticalDownDeadZone;
                targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
            }
        }
        // 空中跟随
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
        m_cameraTargetPosition = new Vector3(target.x, targetHeight, target.z);
    }
    
    // 基于玩家的移动速度自动旋转相机
    // 作用：当玩家在地面上移动时，相机会根据玩家的速度方向（尤其是左右横向速度）
    // 来调整相机的偏航角度，从而营造“相机跟随运动方向”的效果。
    protected virtual void HandleVelocityOrbit()
    {
        // 判断是否允许根据速度来旋转相机，且玩家必须在地面上（避免空中漂浮时乱转相机）
        if (canOrbitWithVelocity && player.isGrounded)
        {
            // 将玩家的世界空间速度转换到相机目标的本地坐标系中
            // localVelocity.x 表示玩家相对相机前方的“横向速度”（左右移动速度）
            // localVelocity.z 表示前后速度（前进/后退），这里暂时未使用
            var localVelocity = m_target.InverseTransformVector(player.velocity);

            // 根据玩家的横向速度调整相机的偏航角度 (Yaw，即水平旋转)
            // localVelocity.x   -> 玩家左右速度
            // orbitVelocityMultiplier -> 灵敏度参数，控制相机旋转的快慢
            // Time.deltaTime    -> 保证旋转与帧率无关，平滑过渡
            m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
        }
    }
    #endregion
    
    public virtual void Reset()
    {
        m_cameraDistance = maxDistance;
        m_cameraTargetPitch = initialAngle; // 设定初始俯仰角
        m_cameraTargetYaw = player.transform.rotation.eulerAngles.y; // 根据玩家朝向设定相机水平角
        m_cameraTargetPosition = player.unsizedPosition + Vector3.up * heightOffset; // 初始位置
        // m_cameraTargetPosition = player.transform.position + Vector3.up * heightOffset; // 初始位置
        MoveTarget();
        m_brain.ManualUpdate(); // 强制刷新相机
    }
    
    protected virtual void MoveTarget()
    {
        m_target.position = m_cameraTargetPosition;
        m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
        m_cameraBody.CameraDistance = m_cameraDistance;
    }

    
    // 判断是否处于需要竖直跟随的状态
    protected virtual bool VerticalFollowingStates()
    {
        return true;
    }
    
    // 限制角度在给定区间内
    protected virtual float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
    
    
}