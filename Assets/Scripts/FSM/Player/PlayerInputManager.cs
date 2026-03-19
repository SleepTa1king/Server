using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public InputActionAsset actions;

    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;
    protected InputAction m_crouch;
    protected InputAction m_dash;
    protected InputAction m_stomp;
    protected InputAction m_spin;
    protected InputAction m_aridive;
    protected InputAction m_dive;
    protected InputAction m_glide;
    protected InputAction m_grindBrake;
    protected InputAction m_releaseLedge;
    protected InputAction m_pause;
    protected InputAction m_run;
    protected InputAction m_pickAndThrow;
    protected Camera m_camera;

    //最近一次按下跳跃的时间
    protected float? m_lastJumpTime;
    //跳跃缓冲时长
    protected const float k_jumpBuffer = 0.15f;

    protected const string k_mouseDeviceName = "Mouse";
    protected virtual void Awake() => CacheActions();

    protected virtual void Start()
    {
        m_camera = Camera.main;
        if (actions != null) actions.Enable();
    }

    protected virtual void Update()
    {
        if (m_jump != null && m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }
    protected virtual void OnEnable() => actions?.Enable();
    protected virtual void OnDisable() => actions?.Disable();
    protected virtual void CacheActions()
    {
        if (actions == null) return;
        m_movement = actions["Movement"];
        m_look = actions["Look"];
        m_jump = actions["Jump"];
        m_crouch = actions["Crouch"];
        m_dash = actions["Dash"];
        m_stomp = actions["Stomp"];
        m_spin = actions["Spin"];
        m_aridive = actions["AirDive"];
        m_dive = actions["Dive"];
        m_glide = actions["Glide"];
        m_grindBrake = actions["Grind Brake"];
        m_releaseLedge = actions["ReleaseLedge"];
        m_pause = actions["Pause"];
        m_run = actions["Run"];
        m_pickAndThrow = actions["PickAndDrop"];
    }

    protected float m_movementDirectionUnlockTime;

    public bool UseNetworkInput { get; set; } = false;
    public CSInput NetworkInput { get; set; } = new CSInput();

    // 掩码需与 ClientFrameSync 保持一致
    public const int MASK_JUMP = 1;
    public const int MASK_DASH = 2;
    public const int MASK_SPIN = 4;
    public const int MASK_PICK = 8;

    public virtual void LockMovementDirection(float direction = 0.25f)
    {
        m_movementDirectionUnlockTime = Time.time + direction;
    }

    public virtual Vector3 GetMovementDirection()
    {
        if (UseNetworkInput)
        {
            return new Vector3(NetworkInput.MoveX, 0, NetworkInput.MoveZ);
        }

        if (Time.time < m_movementDirectionUnlockTime)
            return Vector3.zero;
        
        if (m_movement == null) return Vector3.zero;
        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }

    protected float RemapToDeadzone(float value, float deadzone) => (value - (value > 0 ? -deadzone : deadzone)) / (1 - deadzone);

    public virtual Vector3 GetMovementCameraDirection()
    {
        if (UseNetworkInput)
        {
            return GetMovementDirection();
        }

        var direction = GetMovementDirection();
        
        // 增加安全检查：如果 m_camera 为空，尝试再次查找
        if (m_camera == null) m_camera = Camera.main;

        if (direction.sqrMagnitude > 0)
        {
            if (m_camera != null)
            {
                var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
                direction = rotation * direction;
                direction = direction.normalized;
            }
        }
        return direction;
    }

    public virtual Vector3 GetLookDirection()
    {
        if (m_look == null) return Vector3.zero;
        var value = m_look.ReadValue<Vector2>();
        if (isLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual bool isLookingWithMouse()
    {
        if (m_look == null || m_look.activeControl == null)
        {
            return false;
        }
        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    public virtual bool GetJumpDown()
    {
        if (UseNetworkInput)
        {
            return (NetworkInput.Buttons & MASK_JUMP) != 0;
        }

        if (m_lastJumpTime != null && Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }
        return false;
    }

    public virtual bool GetDashDown()
    {
        if (UseNetworkInput)
        {
            return (NetworkInput.Buttons & MASK_DASH) != 0;
        }
        return m_dash != null && m_dash.WasPressedThisFrame();
    }

    public virtual bool GetJumpUp()
    {
        if (UseNetworkInput) return false;
        return m_jump != null && m_jump.WasReleasedThisFrame();
    }
    public virtual bool GetStompDown() => UseNetworkInput ? false : (m_stomp != null && m_stomp.WasPressedThisFrame());
    public virtual bool GetSpinDown() => UseNetworkInput ? (NetworkInput.Buttons & MASK_SPIN) != 0 : (m_spin != null && m_spin.WasPressedThisFrame());
    public virtual bool GetAirDiveDown() => UseNetworkInput ? false : (m_aridive != null && m_aridive.WasPressedThisFrame());
    public virtual bool GetCrouchAndCraw() => UseNetworkInput ? false : (m_crouch != null && m_crouch.IsPressed());
    public virtual bool GetGrindBrake() => UseNetworkInput ? false : (m_grindBrake != null && m_grindBrake.IsPressed());
    public virtual bool GetDive() => UseNetworkInput ? false : (m_dive != null && m_dive.IsPressed());
    public virtual bool GetGlide() => UseNetworkInput ? false : (m_glide != null && m_glide.IsPressed());
    public virtual bool GetPauseDown() => UseNetworkInput ? false : (m_pause != null && m_pause.WasPressedThisFrame());
    public virtual bool GetReleaseLedgeDown() => UseNetworkInput ? false : (m_releaseLedge != null && m_releaseLedge.WasPressedThisFrame());
    public virtual bool GetRun() => UseNetworkInput ? false : (m_run != null && m_run.IsPressed());
    public virtual bool GetRunUp() => UseNetworkInput ? false : (m_run != null && m_run.WasReleasedThisFrame());
    public virtual bool GetPickAndDropDown() => UseNetworkInput ? (NetworkInput.Buttons & MASK_PICK) != 0 : (m_pickAndThrow != null && m_pickAndThrow.WasPressedThisFrame());
}
