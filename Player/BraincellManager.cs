using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BraincellManager : Singleton<BraincellManager>
{
    [Header("Player Controllers")]
    public PlayerCharacterController[] playerControllers;

    [Header("Visuals")]
    [GradientUsage(true)] public Gradient allyIconGradient;
    [SerializeField] float transitionDuration = 0.2f;
    [SerializeField] AnimationCurve slowDownCurve;
    [SerializeField] AnimationCurve lensDistortionCurve;

    [Header("Postprocess")]
    [SerializeField] VolumeProfile volumeProfile;
    [SerializeField] PostProcessingManager urpManager;

    private int currentCharacter = 0;
    public float transitionTime { get; private set; } = 0f;

    private LensDistortion lensDistortionComponent;

    private void Start()
    {
        for (int i = 0; i < playerControllers.Length; i++)
        {
            playerControllers[i].allyIcon.SetColor(allyIconGradient.Evaluate((float)i / playerControllers.Length));
        }

        lensDistortionComponent = volumeProfile.components.Find(
            (VolumeComponent vc) => vc.GetType() == typeof(LensDistortion)
        ) as LensDistortion;
    }

    private void Update()
    {
        if (transitionTime > 0f)
        {
            TransitionUpdate();
        }

        transitionTime = Mathf.Max(0f, transitionTime - Time.deltaTime);
    }

    private void TransitionUpdate()
    {
        float t = transitionTime / transitionDuration;
        float c = slowDownCurve.Evaluate(1 - t);
        Time.timeScale = 1 - c;
        if (transitionTime - Time.deltaTime <= 0f)
        {
            Time.timeScale = 1f;
        }

        lensDistortionComponent.intensity.value = lensDistortionCurve.Evaluate(1 - t);
    }

    public void SwitchCommand(int id)
    {
        if (id < 0 || id >= playerControllers.Length || transitionTime > 0 || id == currentCharacter 
            || !playerControllers[currentCharacter].canSwitch) return;

        Switch(id);
    }

    private void Switch(int id)
    {
        transitionTime = transitionDuration;
        urpManager.BraincellSwitchTransition(transitionDuration * 1.3f);
        
        DeactivatePlayerController(playerControllers[currentCharacter]);
        ActivatePlayerController(playerControllers[id]);
        
        currentCharacter = id;

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.swap, transform.position);
    }

    private void ActivatePlayerController(PlayerCharacterController c)
    {
        c.SwitchIn();
    } 

    private void DeactivatePlayerController(PlayerCharacterController c)
    {
        c.SwitchOut();
    }
}
