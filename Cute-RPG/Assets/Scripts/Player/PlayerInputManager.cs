using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 要在包管理器里面提前下载好InputSystem这个插件
public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    protected float m_movementDirectionUnlockTime;

    protected InputAction m_movement;
    
    protected Camera m_camera;

    protected virtual void Awake()
    {
        CacheAction();
    }

    protected virtual void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void CacheAction()
    {
        m_movement = actions["Movement"];
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