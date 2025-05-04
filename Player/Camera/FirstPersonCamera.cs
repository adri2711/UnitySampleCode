using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("FOV")]
    public float fov = 100f;
    [SerializeField] private AnimationCurve fovAnimationCurve;

    [Header("Sensitivity")]
    public float verticalSensitivity = 400f;
    public float horizontalSensitivity = 400f;

    [Header("Orientation")]
    public Transform orientation;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private float smoothRotationTimer = 1f;
    private float smoothRotationSpeed = 1f;
    private float xRotationFrom = 0f;
    private float yRotationFrom = 0f;
    private float xRotationDest = 0f;
    private float yRotationDest = 0f;

    private Camera cam;
    private float fovAnimationTimer = 1f;
    private float fovAnimationSpeed = 1f;
    private float fovAnimationScale = 1f;

    private float shakeTime = 0f;
    private float shakeStrength = 1f;
    private int shakeQuadrant = 0;
    private Vector3 shakeOffset = new Vector3();
    private Vector3 startPos;

    public void Setup()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = GetComponent<Camera>();

        startPos = transform.localPosition;
        xRotation = orientation.rotation.eulerAngles.x;
        yRotation = orientation.rotation.eulerAngles.y;
    }

    void Update()
    {
        // Uses unscaled delta time so it's not affected by slow motion mechanics
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.unscaledDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.unscaledDeltaTime;

        // Yaw, unrestricted
        yRotation += mouseX;

        // Pitch, restricted so you don't unintentionally frontflip and snap your neck
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // If we have started a smooth rotation, continue the animation
        if (smoothRotationTimer < 1f)
        {
            // Uses hermite interpolation, saves it on a temp to use it on LerpAngle
            float smoothCurve = Mathf.SmoothStep(0f, 1f, smoothRotationTimer);
            xRotation = Mathf.Lerp(xRotationFrom, xRotationDest, smoothCurve);

            // LerpAngle ensures we handle wrapping around 360º adequately
            yRotation = Mathf.LerpAngle(yRotationFrom, yRotationDest, smoothCurve);

            smoothRotationTimer += Time.unscaledDeltaTime * smoothRotationSpeed;
        }

        // Rotate camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Update utility rotation transform
        float parentCorrection = orientation.parent.rotation.eulerAngles.y;
        orientation.rotation = Quaternion.Euler(0f, yRotation + parentCorrection, 0f);

        // Fov animation, controlled by a curve in the inspector
        if (fovAnimationTimer < 1f)
        {
            // Multiply by scale the difference from full scale in the curve
            float distortionAmt = (fovAnimationCurve.Evaluate(fovAnimationTimer) - 1f) * fovAnimationScale + 1f;
            cam.fieldOfView = fov * distortionAmt;
            fovAnimationTimer += Time.unscaledDeltaTime * fovAnimationSpeed;
        }
        else
        {
            cam.fieldOfView = fov;
        }

        ShakeUpdate();
    }

    private void ShakeUpdate()
    {
        if (shakeTime <= 0) return;        

        shakeQuadrant = (shakeQuadrant + 90) % 360;
        float angle = (Random.Range(0, 90) + shakeQuadrant) * Mathf.Deg2Rad;
        shakeOffset = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);

        Vector2 axis = new Vector2(1f, 1f);
        shakeOffset.x *= axis.x * Random.Range(0f, 1f);
        shakeOffset.y *= axis.y * Random.Range(0f, 1f);
        shakeOffset.z = 0f;

        transform.localPosition = startPos + shakeOffset * shakeStrength;

        shakeTime = Mathf.Max(0f, shakeTime - Time.unscaledDeltaTime);
        if (shakeTime == 0f)
        {
            transform.localPosition = startPos;
        }
    }

    public void ScreenShake(float duration, float strength = .3f)
    {
        shakeTime = duration;
        shakeStrength = strength;
    }

    public void SmoothRotation(float speed, float yaw = 2711, float pitch = 2711)
    {
        // I guess this is how we do dynamic default parameters in C#
        xRotationDest = pitch == 2711 ? xRotation : pitch;
        yRotationDest = yaw == 2711 ? yRotation : yaw - orientation.parent.rotation.eulerAngles.y;

        // Set up rotation
        smoothRotationTimer = 0f;
        smoothRotationSpeed = speed;
        xRotationFrom = xRotation;
        yRotationFrom = yRotation;
    }

    public void FovWarp(float speed, float scale)
    {
        fovAnimationTimer = 0f;
        fovAnimationSpeed = speed;
        fovAnimationScale = scale;
    }

    public void StopFovWarp()
    {
        if (fovAnimationTimer > .9f) return;
        
        fovAnimationTimer = 0.1f;
        fovAnimationSpeed = 8f;
        fovAnimationScale = (fovAnimationCurve.Evaluate(fovAnimationTimer) - 1f) * fovAnimationScale + 1f;
    }
}
