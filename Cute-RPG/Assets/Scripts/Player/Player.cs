using UnityEngine;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvents;
    
    public PlayerInputManager inputs { get; protected set; }

    public PlayerStatsManager stats { get; protected set; }
    
    public int jumpCounter { get; protected set; }
    
    public virtual void ResetJumps() => jumpCounter = 0;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStats();
        
        entityEvents.OnGroundEnter.AddListener(() =>
        {
            ResetJumps();
        });
    }

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
    
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
        var turningDrag = stats.current.turningDrag;
        var acceleration = stats.current.acceleration;
        var finalAcceleration = acceleration;
        var topSpeed = stats.current.topSpeed;
        // 调用底层 Accelerate(方向, 转向阻尼, 加速度, 最大速度) 
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
    }
    
    // 平滑的朝向某个方向旋转
    // 之前看起来瞬移的原因是人物在父物体下的位置不是000
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);
    
    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = inputs.GetMovementCameraDirection(); // 输入相对于相机的方向
        Accelerate(inputDirection);
    }
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);

    
    // 平滑减速
    public virtual void Friction()
    {
        Decelerate(stats.current.friction);      // 普通摩擦
    }
    
    public virtual void Gravity()
    {
        //Debug.Log($"重力生效前 verticalVelocity: {verticalVelocity.y}");
        
        //isGrounded = false;
        if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            // 上升时用普通重力，下落时用更强的下落重力
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;

            // 限制最大下落速度
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    public virtual void Jump()
    {
        // 二段跳限制
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);

        //isGrounded = true;
        if (isGrounded || canMultiJump || canCoyoteJump)
        {
            if (inputs.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }            
        }

        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        } 
    }
    

    public virtual void Jump(float height)
    {
        // Debug.Log("跳");
        GlobalData.effectManager.PlayEffect(GlobalData.effectManager.jump);
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>(); // 切换为下落状态
        playerEvents.OnJump?.Invoke();  // 类似常见的发送信息
    }
    
    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }
}