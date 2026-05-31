using UnityEngine;

public class WalkPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        Debug.Log("Entered WalkPlayerState");
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        // 根据摄像机去确定方向
        var inputDirection = player.inputs.GetMovementCameraDirection();
        Debug.Log($"速度: {player.lateralVelocity.magnitude}, 最大速度: {player.stats.current.topSpeed}");
        if (inputDirection.sqrMagnitude > 0)
        {
            var dot = Vector3.Dot(inputDirection, player.lateralVelocity);
            if (dot >= player.stats.current.brakeThreshold)
            {
                player.Accelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
        }
    }

    public override void OnContact(Player player, Collider other)
    {
    }
}