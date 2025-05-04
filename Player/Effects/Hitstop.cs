using UnityEngine;

public class Hitstop : MonoBehaviour
{
    private float hitstopT = 0f;
    private float aftershockT = 0f;

    void Update()
    {
        if (hitstopT > 0)
        {
            Time.timeScale = hitstopT - Time.unscaledDeltaTime <= 0f ? 1f : 0.02f;
        }
        else if (aftershockT > 0)
        {
            Time.timeScale = aftershockT - Time.unscaledDeltaTime <= 0f ? 1f : 0.2f;
        }
        
        hitstopT = Mathf.Max(0f, hitstopT - Time.unscaledDeltaTime);
        aftershockT = Mathf.Max(0f, aftershockT - Time.unscaledDeltaTime);
    }

    public void Add(float t)
    {
        hitstopT += t;
    }

    public void AddAftershock(float t)
    {
        aftershockT += t;
    }
}
