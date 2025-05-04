using Managers;
using Unity.VisualScripting;
using UnityEngine;

public class HookAbility : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Material hookshotChainMaterial;

    [Header("Stats")]
    [SerializeField] private float range = 100f;
    [SerializeField] private float radius = .5f;

    [Header("Snap")]
    [SerializeField] private float ledgeSnapLeniency = 4f;
    [SerializeField] private float ledgeSnapNormalThreshold = .5f;
    [SerializeField] private float ledgeSnapDistance = 1f;

    [Header("Color")]
    [SerializeField] private Color availableCrosshairColor = new Color(.9f, .9f, .1f);
    [SerializeField] private Color cooldownCrosshairColor = new Color(.1f, .9f, .9f);
    [SerializeField] private Color outOfRangeCrosshairColor = new Color(1f, 1f, 1f);

    private PlayerCharacterController player;
    private RaycastHit hit;

    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();

        var line = player.AddComponent<LineRenderer>();
        line.startWidth = 0.4f;
        line.endWidth = 0.3f;
        line.material = hookshotChainMaterial;
    }

    private void Update()
    {
        if (!player.braincell) return;

        Vector3 startPos;
        Vector3 dir = player.cam.transform.forward;
        startPos = player.transform.position + new Vector3(0f, .5f, 0f);

        int layer = LayerManager.GetRaycastLayersWithoutAlly();
        bool success = Physics.SphereCast(startPos + dir * 2f, radius, dir, out hit, range, layer, QueryTriggerInteraction.Ignore);

        if(!success)
        {
            player.SetCrosshairColor(outOfRangeCrosshairColor);
            return;
        }

        if (player.ability1Time != 0f)
        {
            player.SetCrosshairColor(cooldownCrosshairColor);
            return;
        }

        player.SetCrosshairColor(availableCrosshairColor);

        if (player.ability1Input)
        {
            RaycastHit ledgeHit;
            Vector3 dirNoY = new Vector3(dir.x, 0f, dir.z).normalized; 

            bool ledgeFound = Physics.SphereCast(hit.point + dirNoY * .15f + ledgeSnapLeniency * Vector3.up, .5f, Vector3.down, out ledgeHit, ledgeSnapLeniency, LayerManager.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);
            ledgeFound &= ledgeHit.point != Vector3.zero;
            ledgeFound &= Vector3.Dot(Vector3.up, ledgeHit.normal) > ledgeSnapNormalThreshold;
            ledgeFound &= Mathf.Abs(Vector3.Dot(hit.normal, ledgeHit.normal)) < 0.3f;

            Vector3 endPos = hit.point;
            Vector3 endVisualPos = ledgeFound ? ledgeHit.point : hit.point;
            if (ledgeFound) {
                Vector3 ledgeHitDir = (ledgeHit.point - startPos).normalized;
                Vector3 normalOffset = (ledgeHitDir + ledgeHit.normal).normalized * ledgeSnapDistance;
                endPos = ledgeHit.point + new Vector3(0f, .65f, 0f) + normalOffset;
            }

            player.movementHandler = new HookMovementHandler(player, startPos, endPos, endVisualPos, ledgeFound);
            player.ability1Time = player.ability1Cooldown;

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.hookThrow, transform.position);
        }
    }
}
