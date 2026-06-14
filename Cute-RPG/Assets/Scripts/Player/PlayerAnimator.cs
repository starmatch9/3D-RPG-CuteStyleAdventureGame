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

    // 保证状态机更新后再跟状态
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
        // Unity内部，会为每一个动画机的参数生成一个哈希ID
        // 代码中用这个哈希ID访问比字符串访问更快（就用下面的方法获得）
        m_stateHash = Animator.StringToHash(stateName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
    }
}
