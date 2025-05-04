using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    [Header("Singleton")]
    [SerializeField] protected bool _persistent = true;

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            return _instance = new GameObject($"(Singleton){typeof(T)}").AddComponent<T>();
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (_persistent) DontDestroyOnLoad(gameObject);
    }
}