using UnityEngine;
using UnityEngine.VFX;

public class SpeedLinesVFX : MonoBehaviour
{
    private VisualEffect vfx;

    [Header("Player")]
    [SerializeField] PlayerCharacterController player;

    [Header("Particle Ranges")]
    [SerializeField] private float thresholdMin = 32f;
    [SerializeField] private float thresholdMax = 50f;
    [SerializeField] private float radiusMin = 1.5f;
    [SerializeField] private float radiusMax = 1.2f;
    [SerializeField] private float rateMin = 35f;
    [SerializeField] private float rateMax = 96f;
    [SerializeField] private float alphaMin = .1f;
    [SerializeField] private float alphaMax = .35f;

    [Header("Particle Gradients")]
    [SerializeField] [GradientUsage(true)] private Gradient defaultGradient;
    [SerializeField] [GradientUsage(true)] private Gradient chargeGradient;
    [SerializeField] [GradientUsage(true)] private Gradient hookGradient;

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.enabled = false;
    }

    private void Update()
    {
        float v = player.rb.velocity.magnitude;
        vfx.enabled = v > thresholdMin;
        if (v < thresholdMin) return;
        
        Gradient gradient = player.movementHandler is ChargeMovementHandler ? chargeGradient
            : (player.movementHandler is HookMovementHandler ? hookGradient : defaultGradient);

        vfx.SetGradient("ColorGradient", gradient);
        vfx.SetFloat("Radius", Mathf.Lerp(radiusMin, radiusMax, (v - thresholdMin) / thresholdMax));
        vfx.SetFloat("SpawnRate", Mathf.Lerp(rateMin, rateMax, (v - thresholdMin) / thresholdMax));
        vfx.SetVector2("AlphaRange", Vector2.Lerp(new Vector2(alphaMin, alphaMin * 0.1f), new Vector2(alphaMax, alphaMax * 0.1f), (v - thresholdMin) / thresholdMax));
    }
}
