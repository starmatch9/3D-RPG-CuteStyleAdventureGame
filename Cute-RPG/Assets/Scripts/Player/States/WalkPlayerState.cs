using UnityEngine;

public class WalkPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        // Debug.Log("Entered WalkPlayerState");
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.Jump();
        player.Fall();
        
        // 根据摄像机去确定方向
        var inputDirection = player.inputs.GetMovementCameraDirection();
        // Debug.Log($"速度: {player.lateralVelocity.magnitude}, 最大速度: {player.stats.current.topSpeed}");
        if (inputDirection.sqrMagnitude > 0)
        {
            var dot = Vector3.Dot(inputDirection, player.lateralVelocity);
            if (dot >= player.stats.current.brakeThreshold)
            {
                player.Accelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                // 低于刹车阈值 → 进入刹车状态
                player.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            // 没有输入 → 使用摩擦力减速
            player.Friction();

            // 当水平速度为零 → 切换到闲置状态
            if (player.lateralVelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdlePlayerState>();
            }
        }
    }

    public override void OnContact(Player player, Collider other)
    {
    }
}