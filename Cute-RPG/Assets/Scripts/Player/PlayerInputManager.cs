using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 要在包管理器里面提前下载好InputSystem这个插件
public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    protected float m_movementDirectionUnlockTime;

    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;
    
    protected Camera m_camera;
    
    // 常量：鼠标设备名称
    protected const string k_mouseDeviceName = "Mouse";

    protected float? m_lastJumpTime;

    protected const float k_jumpBuffer = 0.15f;

    protected virtual void Awake()
    {
        CacheAction();
    }

    protected virtual void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void Update()
    {
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }

    protected virtual void OnEnable() => actions?.Enable();
    protected virtual void OnDisable() => actions?.Disable();
    
    protected virtual void CacheAction()
    {
        m_movement = actions["Movement"];
        m_look = actions["Look"];
        m_jump = actions["Jump"];
    }
    
    // 获取视角方向，如果是鼠标直接获取轴，如果是摇杆把死区考虑在内
    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();

        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }

        return GetAxisWithCrossDeadZone(value);
    }
    
    public virtual bool IsLookingWithMouse()
    {
        if (m_look.activeControl == null)
        {
            return false;
        }
        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }
    
    // 在世界坐标的基础上基于摄像机旋转，得到此视角下Moved朝向是什么
    public virtual Vector3 GetMovementCameraDirection()
    {
        var direction = GetMovementDirection();

        if (direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
        }

        return direction;
    }

    // 获取移动方向
    public virtual Vector3 GetMovementDirection()
    {
        if (Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    // 获取把死区剔除死区的力度百分比
    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    // 摇杆防抖，所以中间有一截死区，这个方法就是返回摇杆力度了（重映射说是）
    protected float RemapToDeadzone(float value, float deadzone) =>
        (value - (value > 0 ? -deadzone : deadzone)) / (1 - deadzone);


    // 指跳远按键
    public virtual bool GetJumpDown()
    {
        if (m_lastJumpTime != null && Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }

        return false;
    }

    // 
    public virtual bool GetJumpUp()
    {
        return m_jump.WasReleasedThisFrame();
    }
}