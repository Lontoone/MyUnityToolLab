using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
public class GenshinCameraController : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The character to follow.")]
    public Transform target;
    [Tooltip("Optional pivot (will be created under target if left null).")]
    public Transform pivot;

    [Header("Offsets & Distance")]
    public Vector3 pivotOffset = new Vector3(0f, 1.45f, 0f); // chest height
    public float distance = 4.5f;
    public float minDistance = 1.6f;
    public float maxDistance = 6.0f;
    [Tooltip("Horizontal offset when aiming / over-shoulder.")]
    public float shoulderOffset = 0.6f;

    [Header("Orbit")]
    public float yaw = 0f;
    public float pitch = 15f;
    public float minPitch = -20f;
    public float maxPitch = 65f;
    public float yawSpeed = 220f;   // deg/sec per 1.0 input
    public float pitchSpeed = 180f; // deg/sec per 1.0 input
    public float rotationSmoothTime = 0.04f; // lower = snappier

    [Header("Zoom & FOV")]
    public float zoomSpeed = 3.0f;      // units per scroll unit
    public float baseFov = 60f;
    public float sprintFov = 68f;
    public float fovLerpSpeed = 6f;

    [Header("Collision")]
    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.25f;
    public float collisionBuffer = 0.1f;
    public float collisionLerp = 20f;

    [Header("Auto Align")]
    public bool autoAlignWhenMoving = true;
    [Tooltip("How quickly yaw aligns to movement when moving and not aiming/locked.")]
    public float autoAlignSpeed = 360f; // deg/sec
    [Tooltip("How strong movement needs to be (0..1) to trigger auto-align.")]
    public float moveThreshold = 0.2f;

    [Header("Lock-On")]
    public bool enableLockOn = true;
    [Tooltip("Press Lock to toggle; finds closest within screen radius.")]
    public float lockScreenRadius = 0.25f; // as fraction of min(screenW, screenH)
    public float lockMaxDistance = 25f;
    public string enemyTag = "Enemy";
    [Tooltip("How fast camera turns to keep lock target framed.")]
    public float lockTurnSpeed = 540f; // deg/sec

    [Header("Aiming")]
    public bool enableAim = true;
    public float aimPitchClampUp = 45f;
    public float aimPitchClampDown = -30f;
    public float aimDistance = 3.0f;

    [Header("Cursor")]
    public bool lockCursorOnPlay = true;

    /*
    [Header("Input (Input System)")]
    public InputActionReference lookAction;
    public InputActionReference zoomAction;
    public InputActionReference moveAction;
    public InputActionReference lockAction;
    public InputActionReference aimAction;
    public InputActionReference sprintAction;
    public InputActionReference resetAction;
    */

    // runtime
    Transform _internalPivot;
    Transform _cam;
    float _curDistance;
    float _yawVel, _pitchVel;
    float _targetFov;
    Transform _lockTarget;
    bool _isAiming;
    bool _isSprinting;
    Vector2 _oldMousePosition;

    // simple shake
    float _shakeAmp;
    float _shakeTimeLeft;
    Vector2 _shakeSeed;

    void OnEnable()
    {
        /*
        if (lookAction) lookAction.action.Enable();
        if (zoomAction) zoomAction.action.Enable();
        if (moveAction) moveAction.action.Enable();
        if (lockAction) lockAction.action.Enable();
        if (aimAction) aimAction.action.Enable();
        if (sprintAction) sprintAction.action.Enable();
        if (resetAction) resetAction.action.Enable();
        */
        _oldMousePosition = Input.mousePosition;
    }

    void OnDisable()
    {
        /*
        if (lookAction) lookAction.action.Disable();
        if (zoomAction) zoomAction.action.Disable();
        if (moveAction) moveAction.action.Disable();
        if (lockAction) lockAction.action.Disable();
        if (aimAction) aimAction.action.Disable();
        if (sprintAction) sprintAction.action.Disable();
        if (resetAction) resetAction.action.Disable();
        */
    }

    void Start()
    {
        if (lockCursorOnPlay) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        _cam = transform;
        if (target == null)
        {
            Debug.LogError("[GenshinLikeCamera] Please assign a Target.");
            enabled = false;
            return;
        }

        if (pivot == null)
        {
            _internalPivot = new GameObject("CameraPivot_Auto").transform;
            _internalPivot.SetParent(target, worldPositionStays: false);
            _internalPivot.localPosition = pivotOffset;
            pivot = _internalPivot;
        }

        _curDistance = distance;
        _targetFov = baseFov;
        var cam = GetComponent<Camera>();
        if (cam) cam.fieldOfView = baseFov;

        // hook up buttons that are edge-triggered
        //if (lockAction) lockAction.action.performed += OnLockPressed;
        //if (resetAction) resetAction.action.performed += OnResetPressed;
    }

    void OnDestroy()
    {
        //if (lockAction) lockAction.action.performed -= OnLockPressed;
        //if (resetAction) resetAction.action.performed -= OnResetPressed;
    }

    void Update()
    {
        // state from buttons (hold)
        bool aimAction = Input.GetMouseButtonDown(0);
        bool sprintAction = Input.GetKey(KeyCode.LeftShift);
        _isAiming = enableAim && aimAction ;
        _isSprinting = sprintAction;

        HandleLookInput();
        HandleZoomInput();
        //HandleAutoAlign();
        UpdateFov();
        UpdateShake(Time.deltaTime);
        _oldMousePosition = Input.mousePosition;
    }

    void LateUpdate()
    {
        if (!target || !pivot) return;

        // Position pivot to follow target + offset
        pivot.position = target.TransformPoint(pivotOffset);

        // Compute desired camera local position (with aim shoulder offset)
        float desiredDistance = _isAiming ? aimDistance : distance;
        float targetDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

        // rotate pivot from yaw/pitch
        Quaternion desiredRot = Quaternion.Euler(pitch, yaw, 0f);

        // apply lock-on framing if any
        if (enableLockOn && _lockTarget)
        {
            Vector3 toTarget = (_lockTarget.position - pivot.position).normalized;
            var lockEuler = Quaternion.LookRotation(toTarget, Vector3.up).eulerAngles;
            float desiredYaw = lockEuler.y;
            float desiredPitch = lockEuler.x;

            yaw = Mathf.MoveTowardsAngle(yaw, desiredYaw, lockTurnSpeed * Time.deltaTime);
            // allow some manual influence if player still moves stick/mouse
            float pitchClampUp = _isAiming ? aimPitchClampUp : maxPitch;
            float pitchClampDn = _isAiming ? aimPitchClampDown : minPitch;
            pitch = Mathf.Clamp(Mathf.MoveTowardsAngle(pitch, desiredPitch, lockTurnSpeed * Time.deltaTime), pitchClampDn, pitchClampUp);
            desiredRot = Quaternion.Euler(pitch, yaw, 0f);
        }

        // shoulder offset
        Vector3 lateral = desiredRot * Vector3.right * (_isAiming ? shoulderOffset : 0f);
        Vector3 back = desiredRot * Vector3.back * targetDistance;
        Vector3 desiredCamPos = pivot.position + lateral + back;

        // collision: spherecast from pivot toward desiredCamPos
        Vector3 hitCamPos = desiredCamPos;
        Vector3 dir = (desiredCamPos - pivot.position);
        float dist = dir.magnitude;
        if (dist > 0.001f)
        {
            dir /= dist;
            if (Physics.SphereCast(pivot.position, collisionRadius, dir, out var hit, dist + collisionBuffer, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                float adjust = Mathf.Max(0f, hit.distance - collisionBuffer);
                hitCamPos = pivot.position + dir * adjust;
            }
        }

        // smooth camera position (distance smoothing)
        _curDistance = Mathf.Lerp(_curDistance, (hitCamPos - pivot.position).magnitude, 1f - Mathf.Exp(-collisionLerp * Time.deltaTime));
        Vector3 finalPos = pivot.position + lateral + (desiredRot * Vector3.back * _curDistance);

        // Apply transform
        _cam.position = finalPos + SampleShakeOffset();
        _cam.rotation = desiredRot;
    }

    void HandleLookInput()
    {
        //Vector2 look = (Vector2)Input.mousePosition - _oldMousePosition;
        Vector2 look = new Vector2(Input.GetAxis("Mouse X") , Input.GetAxis("Mouse Y"));
        //Vector2 look = lookAction ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;

        // Reduce mouse delta sensitivity to "deg/sec" feel
        float dt = Time.deltaTime;
        float sx = yawSpeed * dt, sy = pitchSpeed * dt;

        // during aim, you often want a little slower rotation ¡V tweak if desired
        float aimFactor = _isAiming ? 0.75f : 1f;

        yaw = Mathf.SmoothDampAngle(yaw, yaw + look.x * sx * aimFactor, ref _yawVel, rotationSmoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, pitch - look.y * sy * aimFactor, ref _pitchVel, rotationSmoothTime);

        float pitchMin = _isAiming ? aimPitchClampDown : minPitch;
        float pitchMax = _isAiming ? aimPitchClampUp : maxPitch;
        pitch = ClampPitch(pitch, pitchMin, pitchMax);
    }

    void HandleZoomInput()
    {
        float zoomAction = Input.GetAxis("Mouse ScrollWheel");
        float z = zoomAction ;
        if (Mathf.Abs(z) > 0.001f)
        {
            distance = Mathf.Clamp(distance - z * zoomSpeed, minDistance, maxDistance);
        }
    }

    void HandleAutoAlign()
    {
        Vector2 moveAction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        if (!autoAlignWhenMoving || _isAiming || (_lockTarget != null)) return;
        Vector2 move = moveAction ;
        if (move.sqrMagnitude < moveThreshold * moveThreshold) return;

        // Assume target.forward is movement orientation (if your character uses a different facing, replace this).
        float targetYaw = target.eulerAngles.y;
        yaw = Mathf.MoveTowardsAngle(yaw, targetYaw, autoAlignSpeed * Time.deltaTime);
    }

    void UpdateFov()
    {
        var cam = GetComponent<Camera>();
        if (!cam) return;
        float desired = _isSprinting ? sprintFov : baseFov;
        _targetFov = Mathf.Lerp(_targetFov, desired, 1f - Mathf.Exp(-fovLerpSpeed * Time.deltaTime));
        cam.fieldOfView = _targetFov;
    }

    float ClampPitch(float angle, float min, float max)
    {
        angle = NormalizeAngle(angle);
        min = NormalizeAngle(min);
        max = NormalizeAngle(max);
        return Mathf.Clamp(angle, min, max);
    }

    float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        if (a < -180f) a += 360f;
        return a;
    }

    void OnLockPressed(InputAction.CallbackContext ctx)
    {
        if (!enableLockOn) return;

        if (_lockTarget)
        {
            _lockTarget = null; // toggle off
            return;
        }

        // Find closest enemy near screen center within radius
        var cam = GetComponent<Camera>();
        if (!cam) return;

        float maxRadiusPx = Mathf.Min(Screen.width, Screen.height) * lockScreenRadius;
        Transform best = null;
        float bestScore = float.PositiveInfinity;

        var enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (var e in enemies)
        {
            var t = e.transform;
            float d = Vector3.Distance(t.position, target.position);
            if (d > lockMaxDistance) continue;

            Vector3 sp = cam.WorldToScreenPoint(t.position);
            if (sp.z < 0f) continue; // behind camera
            Vector2 delta = new Vector2(sp.x - Screen.width * 0.5f, sp.y - Screen.height * 0.5f);
            float r = delta.magnitude;
            if (r <= maxRadiusPx)
            {
                // Prefer closer in screen space and world space
                float score = r + d * 10f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = t;
                }
            }
        }
        _lockTarget = best;
    }

    void OnResetPressed(InputAction.CallbackContext ctx)
    {
        // Snap yaw to player forward; clamp pitch
        yaw = target.eulerAngles.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    // --- Shake (simple) ---
    public void Shake(float amplitude = 0.1f, float duration = 0.15f)
    {
        _shakeAmp = amplitude;
        _shakeTimeLeft = duration;
        _shakeSeed = new Vector2(Random.value * 1000f, Random.value * 1000f);
    }

    void UpdateShake(float dt)
    {
        if (_shakeTimeLeft > 0f)
        {
            _shakeTimeLeft -= dt;
            if (_shakeTimeLeft <= 0f) _shakeAmp = 0f;
        }
    }

    Vector3 SampleShakeOffset()
    {
        if (_shakeAmp <= 0f) return Vector3.zero;
        float t = Time.time * 35f;
        float x = (Mathf.PerlinNoise(_shakeSeed.x, t) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(_shakeSeed.y, t) - 0.5f) * 2f;
        return new Vector3(x, y, 0f) * _shakeAmp;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (pivot)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pivot.position, 0.05f);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
            Gizmos.DrawWireSphere(pivot.position, collisionRadius);
        }
    }
#endif
}
