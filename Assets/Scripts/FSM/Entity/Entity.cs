using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public abstract class EntityBase : MonoBehaviour
{
    public float accelerationMultiplier { get; set; } = 1f;
    public float gravityMultiplier { get; set; } = 1f;
    public float topSpeedMultiplier { get; set; } = 1f;
    public float turningDragMultiplier { get; set; } = 1f;
    public float decelerationMultiplier { get; set; } = 1f;

    protected Collider[] m_contactBuffer = new Collider[10];
    public EntityEvents entityEvents;
    protected readonly float m_groundOffset = 0.1f;
    public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originHeight * 0.5f;
    public bool isGrounded { get; protected set; } = true;
    public bool onRails { get; set; }
    public SplineContainer rails { get; protected set; }
    public float lastGroundTime { get; protected set; }
    public Vector3 lastPosition { get; protected set; }
    public Vector3 velocity { get; set; }
    protected CapsuleCollider m_collider;
    protected Rigidbody m_rigidbody;
    public float positionDelta { get; protected set; }
    public Vector3 lateralVelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z); }
    }
    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }
    public CharacterController controller { get; protected set; }
    public float originHeight { get; protected set; }
    public float height => controller.height;
    public float radius => controller.radius;
    public Vector3 center => controller.center;
    public Vector3 position => transform.position + center;

    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);
    public RaycastHit groundHit;
    public float groundAngle { get; protected set; }
    public Vector3 groundNormal { get; protected set; }
    public Vector3 localSlopeDirection { get; protected set; }
    public virtual bool OnSlopeGround()
    {
        return false;
    }

    public virtual void ResizeColider(float height)
    {
        var delta = height - this.height;
        controller.height = height;
        controller.center += Vector3.up * delta * 0.5f;
    }

    public virtual bool SphereCast(Vector3 direction, float distance,
         int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
    }

    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance,
         int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var origin = position - direction * radius + center;
        var offset = transform.up * (height * 0.5f - radius);
        var top = origin + offset;
        var bottom = origin - offset;
        return Physics.CapsuleCast(top, bottom, radius, direction, out hit, distance + radius, layer, queryTriggerInteraction);
    }

    public virtual int OverLapEntity(Collider[] result, float skinOffset = 0)
    {
        var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
        var overlapsRadius = radius + contactOffset;
        var offset = (height + contactOffset) * 0.5f - overlapsRadius;
        var top = position + Vector3.up * offset;
        var bottom = position + Vector3.down * offset;
        return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
    }

    public virtual void ApplyDamage(int amount, Vector3 origin)
    {
    }

    public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;
}

public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    public EntityStateManager<T> states { get; protected set; }

    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed, float deltaTime)
    {
        if (direction.sqrMagnitude > 0)
        {
            var speed = Vector3.Dot(direction, lateralVelocity);
            var velocity = direction * speed;
            var turningVelocity = lateralVelocity - velocity;
            var turningDelta = turningDrag * turningDragMultiplier * deltaTime;
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier * deltaTime;
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }
            velocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = velocity + turningVelocity;
        }
    }

    public virtual void Decelerate(float deceleration, float deltaTime)
    {
        var delta = deceleration * decelerationMultiplier * deltaTime;
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }

    protected virtual void Awake()
    {
        InitializeStateManager();
        InitializeController();
    }

    protected virtual void InitializeCollider()
    {
        m_collider = gameObject.AddComponent<CapsuleCollider>();
        m_collider.height = controller.height;
        m_collider.radius = controller.radius;
        m_collider.center = controller.center;
        m_collider.isTrigger = true;
        m_collider.enabled = false;
    }

    protected virtual void InitializeRigidbody()
    {
        m_rigidbody = gameObject.AddComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;
    }

    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }
        controller.skinWidth = 0.005f;
        controller.minMoveDistance = 0;
        originHeight = controller.height;
    }

    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

    public virtual void Gravity(float gravity, float deltaTime)
    {
        if (!isGrounded)
        {
            verticalVelocity += Vector3.down * gravity * gravityMultiplier * deltaTime;
        }
    }

    public virtual bool FitsIntoPosition(Vector3 position)
    {
        var radius = controller.radius - controller.skinWidth;
        var offset = height * 0.5f - radius;
        var top = position + Vector3.up * offset;
        var bottom = position + Vector3.down * offset;
        return !Physics.CheckCapsule(top, bottom, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    public virtual void FaceDirection(Vector3 direction, float degreesPerSecond, float deltaTime)
    {
        if (direction != Vector3.zero)
        {
            var rotation = transform.rotation;
            var rotationDelta = degreesPerSecond * deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
        }
    }

    public virtual void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = rotation;
        }
    }

    protected virtual void HandleController(float deltaTime)
    {
        if (controller.enabled)
        {
            controller.Move(velocity * deltaTime);
            return;
        }
        transform.position += velocity * deltaTime;
    }

    protected virtual void HandleStates(float deltaTime) => states.Step(deltaTime);

    protected virtual void HandleGround(float deltaTime)
    {
        if (onRails) return;
        var distance = (height * 0.5f) + m_groundOffset;
        if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            if (!isGrounded)
            {
                if (EvaluateLanding(hit))
                {
                    EnterGround(hit);
                }
            }
            else if (IsPointUnderStep(hit.point))
            {
                UpdateGround(hit);
            }
        }
        else
        {
            ExitGround(deltaTime);
        }
    }

    protected virtual void OnContact(Collider other)
    {
        if (other)
        {
            states.OnContact(other);
        }
    }

    protected virtual void HandleContacts()
    {
        var overlaps = OverLapEntity(m_contactBuffer);
        for (int i = 0; i < overlaps; i++)
        {
            OnContact(m_contactBuffer[i]);
            var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();
            foreach (var contact in listeners)
            {
                contact.OnEntityContact((T)this);
            }
            if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
            {
                verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
            }
        }
    }

    protected virtual void HandleSpline()
    {
        var distance = height * 0.5f + height * 0.5f;
        if (SphereCast(-transform.up, distance, out var hit) && hit.collider.CompareTag(GameTags.InteractiveRail))
        {
            if (!onRails && verticalVelocity.y <= 0)
            {
                EnterRail(hit.collider.GetComponent<SplineContainer>());
            }
        }
        else
        {
            ExitRail();
        }
    }

    protected virtual void EnterRail(SplineContainer rails)
    {
        if (!onRails)
        {
            onRails = true;
            this.rails = rails;
            entityEvents.OnRailsEnter.Invoke();
        }
    }

    public virtual void ExitRail()
    {
        if (onRails)
        {
            onRails = false;
            entityEvents.OnRailsExit.Invoke();
        }
    }

    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
        return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }

    protected virtual void EnterGround(RaycastHit hit)
    {
        if (!isGrounded)
        {
            groundHit = hit;
            isGrounded = true;
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    protected virtual void ExitGround(float deltaTime)
    {
        if (isGrounded)
        {
            isGrounded = false;
            transform.parent = null;
            // Note: In LockStep, we should use a logic time instead of Time.time
            // For now we use Time.time but it might need to be passed in
            lastGroundTime = Time.time; 
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    protected virtual void UpdateGround(RaycastHit hit)
    {
        if (isGrounded)
        {
            groundHit = hit;
            groundNormal = groundHit.normal;
            groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
            localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
        }
    }

    protected virtual void HandlePosition()
    {
        positionDelta = (position - lastPosition).magnitude;
        lastPosition = position;
    }

    public virtual void SnapToGround(float force)
    {
        if (isGrounded && (verticalVelocity.y <= 0))
        {
            verticalVelocity = Vector3.down * force;
        }
    }

    public virtual void UseCustomCollision(bool value)
    {
        controller.enabled = !value;
        if (value)
        {
            InitializeCollider();
            InitializeRigidbody();
        }
        else
        {
            Destroy(m_collider);
            Destroy(m_rigidbody);
        }
    }

    public virtual void LogicUpdate(float deltaTime)
    {
        if (controller.enabled)
        {
            HandleStates(deltaTime);
            HandleController(deltaTime);
            HandleGround(deltaTime);
            HandleContacts();
            HandleSpline();
            OnUpdate(deltaTime);
        }
    }

    protected virtual void Update()
    {
        // Core logic driven by LogicUpdate from ClientFrameSync
    }

    protected virtual void LateUpdate()
    {
        if (controller.enabled)
        {
            HandlePosition();
        }
    }

    protected virtual void OnUpdate(float deltaTime) { }
}
