public class Player : Entity<Player>
{
    public PlayerInputManager input { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
    }

    protected virtual void InitializeInputs()
    {
        // 从组件中获取
        input = GetComponent<PlayerInputManager>();
    }
}