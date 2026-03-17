using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("PLAYERTWO/Platformer Project/Entity/Entity HitBox")]
public class EntityHitBox:MonoBehaviour
{

    [Header("Attack Settings")] // 在 Inspector 面板中分组显示：攻击相关设置
    public bool breakObjects;   // 是否可以击碎可破坏物体
    public int damage = 1;      // 攻击造成的伤害值

    [Header("Rebound Settings")] // 分组显示：反弹相关设置
    public bool rebound;        // 是否启用反弹效果
    public float reboundMinForce = 10f; // 反弹最小力度
    public float reboundMaxForce = 25f; // 反弹最大力度

    [Header("Push Back Settings")] // 分组显示：击退相关设置
    public bool pushBack;        // 是否启用击退效果
    public float pushBackMinMagnitude = 5f;  // 击退最小速度
    public float pushBackMaxMagnitude = 10f; // 击退最大速度

    protected EntityBase m_entity;
    protected Collider m_collider;
    protected virtual void Start()
    {
        InitializeEntity();
        InitializeColider();
    }

    protected virtual void InitializeEntity()
    {
        if(!m_entity)
        {
            m_entity = GetComponentInParent<EntityBase>();
        }
    }

    protected virtual void InitializeColider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
        HandleCustomCollision(other);
    }

    protected virtual void HandleCustomCollision(Collider other) { }

    protected virtual void HandleCollision(Collider other)
    {
        if(other != m_entity.controller)
        {
            if(other.TryGetComponent(out EntityBase target))
            {
                HandleEntityAttack(target);
                HandleRebound();
                HandlePushBack();
            }
            else if(other.TryGetComponent(out Breakable breakable))
            {
                HandleBreakableObject(breakable);
            }
        }
    }

    protected virtual void HandleEntityAttack(EntityBase other)
    {
        other.ApplyDamage(damage, transform.position);
    }

    protected virtual void HandleRebound()
    {
        if(rebound)
        {
            var force = -m_entity.velocity.y;
            force = Mathf.Clamp(force, reboundMinForce, reboundMaxForce);
            m_entity.verticalVelocity = Vector3.up * force;
        }
    }

    protected virtual void HandlePushBack()
    {
        if(pushBack)
        {
            var force = m_entity.lateralVelocity.magnitude;
            force = Mathf.Clamp(force, pushBackMinMagnitude, pushBackMaxMagnitude);
            m_entity.lateralVelocity = -transform.forward * force;
        }
    }

    protected virtual void HandleBreakableObject(Breakable breakable)
    {
        if(breakObjects)
        {
            breakable.Break();
        }
    }

}

