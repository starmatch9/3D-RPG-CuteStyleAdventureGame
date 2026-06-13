public class FallPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {

    }

    protected override void OnExit(Player player)
    {
        
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();
        player.FaceDirectionSmooth(player.lateralVelocity);  //在空中也可以转向移动
        player.AccelerateToInputDirection();  // 其实就是走动逻辑
        player.Jump();

        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }
}