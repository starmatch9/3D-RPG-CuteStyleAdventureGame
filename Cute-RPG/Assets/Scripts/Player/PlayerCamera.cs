using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Camera")]
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
    
    protected virtual void Start()
    {
        InitializeComponents();
        InitializeFollower();
        InitializeCamera();
    }
    
    protected virtual void LateUpdate()
    {
        //HandleOrbit();        // 输入环绕
        //HandleVelocityOrbit(); // 速度驱动环绕
        //HandleOffset();       // 高度跟随
        MoveTarget();         // 更新相机目标
    }
    
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

}