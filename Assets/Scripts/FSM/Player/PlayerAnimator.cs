using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(Player))]
[AddComponentMenu("PLAYERTWO/Platformer Project/Player/Player Animator")]
public class PlayerAnimator:MonoBehaviour
{
    /// <summary>
    /// 强制过度类
    /// </summary>
    protected Dictionary<int, ForcedTransition> m_forcedTransitions;

    [Header("Settings")]
    public float minLateralAnimationSpeed = 0.5f; // 横向动画播放的最小速度，防止太慢
    public List<ForcedTransition> forcedTransitions; // 强制过渡的列表

    [Header("Parameters Names")] // Animator 参数的变量名（可在 Inspector 修改）
    public string stateName = "State";                      // 当前状态
    public string lastStateName = "Last State";             // 上一个状态
    public string lateralSpeedName = "Lateral Speed";       // 横向速度
    public string verticalSpeedName = "Vertical Speed";     // 纵向速度
    public string lateralAnimationSpeedName = "Lateral Animation Speed"; // 横向动画播放速度
    public string healthName = "Health";                    // 血量
    public string jumpCounterName = "Jump Counter";         // 跳跃计数
    public string isGroundedName = "Is Grounded";           // 是否落地
    public string isHoldingName = "Is Holding";             // 是否正在抓取物品
    public string onStateChangedName = "On State Changed";  // 状态切换触发器

    protected Player m_player;

    [System.Serializable]
    public class ForcedTransition
    {
        [Tooltip("玩家状态机中 fromStated' 状态结束时,强制跳转到某个动画")]
        public int fromStateId;

        [Tooltip("目标动画所在的Animator层索引，默认０表示BaseLayer")]
        public int animatorLayer;

        [Tooltip("要强制播放的动画状态名")]
        public string toAnimateState;
    }

    public Animator animator;

    // Animator 参数的 Hash 值，避免字符串查找开销
    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeAnimatorTriggers();
        InitializeForcedTransitions();
        InitializeParametersHash();
    }

    protected virtual void LateUpdate() => HandleAnimatorParameters();

    /// <summary>
    /// 每帧更新 Animator 参数，使动画与玩家实际状态同步
    /// </summary>
    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude; // 横向速度
        var verticalSpeed = m_player.verticalVelocity.y;       // 纵向速度
        // 横向动画播放速度 = 横向速度 / 最大速度，保证最小速度不低于 minLateralAnimationSpeed
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);

        // 设置 Animator 参数
        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetInteger(m_jumpCounterHash,m_player.jumpCounter);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
        animator.SetBool(m_isHoldingHash, m_player.holding);
    }

    /// <summary>
    /// 初始化Player引用,并切换状态监听事件
    /// </summary>
    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
        m_player.states.events.onChange.AddListener(HandleForcedTransitions);
    }

    /// <summary>
    /// 初始化 Animator 的触发器，当状态切换时触发动画事件
    /// </summary>
    protected virtual void InitializeAnimatorTriggers()
    {
        // 给 Animator 发送 trigger（触发器参数），用于过渡动画
        m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));
    }

    /// <summary>
    /// 初始化强制过渡字典，避免重复 Key
    /// </summary>
    protected virtual void InitializeForcedTransitions()
    {
        m_forcedTransitions = new Dictionary<int, ForcedTransition>();

        foreach (var transition in forcedTransitions)
        {
            if (!m_forcedTransitions.ContainsKey(transition.fromStateId))
            {
                m_forcedTransitions.Add(transition.fromStateId, transition);
            }
        }
    }

    /// <summary>
    /// 把参数名转换为 Hash，提高性能
    /// </summary>
    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }
    /// <summary>
    /// 执行强制过渡逻辑：
    /// 如果上一个状态匹配强制过渡表，则播放对应的动画
    /// </summary>
    protected virtual void HandleForcedTransitions()
    {
        var lastStateIndex = m_player.states.lastIndex;

        if(m_forcedTransitions.ContainsKey(lastStateIndex))
        {
           var layer = m_forcedTransitions[lastStateIndex].animatorLayer;
            animator.Play(m_forcedTransitions[lastStateIndex].toAnimateState, layer);

        }
    }
}

