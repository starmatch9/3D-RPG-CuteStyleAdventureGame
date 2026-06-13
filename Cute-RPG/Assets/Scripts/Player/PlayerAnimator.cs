using UnityEngine;

// 要求当前对象必须挂载 Player 组件
[RequireComponent(typeof(Player))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Parameters Names")]
    public string stateName = "State";
    public string lastStateName = "Last State";
    public string lateralSpeedName = "Lateral Speed";
    public string verticalSpeedName = "Vertical Speed";
    public string lateralAnimationSpeedName = "Lateral Animation Speed";
    public string jumpCounterName = "Jump Counter";
    public string isGroundedName = "Is Grounded";
    public string onStateChangedName = "On State Changed";

    [Header("Settings")]
    public float minLateralAnimationSpeed = 0.5f;

    public Animator animator;

    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_onStateChangedHash;

    protected Player m_player;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeParametersHash();
        InitializeAnimatorTriggers();
    }

    protected virtual void LateUpdate()
    {
        HandleAnimatorParameters();
    }

    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude;
        var verticalSpeed = m_player.verticalVelocity.y;
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed,
            lateralSpeed / m_player.stats.current.topSpeed);

        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
    }

    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
    }

    protected virtual void InitializeAnimatorTriggers()
    {
        m_player.states.events.onChange.AddListener(
            () => animator.SetTrigger(m_onStateChangedHash));
    }

    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }
}
