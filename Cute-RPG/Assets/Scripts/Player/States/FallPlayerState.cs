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
        player.FaceDirectionSmooth(player.lateralVelocity);
        player.AccelerateToInputDirection();
        player.Jump();

        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }
}