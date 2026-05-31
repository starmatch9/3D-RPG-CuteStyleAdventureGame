using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : Entity<Player>
{
    public PlayerInputManager inputs { get; protected set; }

    public PlayerStatsManager stats { get; protected set; }
    
    

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStats();
    }

    protected virtual void InitializeInputs()
    {
        // 从组件中获取
        inputs = GetComponent<PlayerInputManager>();
    }

    protected virtual void InitializeStats()
    {
        stats = GetComponent<PlayerStatsManager>();
    }
    
    public virtual void Accelerate(Vector3 direction)
    {
        // 根据是否按下 Run 键、是否在地面，决定不同的转向阻尼与加速度
        // var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        // var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        // var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration; // 空中与地面不同
        // var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;

        var turningDrag = stats.current.turningDrag;
        var acceleration = stats.current.acceleration;
        var finalAcceleration = stats.current.airAcceleration;
        var topSpeed = stats.current.topSpeed;
        // 调用底层 Accelerate(方向, 转向阻尼, 加速度, 最大速度) 
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

        // // 如果刚松开跑步键，限制最大速度，避免瞬间超速
        // if (inputs.GetRunUp())
        // {
        //     lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, topSpeed);
        // }
    }
}