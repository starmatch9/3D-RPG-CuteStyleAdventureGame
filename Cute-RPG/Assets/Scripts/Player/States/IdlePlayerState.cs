using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class IdlePlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
    }

    protected override void OnExit(Player player)
    {
        //Debug.Log("Exited IdlePlayerState");
    }

    protected override void OnStep(Player player)
    {
        //Debug.Log("IdleStep");
        player.Gravity();  
        player.SnapToGround();  // 贴地
        player.Jump();
        player.Fall();

        
        var inputDirection = player.inputs.GetMovementDirection();
        //Debug.Log("输入方向为：" + inputDirection);
        // 输入操作--配置数据（速度、方向）--改变状态
        if (inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
        {
            player.states.Change<WalkPlayerState>();
        }
        
    }

    public override void OnContact(Player player, Collider other)
    {
    }
}