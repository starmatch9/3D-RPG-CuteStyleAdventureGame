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
    
    protected Camera m_camera;
    
    // 常量：鼠标设备名称
    protected const string k_mouseDeviceName = "Mouse";

    protected virtual void Awake()
    {
        CacheAction();
    }

    protected virtual void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void OnEnable() => actions?.Enable();
    protected virtual void OnDisable() => actions?.Disable();
    
    protected virtual void CacheAction()
    {
        m_movement = actions["Movement"];
        m_look = actions["Look"];
    }
    
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

    public virtual Vector3 GetMovementDirection()
    {
        if (Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadzone(float value, float deadzone) =>
        (value - (value > 0 ? -deadzone : deadzone)) / (1 - deadzone);
}