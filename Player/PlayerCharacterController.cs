using TMPro;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("References")]
    public bool braincell = false;
    public Transform orientation;
    public LayerMask groundMask;
    public RaycastHit groundHit;
    public Transform hand;
    public GameObject display;
    public Canvas uiCanvas;
    public SwitchModeUI switchModeUI;
    public AllyIcon allyIcon;
    private TextMeshProUGUI crosshair;

    public Camera switchModeCamera;

    [Header("Properties")]
    [Range(1,3)]
    public int jumps = 1;
    [Range(0,3)]
    public float moveSpeedMultiplier = 1f;
    public bool canHoldSpaceToHover = false;
    [Range(20, 90)]
    public float slideAngle = 37.5f;

    [Header("Cooldowns")]
    public float ability1Cooldown = 3f;
    public float ability2Cooldown = 5f;

    [Header("Vertical Forces")]
    public float gravityStrength = 45f;
    public float hoverHeight = 2f;
    public float hoverStrength = 45f;
    public float hoverDamp = 20f;

    [Header("Rotation Correction")]
    public float rotationCorrectionStrength = 100f;
    public float rotationCorrectionDamp = 70f;

    [Header("Switch Mode")]
    public float switchModeSpeed = 30f;
    public float switchModeFalloffPower = 3f;
    public float switchModeYFactor = 0.3f;
    public float switchModeRotationFactor = 2f;
    public float switchModeMouseSpeed = .4f;

    public Rigidbody rb { get; private set; }
    public CapsuleCollider capsuleCollider { get; private set; }
    public FirstPersonCamera cam { get; private set; }
    public PlayerHealth health { get; private set; }
    public Hitstop hitstop { get; private set; }
    public bool onGround { get; private set; }
    public float xInput { get; private set; }
    public float yInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool ability1Input { get; private set; }
    public bool ability2Input { get; private set; }
    public bool switchModeInput { get; private set; }
    public bool switch1 { get; private set; }
    public bool switch2 { get; private set; }
    public bool switch3 { get; private set; }
    public bool canSwitch { get; set; } = true;

    [HideInInspector] public float ability1Time = 0f;
    [HideInInspector] public float ability2Time = 0f;

    public MovementHandler movementHandler;

    [HideInInspector] public float switchModeTime = 0f;
    private Vector3 startPos;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        cam = GetComponentInChildren<FirstPersonCamera>();
        cam.Setup();
        if (braincell)
        {
            display.SetActive(false);
            allyIcon.gameObject.SetActive(false);
        }
        else
        {
            cam.gameObject.SetActive(false);
        }

        movementHandler = new GroundedMovementHandler();
        uiCanvas.gameObject.SetActive(true);
        crosshair = uiCanvas.GetComponentInChildren<TextMeshProUGUI>();
        hitstop = GetComponent<Hitstop>();
        health = GetComponent<PlayerHealth>();
        startPos = transform.position;
    }

    private void Update()
    {
        CheckGround();
        CheckInput();
        VisualUpdate();
        CalculateCooldowns();

        if (braincell)
        {
            BraincellUpdate();
        }
    }

    private void BraincellUpdate()
    {
        CheckSwitch();
        SwitchModeUpdate();
    }

    private void FixedUpdate()
    {        
        if (movementHandler.ShouldGravityApply(this))
        {
            ApplyGravity();
        }

        if (movementHandler.ShouldHoverApply(this))
        {
            Hover();
        }

        CorrectRotation();        
        
        movementHandler.Move(this);

        ReturnIfOutOfBounds();
    }

    private void ApplyGravity()
    {
        float gravity = jumpInput && canHoldSpaceToHover && rb.velocity.y < -4f ? gravityStrength * 0.2f : gravityStrength;
        rb.AddForce(Vector3.down * gravity , ForceMode.Acceleration);
    }

    private void Hover()
    {
        if (!onGround) return;
        float heightDiff = groundHit.distance - hoverHeight;
        rb.AddForce(Vector3.down * (heightDiff * hoverStrength + rb.velocity.y * hoverDamp), ForceMode.Acceleration);
    }

    private void CorrectRotation()
    {
        Quaternion correction = Quaternion.FromToRotation(transform.up, Vector3.up);
        correction.ToAngleAxis(out float alpha, out Vector3 axis);
        rb.AddTorque(axis.normalized * alpha * Mathf.Deg2Rad * rotationCorrectionStrength - rb.angularVelocity * rotationCorrectionDamp, ForceMode.Acceleration);
        Quaternion correction2 = Quaternion.FromToRotation(transform.forward, Vector3.forward);
        correction2.ToAngleAxis(out float alpha2, out Vector3 axis2);
        rb.AddTorque(axis2.normalized * alpha2 * Mathf.Deg2Rad * rotationCorrectionStrength - rb.angularVelocity * rotationCorrectionDamp, ForceMode.Acceleration);
    }

    private void ReturnIfOutOfBounds()
    {
        if (transform.position.y < -30f)
        {
            transform.position = startPos;
        }
    }

    private void CheckGround()
    {
        float raycastMargin = 0.5f;
        onGround = Physics.Raycast(
            transform.position + Vector3.up * (raycastMargin + 0.05f), Vector3.down, out groundHit,
            hoverHeight * 5.0f, groundMask
        );

        if (!onGround) groundHit.distance = float.PositiveInfinity;
        onGround &= groundHit.distance < hoverHeight + raycastMargin && Vector3.Angle(groundHit.normal, Vector3.up) < slideAngle;
    }

    private void CheckInput()
    {
        if (!braincell)
        {
            yInput = 0f;
            xInput = 0f;
            jumpInput = false;
            ability1Input = false;
            ability2Input = false;
            switch1 = false;
            switch2 = false;
            switch3 = false;
            return;
        }

        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetButton("Jump");
        ability1Input = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Return);
        ability2Input = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightCurlyBracket);
        switch1 = Input.GetKey(KeyCode.Alpha1) || Input.GetKey(KeyCode.Quote);
        switch2 = Input.GetKey(KeyCode.Alpha2) || Input.GetKey(KeyCode.Slash);
        switch3 = Input.GetKey(KeyCode.Alpha3) || Input.GetKey(KeyCode.Equals);

        switchModeInput = Input.GetMouseButton(1);
    }

    private void CheckSwitch()
    {
        if (switch1)
        {
            BraincellManager.Instance.SwitchCommand(0);
        }

        if (switch2)
        {
            BraincellManager.Instance.SwitchCommand(1);
        }

        if (switch3)
        {
            BraincellManager.Instance.SwitchCommand(2);
        }
    }

    private void VisualUpdate()
    {
        if (movementHandler is VisualUpdatable)
        {
            (movementHandler as VisualUpdatable).VisualUpdate(this);
        }

        HandWobble();
    }

    private void HandWobble()
    {
        if (!onGround) return;

        Vector3 wandPos = new Vector3(Mathf.Sin(Time.time * 5f), Mathf.Sin(Time.time * 6f));
        float wobbleMagnitude = Mathf.Clamp(rb.velocity.magnitude / 25f, 0f, 1f);
        wandPos *= wobbleMagnitude * 0.05f;
        hand.localPosition = wandPos;
    }

    private void CalculateCooldowns()
    {
        ability1Time = Mathf.Max(0f, ability1Time - Time.deltaTime);
        ability2Time = Mathf.Max(0f, ability2Time - Time.deltaTime);
    }

    public void SetCrosshairColor(Color color)
    {
        crosshair.color = color;
    }

    public void SwitchIn()
    {
        cam.gameObject.SetActive(true);
        display.SetActive(false);
        allyIcon.gameObject.SetActive(false);
        braincell = true;

        TransferRotationFromRigidbodyToOrientation();
    }

    public void SwitchOut()
    {
        cam.gameObject.SetActive(false);
        display.SetActive(true);
        allyIcon.gameObject.SetActive(true);
        braincell = false;
    }

    private void TransferRotationFromRigidbodyToOrientation()
    {
        Quaternion r = transform.rotation;
        transform.rotation = Quaternion.identity;
        orientation.rotation = r;
    }

    private void SwitchModeUpdate()
    {
        bool cantSwitchMode = !switchModeInput || !canSwitch || BraincellManager.Instance.transitionTime > 0f;
        if (cantSwitchMode)
        {
            if (switchModeTime > 0)
            {
                if (braincell)
                {
                    cam.gameObject.SetActive(true);
                    display.SetActive(false);
                }

                cam.tag = "MainCamera";
                switchModeCamera.tag = "Player";
                switchModeUI.ReleaseSwitch(this);
                switchModeCamera.gameObject.SetActive(false);
                Time.timeScale = 1f;

                PostProcessingManager.Instance.DisableSwitchMode();
            }

            switchModeTime = 0f;
            return;
        }

        if (switchModeTime == 0f)
        {
            cam.gameObject.SetActive(false);
            switchModeCamera.gameObject.SetActive(true);
            cam.tag = "Player";
            switchModeCamera.tag = "MainCamera";
            switchModeCamera.transform.rotation = orientation.rotation;
            switchModeCamera.transform.position = cam.transform.position;
            display.SetActive(true);

            PostProcessingManager.Instance.EnableSwitchMode(.45f, .1f);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.openSwitchMode, transform.position);
        }

        float t = Mathf.Pow(switchModeTime, 1f / switchModeFalloffPower);
        float z = -t * switchModeSpeed;
        float rx = Mathf.Pow(t, 0.5f) * switchModeRotationFactor;
        Vector3 forwardnoY = new Vector3(switchModeCamera.transform.forward.x, 0f, switchModeCamera.transform.forward.z).normalized;
        float ryfangle = Vector3.Angle(forwardnoY, orientation.forward);
        float dot = Vector3.Dot(forwardnoY, orientation.forward);
        float ryf = 8f / (8f + ryfangle);
        ryf = dot < -0.45f ? 0f : ryf;
        ryf = Quaternion.FromToRotation(forwardnoY, orientation.forward).y * Input.GetAxis("Mouse X") <= 0 ? ryf : 1f;
        ryf = ryf > 0f && ryf < 0.3f ? 0.3f : ryf;
        
        float ry = switchModeCamera.transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * switchModeMouseSpeed * ryf;
        float rz = switchModeCamera.transform.rotation.eulerAngles.z + Input.GetAxis("Mouse X") * switchModeMouseSpeed * ryf * 0.1f;
        
        switchModeCamera.transform.rotation = Quaternion.Euler(rx, ry, rz);
        switchModeCamera.transform.localPosition = cam.transform.localPosition + (orientation.transform.forward + Vector3.down * switchModeYFactor).normalized * z;

        Time.timeScale = 1f / (1f + t * 10f);

        switchModeUI.SwitchModeUpdate(this);

        switchModeTime += Time.unscaledDeltaTime;
    }
}
