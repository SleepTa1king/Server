using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Enemy : Entity<Enemy>
{
    public EnemyEvents enemyEvents;
    public Collider[] m_sightOverlaps = new Collider[1024];
    public Collider[] m_contactAttackOverlaps = new Collider[1024];
    public WaypointManager waypoints { get; protected set; }
    public EnemyStatsManager stats { get; protected set; }
    public Player player { get; protected set; }
    public Health health { get; protected set; }

    public float logicTime { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeWaypointManager();
        InitializeHealth();
        InitializeStatsManager();
        InitializeTag();
    }

    public override void LogicUpdate(float deltaTime)
    {
        logicTime += deltaTime;
        base.LogicUpdate(deltaTime);
    }

    protected override void OnUpdate(float deltaTime)
    {
        HandleSight();
        ContactAttack();
    }

    public virtual void InitializeTag() => tag = GameTags.Enemy;
    public virtual void InitializeHealth() => health = GetComponent<Health>();
    public virtual void InitializeWaypointManager() => waypoints = GetComponent<WaypointManager>();
    public virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();
    
    public virtual void FaceDirectionSmooth(Vector3 direction, float deltaTime) => FaceDirection(direction, stats.current.rotationSpeed, deltaTime);
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirectionSmooth(direction, Time.deltaTime);

    public virtual void Accelerate(Vector3 direction, float accletation, float topSpeed, float deltaTime) =>
        Accelerate(direction, stats.current.turningDrag, accletation, topSpeed, deltaTime);
    public virtual void Accelerate(Vector3 direction, float accletation, float topSpeed) =>
        Accelerate(direction, accletation, topSpeed, Time.deltaTime);

    public virtual void Decelerate(float deltaTime) => Decelerate(stats.current.deceleration, deltaTime);
    public virtual void Decelerate() => Decelerate(Time.deltaTime);

    public virtual void Friction(float deltaTime) => Decelerate(stats.current.friction, deltaTime);
    public virtual void Friction() => Friction(Time.deltaTime);

    protected virtual void HandleSight()
    {
        if (!player)
        {
            var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);
            for (int i = 0; i < overlaps; i++)
            {
                if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
                {
                    this.player = player;
                    enemyEvents.OnPlayerSpotted?.Invoke();
                    return;
                }
            }
        }
        else
        {
            var distance = Vector3.Distance(position, player.position);
            if (player.health.current == 0 || distance > stats.current.viewRange)
            {
                player = null;
                enemyEvents.OnPlayerScaped?.Invoke();
            }
        }
    }

    protected virtual void ContactAttack()
    {
        if(stats.current.canAttackOnContact)
        {
            var overlaps = OverLapEntity(m_contactAttackOverlaps, stats.current.contactOffset);
            for(int i = 0; i < overlaps; i++)
            {
                if (m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
                {
                    var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;
                    if(!player.IsPointUnderStep(stepping))
                    {
                        if(stats.current.contactPushback)
                        {
                            lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
                        }
                        player.ApplyDamage(stats.current.contactDamage, transform.position);
                        enemyEvents.OnPlayerContact?.Invoke();
                    }
                }
            }
        }
    }

    public override void ApplyDamage(int amount, Vector3 origin)
    {
        if(!health.isEmpty && !health.recovering)
        {
            health.Damage(amount);
            enemyEvents.OnDamage?.Invoke();

            if(health.isEmpty)
            {
                controller.enabled = false;
                enemyEvents.OnDie?.Invoke();
            }
        }
    }

    public virtual void Gravity(float deltaTime) => Gravity(stats.current.gravity, deltaTime);
    public virtual void Gravity() => Gravity(Time.deltaTime);

    public virtual void SnapToGround(float deltaTime) => SnapToGround(stats.current.snapForce);
    public virtual void SnapToGround() => SnapToGround(Time.deltaTime);
}
