using UnityEngine;

public class MatchCameraFOV : MonoBehaviour
{
    [SerializeField] Camera camToMatch;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        cam.fieldOfView = camToMatch.fieldOfView;
    }
}
