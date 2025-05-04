using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PostProcessingManager : Singleton<PostProcessingManager>
{
    public Dictionary<string, FullScreenPassRendererFeature> fullscreenPasses = new Dictionary<string, FullScreenPassRendererFeature>();

    [Header("Effect intensity curves")]
    [SerializeField] private AnimationCurve braincellSwitchCurve;
    [SerializeField] private AnimationCurve chargeRunCurve;
    [SerializeField] private AnimationCurve chargeBounceCurve;
    [SerializeField] private AnimationCurve switchModeCurve;
    [SerializeField] private AnimationCurve switchModeOutCurve;
    [SerializeField] private AnimationCurve damageCurve;

    private Coroutine chargeRunCoroutine;
    private bool switchMode = false;

    private void Start()
    {
        GetFullscreenPasses();
    }

    public void DamageEffect(float t)
    {
        FullScreenPassRendererFeature pass = fullscreenPasses["Damage"];
        StartCoroutine(EffectCoroutine(pass, t, damageCurve));
    }

    public void ChargeRunEffect(float t)
    {
        FullScreenPassRendererFeature pass = fullscreenPasses["Charge"];
        chargeRunCoroutine = StartCoroutine(EffectCoroutine(pass, t, chargeRunCurve));
    }

    public void ChargeCollideEffect(float t)
    {
        FullScreenPassRendererFeature pass = fullscreenPasses["Charge"];
        if (chargeRunCoroutine != null) StopCoroutine(chargeRunCoroutine);
        StartCoroutine(ChargeCollideCoroutine(pass, t, chargeBounceCurve));
    }

    private IEnumerator ChargeCollideCoroutine(FullScreenPassRendererFeature pass, float duration, AnimationCurve curve)
    {
        pass.SetActive(true);

        Color cellColor = pass.passMaterial.GetColor("_CellColor");
        Color intenseCellColor = new Color(1f, .01f, 0f);

        float fade = pass.passMaterial.GetFloat("_Fade");
        float intenseFade = .8f;

        float t = 0f;
        while (t < duration)
        {
            float p = curve.Evaluate(t / duration);

            pass.passMaterial.SetFloat("_Progress", p);
            pass.passMaterial.SetFloat("_Fade", Mathf.Lerp(intenseFade, fade, p));
            pass.passMaterial.SetColor("_CellColor", new Color(
                Mathf.Lerp(intenseCellColor.r, cellColor.r, p),
                Mathf.Lerp(intenseCellColor.g, cellColor.g, p),
                Mathf.Lerp(intenseCellColor.b, cellColor.b, p), 1f)
            );

            t += Time.fixedUnscaledDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        pass.passMaterial.SetFloat("_Fade", fade);
        pass.passMaterial.SetColor("_CellColor", cellColor);

        pass.SetActive(false);
    }

    public void EnableSwitchMode(float duration, float outDuration)
    {
        switchMode = true;
        StartCoroutine(SwitchModeCoroutine(switchModeCurve, switchModeOutCurve, duration, outDuration));
    }

    public void DisableSwitchMode()
    {
        switchMode = false;
    }

    private IEnumerator SwitchModeCoroutine(AnimationCurve inCurve, AnimationCurve outCurve, float duration, float outDuration)
    {
        FullScreenPassRendererFeature bPass = fullscreenPasses["BraincellSwitch"];
        FullScreenPassRendererFeature sPass = fullscreenPasses["SwitchMode"];

        bPass.SetActive(true);
        sPass.SetActive(true);

        float bFade = bPass.passMaterial.GetFloat("_Fade");
        float sFade = sPass.passMaterial.GetFloat("_Fade");

        float t = 0f;
        while (t < duration && switchMode)
        {
            bPass.SetActive(true);

            float c = inCurve.Evaluate(t / duration);

            bPass.passMaterial.SetFloat("_Progress", c);
            bPass.passMaterial.SetFloat("_Fade", Mathf.Lerp(bFade, 0f, c));
            sPass.passMaterial.SetFloat("_Progress", c);
            sPass.passMaterial.SetFloat("_Fade", Mathf.Lerp(sFade, 0f, c));

            t += 0.02f;
            yield return new WaitForSecondsRealtime(0.02f);
        }

        yield return new WaitUntil(() => !switchMode);

        bPass.passMaterial.SetFloat("_Fade", bFade);
        sPass.passMaterial.SetFloat("_Fade", sFade);
        bPass.SetActive(false);

        t = 0f;
        while (t < outDuration && !switchMode)
        {
            float c = outCurve.Evaluate(t / outDuration);
            sPass.passMaterial.SetFloat("_Progress", c);

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (t >= outDuration) {
            sPass.SetActive(false);
        }
    }


    public void BraincellSwitchTransition(float t)
    {
        FullScreenPassRendererFeature pass = fullscreenPasses["BraincellSwitch"];
        StartCoroutine(EffectCoroutine(pass, t, braincellSwitchCurve));
    }

    private IEnumerator EffectCoroutine(FullScreenPassRendererFeature pass, float duration, AnimationCurve curve)
    {
        pass.SetActive(true);

        float t = 0f;
        while (t < duration)
        {
            pass.passMaterial.SetFloat("_Progress", curve.Evaluate(t / duration));

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        pass.SetActive(false);
    }

    private void GetFullscreenPasses()
    {
        var handledDataObjects = new List<ScriptableRendererData>();

        int levels = QualitySettings.names.Length;
        for (int level = 0; level < levels; level++)
        {
            var asset = QualitySettings.GetRenderPipelineAssetAt(level) as UniversalRenderPipelineAsset;
            ScriptableRendererData data = GetDefaultRenderer(asset);

            if (handledDataObjects.Contains(data))
            {
                continue;
            }

            handledDataObjects.Add(data);

            foreach (var feature in data.rendererFeatures)
            {
                if (feature is FullScreenPassRendererFeature && !fullscreenPasses.ContainsKey(feature.name))
                {
                    fullscreenPasses.Add(feature.name, feature as FullScreenPassRendererFeature);
                }
            }
        }
    }

    static int GetDefaultRendererIndex(UniversalRenderPipelineAsset asset)
    {
        return (int) typeof(UniversalRenderPipelineAsset).GetField(
            "m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance
        ).GetValue(asset);
    }

    static ScriptableRendererData GetDefaultRenderer(UniversalRenderPipelineAsset asset)
    {
        if (!asset) return null;

        int defaultRendererIndex = GetDefaultRendererIndex(asset);
        var rendererDataList = (ScriptableRendererData[]) typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(asset);

        return rendererDataList[defaultRendererIndex];
    }
}
