using UnityEngine;

// 要求当前对象必须挂载 Player 组件
[RequireComponent(typeof(Player))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Parameters Names")]
    public string stateName = "State";
    public string isGroundedName = "Is Grounded";

    public Animator animator;

    protected int m_stateHash;
    protected int m_isGroundedHash;

    protected Player m_player;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeParametersHash();
    }

    protected virtual void LateUpdate()
    {
        HandleAnimatorParameters();
    }

    protected virtual void HandleAnimatorParameters()
    {
        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
    }

    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
    }
    

    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
    }
}
