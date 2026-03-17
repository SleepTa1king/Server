using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Pickable : MonoBehaviour,IEntityContact
{
    [Header("General Settings")]
    public Vector3 offset;                 // 物体被拾取后在玩家手中的位置偏移
    public float releaseOffset = 0.5f;     // 物体被释放时向前偏移的距离

    [Header("Respawn Settings")]
    public bool autoRespawn;               // 是否开启自动重生
    public bool respawnOnHitHazards;       // 碰到危险物（Hazard）是否重生
    public float respawnHeightLimit = -100;// 超过某个高度（掉落过深）是否重生

    [Header("Attack Settings")]
    public bool attackEnemies = true;      // 是否可以攻击敌人
    public int damage = 1;                 // 对敌人造成的伤害值
    public float minDamageSpeed = 5f;      // 物体速度超过这个阈值时才会造成伤害

    [Space(15)]

    /// <summary>
    /// 当物体被拾取时触发
    /// </summary>
    public UnityEvent onPicked;

    /// <summary>
    /// 当物体被释放时触发
    /// </summary>
    public UnityEvent onReleased;

    /// <summary>
    /// 当物体被重生时触发
    /// </summary>
    public UnityEvent onRespawn;

    protected Collider m_collider;              // 缓存物体的碰撞体
    protected Rigidbody m_rigidBody;            // 缓存物体的刚体

    protected Vector3 m_initialPosition;        // 初始位置（用于重生）
    protected Quaternion m_initialRotation;     // 初始旋转（用于重生）
    protected Transform m_initialParent;        // 初始父物体（用于重生时还原层级）

    protected RigidbodyInterpolation m_interpolation; // 保存插值模式（被拾取时关闭）

    public bool beingHold { get; protected set; } // 是否当前正被玩家持有

    /// <summary>
    /// 拾取物体
    /// </summary>
    /// <param name="slot">玩家的拾取槽（Transform）</param>
    public virtual void PickUp(Transform slot)
    {
        if(!beingHold)
        {
            beingHold = true;
            transform.parent = slot;
            transform.localPosition = Vector3.zero + offset;
            transform.localRotation = Quaternion.identity;
            m_rigidBody.isKinematic = true;
            m_collider.isTrigger = true;
            m_interpolation = m_rigidBody.interpolation;
            m_rigidBody.interpolation = RigidbodyInterpolation.None;
            onPicked?.Invoke();
        }
    }

    public virtual void Release(Vector3 direction,float force)
    {
        if(beingHold)
        {
            transform.parent = null;
            transform.position += direction * releaseOffset;
            m_collider.isTrigger = false;
            m_rigidBody.isKinematic = false;
            beingHold = false;
            m_rigidBody.velocity = direction * force;
            onReleased?.Invoke();
            
        }
    }

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_initialPosition = transform.localPosition;
        m_initialRotation = transform.localRotation;
        m_initialParent = transform.parent;
    }
    protected void Update()
    {
        if(autoRespawn && transform.position.y <= respawnHeightLimit)
        {
            Respawn();
        }
    }
    public virtual void EvaluateHazardRespawn(Collider other)
    {
        if(autoRespawn && respawnOnHitHazards &&other.CompareTag(GameTags.Hazard))
        {
            Respawn();
        }
    }
    public virtual void Respawn()
    {
        m_rigidBody.velocity = Vector3.zero;
        transform.parent = m_initialParent;
        transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
        m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;
        onRespawn?.Invoke();
    }

    public virtual void OnEntityContact(EntityBase entity)
    {
        if (attackEnemies && entity is Enemy && m_rigidBody.velocity.magnitude > minDamageSpeed)
        {
            entity.ApplyDamage(damage, transform.position);
        }
    }

    protected virtual void OnTriggerEnter(Collider other) => EvaluateHazardRespawn(other);
    protected virtual void OnCollisionEnter(Collision collision) => EvaluateHazardRespawn(collision.collider);
}

