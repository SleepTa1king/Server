using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class Player:Entity<Player>
{
    public PlayerEvents playerEvents;
    public PlayerInputManager inputs { get;protected set; }
    public PlayerStatsManager stats { get;protected set; }
    public int jumpCounter { get; protected set; }
    public int airDashCounter { get; protected set; }
    public int airSpinCounter { get; protected set; }
    public float lastDashTime { get; protected set; }

    public bool onWater { get; protected set; } 
    public Health health { get; protected set; }
    public bool holding { get; protected set; }
    public Collider water { get; protected set; }
    public Pickable pickable { get; protected set; }
    public Transform pickableSlot;
    public Vector3 lastWallNormal { get; protected set; }
    public Transform skin;
    public Pole pole { get; protected set; }

    public Vector3 m_skinInitialPosition = Vector3 .zero;
    public Quaternion m_skinInitialRotation = Quaternion.identity;
    public virtual bool isAlive => !health.isEmpty;

    protected const float k_waterExitOffset = 0.25f;
    protected Vector3 m_respawnPosition = Vector3.zero;
    protected Quaternion m_respawnRotation = Quaternion.identity;
    
    public float logicTime { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        InitializeInput();
        InitializeStat();
        InitializeHealth();
        InitializeTag();
        InititalRespawn();

        entityEvents.OnGroundEnter.AddListener(() => 
        { 
            ResetJumps(); 
            ResetAirDash();
            ResetAirSpin();
           
        });
        entityEvents.OnRailsEnter.AddListener(() =>
        {
            ResetJumps();
            ResetAirSpin();
            ResetAirDash();
            StartGrind();
        });
       
    }

    public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

    protected virtual void InitializeInput() => inputs = GetComponent<PlayerInputManager>();
    protected virtual void InitializeStat() => stats = GetComponent<PlayerStatsManager>();
    protected virtual void InitializeHealth() => health = GetComponent<Health>();
    protected virtual void InitializeTag() => tag = GameTags.Player;


    public virtual void Accelerate(Vector3 direction, float deltaTime)
    {
        var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;
        var topSpeed = isGrounded && inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;
     
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed, deltaTime);
    }
    public virtual void Accelerate(Vector3 direction) => Accelerate(direction, Time.deltaTime);

    public virtual void AccelerateToInputDirection(float deltaTime)
    {
        var inputDirection = inputs.GetMovementCameraDirection();
        Accelerate(inputDirection, deltaTime);
    }
    public virtual void AccelerateToInputDirection() => AccelerateToInputDirection(Time.deltaTime);

    public virtual void CrawlingAccelerate(Vector3 direction, float deltaTime) =>
        Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAccleration, stats.current.topSpeed, deltaTime);
    public virtual void CrawlingAccelerate(Vector3 direction) => CrawlingAccelerate(direction, Time.deltaTime);

    public virtual void BackflipAcceleration(float deltaTime)
    {
        var direction = inputs.GetMovementCameraDirection();
        Accelerate(direction, stats.current.backflipTurningDrag, stats.current.backflipAirAcceleration, stats.current.backflipTopSpeed, deltaTime);
    }
    public virtual void BackflipAcceleration() => BackflipAcceleration(Time.deltaTime);

    public virtual void WaterAcceleration(Vector3 direction, float deltaTime) =>
        Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration, stats.current.swimTopSpeed, deltaTime);
    public virtual void WaterAcceleration(Vector3 direction) => WaterAcceleration(direction, Time.deltaTime);

    public virtual void WaterFaceDirection(Vector3 direction, float deltaTime) =>
        FaceDirection(direction, stats.current.waterRotationSpeed, deltaTime);
    public virtual void WaterFaceDirection(Vector3 direction) => WaterFaceDirection(direction, Time.deltaTime);
       
    public virtual void Decelerate(float deltaTime) => Decelerate(stats.current.deceleration, deltaTime);
    public virtual void Decelerate() => Decelerate(Time.deltaTime);

    public virtual void Friction(float deltaTime)
    {
        if (OnSlopeGround())
            Decelerate(stats.current.slopeFriction, deltaTime);
        else
            Decelerate(stats.current.friction, deltaTime);
    }
    public virtual void Friction() => Friction(Time.deltaTime);

    public virtual void FaceDirectionSmooth(Vector3 direction, float deltaTime) => FaceDirection(direction, stats.current.rotationSpeed, deltaTime);
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirectionSmooth(direction, Time.deltaTime);

    public virtual void Gravity(float deltaTime)
    {
        if(!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGrivity;
            speed -= force * gravityMultiplier * deltaTime;

            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }
    public virtual void Gravity() => Gravity(Time.deltaTime);

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
    public virtual void ResetJumps() => jumpCounter = 0;
    public virtual void SetJumps(int amount) => jumpCounter = amount;

    public virtual void Jump()
    {
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        var canCoyoteJump = (jumpCounter == 0) && (logicTime < lastGroundTime + stats.current.coyoteJumpThreshold);

        if ((isGrounded || canMultiJump || canCoyoteJump))
        {
            if (inputs.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    public virtual void Jump(float height)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>();
        playerEvents.OnJump?.Invoke();
    }

    public virtual void Fall()
    {
        if(!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }

    public override void ApplyDamage(int amount,Vector3 origin)
    {
        if (!health.isEmpty && !health.recovering)
        {
            health.Damage(amount);
            var damageDir = origin - transform.position;
            damageDir.y = 0;
            damageDir = damageDir.normalized;
            FaceDirection(damageDir);

            lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

            if (!onWater)
            {
                verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                states.Change<HurtPlayerState>();
            }

            playerEvents.OnHurt?.Invoke();
        }
    }

    public virtual bool canStandUp => !SphereCast(Vector3.up, originHeight);   

    public virtual void Backflip(float force)
    {
        if(stats.current.canBackflip && !holding)
        {
            verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
            lateralVelocity = -transform.forward * force;
            states.Change<BackflipPlayerState>();
            playerEvents.OnBackflip?.Invoke();
        }
    }

    public virtual void ResetAirDash() => airDashCounter = 0;
    public virtual void ResetAirSpin() => airSpinCounter = 0;
    public virtual void Dash()
    {
        var canAirDash = stats.current.canAirDash && !isGrounded && airDashCounter < stats.current.allowedAirDashes;
        var canGroundDash = stats.current.canGroundDash && isGrounded && logicTime - lastDashTime > stats.current.groundDashCoolDown;
        
        if(inputs.GetDashDown() && (canAirDash||canGroundDash))
        {
            if (!isGrounded)
                airDashCounter++;
            lastDashTime = logicTime;
            states.Change<DashPlayerState>();
        }
    }

    public virtual void StompAttack()
    {
        if(!isGrounded && stats.current.canStompAttack &&inputs.GetStompDown())
        {
            states.Change<StompPlayerState>();
        }
    }

    public virtual void Spin()
    {
        var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;

        if(stats.current.canSpin && canAirSpin &&!holding && inputs.GetSpinDown())
        {
            if(!isGrounded)
            {
                airSpinCounter++;
            }

            states.Change<SpinPlayerState>();
            playerEvents.OnSpin?.Invoke();
        }
    }

    public virtual void AirDive()
    {
        if(stats.current.canAirDive && !isGrounded &&!holding &&inputs.GetAirDiveDown())
        {
            states.Change<AirDivePlayerState>();
            playerEvents.OnAirDive?.Invoke();
        }
    }

    public virtual void OnTriggerStay(Collider other)
    {
        if(other.CompareTag(GameTags.VolumeWater))
        {
            if (!onWater && other.bounds.Contains(unsizedPosition))
            {
                EnterWater(other);
            }
            else if(onWater)
            {
                var exitPoint = position + Vector3.down * k_waterExitOffset;
                if(!other.bounds.Contains(exitPoint))
                {
                    ExitWater();
                }
            }
        }
    }

    public virtual void EnterWater(Collider water)
    {
        if(!onWater && !health.isEmpty)
        {
            onWater = true;
            this.water = water;
            states.Change<SwimPlayerState>();
        }
    }

    public virtual void ExitWater()
    {
        if(onWater)
        {
            onWater = false;
        }
    }

    public virtual void Glide()
    {
        if (!isGrounded && inputs.GetGlide() &&
            verticalVelocity.y <= 0 && stats.current.canGlide)
            states.Change<GlidingPlayerState>();
    }

    public virtual void WallDrag(Collider other)
    {
        if(stats.current.canWallDrag && velocity.y<=0 && !holding && !other.TryGetComponent<Rigidbody>(out _))
        {
            if(CapsuleCast(transform.forward,0.25f,out var hit,stats.current.wallDragLayers))
            {
                if(hit.collider.CompareTag(GameTags.Platform))
                    transform.parent = hit.transform;
                lastWallNormal = hit.normal;
                states.Change<WallDragPlayerState>();
            }
        }
    }

    public virtual void DirectionalJump(Vector3 direction,float height,float distance)
    {
        jumpCounter++;
        verticalVelocity += height * Vector3.up;
        lateralVelocity += distance * direction;
        playerEvents.OnJump?.Invoke();
    }

    public virtual void GrabPole(Collider other)
    {
        if (stats.current.canPoleClimb && velocity.y <= 0&&!holding &&other.TryGetComponent(out Pole pole))
        {
            this.pole = pole;
            states.Change<PoleClimbingPlayerState>();
        }
    }

    public virtual void LedgeGrab()
    {
        if (stats.current.canLedgeHang && velocity.y < 0 && !holding &&
            states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
            DetectingLedge(stats.current.ledgeMaxForwardDistance, stats.current.ledgeMaxDownwardDistance, out var hit))
        {
            if(!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
            {
                var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                var lateralOffset = transform.forward * ledgeDistance;
                var verticalOffset = Vector3.down * height * 0.5f - center;

                velocity = Vector3.zero;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                transform.position = hit.point - lateralOffset + verticalOffset;

                states.Change<LedgeHangingPlayerState>();
                playerEvents.OnLedgeGrabbed?.Invoke();
            }
        }
    }

    protected virtual bool DetectingLedge(float forwardDistance, float downwardDistance, out RaycastHit ledgeHit)
    {
        var contactOffset = Physics.defaultContactOffset + positionDelta;
        var ledgeMaxDistance = radius + forwardDistance;
        var ledgeHeightOffset = height * 0.5f + contactOffset;
        var upwardOffset = transform.up * ledgeHeightOffset;
        var forwardOffset = transform.forward * ledgeMaxDistance;

        if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
            Physics.Raycast(position + forwardOffset * .01f, transform.up,ledgeHeightOffset,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            ledgeHit = new RaycastHit();
            return false;
        }

        var origin = position + upwardOffset + forwardOffset;
        var distance = downwardDistance + contactOffset;

        return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance,
            stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
    }
    public virtual void SetSkinParent(Transform parent)
    {
        if(skin)
        {
            skin.parent = parent;
        }
    }
    public virtual void ResetSkinParent()
    {
        if(skin)
        {
            skin.parent = transform;
            skin.localPosition = m_skinInitialPosition;
            skin.localRotation = m_skinInitialRotation;
        }
    }

    public virtual void PushRigidbody(Collider other)
    {
        if(!IsPointUnderStep(other.bounds.max)&& other.TryGetComponent(out Rigidbody rigidbody))
        {
            var force = lateralVelocity * stats.current.pushForce;
            rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
        }
    }

    public virtual void PickAndThrow()
    {
        if(stats.current.canPickUp && inputs.GetPickAndDropDown())
        {
            if(CapsuleCast(transform.forward,stats.current.pickDistance,out var hit))
            {
                if(hit.transform.TryGetComponent(out Pickable pickable))
                {
                    PickUp(pickable);
                }
            }
            else
            {
                Throw ();
            }
        }
    }

    public virtual void PickUp(Pickable pickable)
    {
        if(!holding && (isGrounded || stats.current.canPickUpOnAir))
        {
            holding = true;
            this.pickable = pickable;
            pickable.PickUp(pickableSlot);
            pickable.onRespawn.AddListener(RemovePickable);
            playerEvents.OnPickUp?.Invoke();
        }
    }

    public virtual void RemovePickable()
    {
        if(holding)
        {
            holding = false;
            this.pickable = null;
        }
    }
    public virtual void Throw()
    {
        if(holding)
        {
            var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
            this.pickable.Release(transform.forward, force);
            this.pickable = null;
            holding = false;
            playerEvents.OnThrow?.Invoke();
        }
    }
    protected virtual void InititalRespawn()
    {
        m_respawnPosition = transform.position;
        m_respawnRotation = transform.rotation;
    }
    public virtual void SetRespawn(Vector3 positon,Quaternion rotation)
    {
        m_respawnPosition = position;
        m_respawnRotation = rotation;
    }
    public virtual void Respawn()
    {
        health.Reset();
        transform.SetLocalPositionAndRotation(m_respawnPosition, m_respawnRotation);
        states.Change<IdlePlayerState>();
    }

    public CSInput currentInput { get; protected set; } = new CSInput();

    public virtual void SetInput(CSInput input)
    {
        currentInput = input;
    }

    public override void LogicUpdate(float deltaTime)
    {
        logicTime += deltaTime;
        base.LogicUpdate(deltaTime);
    }
}
