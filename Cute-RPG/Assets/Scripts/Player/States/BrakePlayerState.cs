using UnityEngine;


// 将该类添加到 Unity 的组件菜单，方便在 Inspector 里挂载
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Brake Player State")]
public class BrakePlayerState : PlayerState
{
    // 进入刹车状态时调用
    protected override void OnEnter(Player player)
    {
    }
    
    // 离开刹车状态时调用
    protected override void OnExit(Player player)
    {
    }
    
    // 每帧更新时调用
    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.Jump();
        player.Fall();

        
        // 执行减速逻辑（逐渐降低水平速度，直到停下）
        player.Decelerate();
        
        // 如果玩家的水平速度为 0（完全停下来了）
        if (player.lateralVelocity.sqrMagnitude == 0)
        {
            // 状态切换为 Idle（待机状态）
            player.states.Change<IdlePlayerState>();
        }
    }
    
    // 当处于刹车状态下发生碰撞时调用
    public override void OnContact(Player player, Collider other) { }
}