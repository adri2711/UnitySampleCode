using UnityEngine;

public class DashAbility : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int airDashes = 1;
    
    private int timesDashed = 0;
    private PlayerCharacterController player;
    private RaycastHit hit;

    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
    }
    
    private void Update()
    {
        if (!player.braincell) return;

        if (player.onGround)
        {
            timesDashed = 0;
        }

        if (player.ability1Input && player.ability1Time == 0f && timesDashed < airDashes)
        {
            Quaternion r = player.orientation.rotation;

            Vector3 dir = r * new Vector3(player.xInput, 0f, player.yInput).normalized;
            dir = dir == Vector3.zero ? new Vector3(player.cam.transform.forward.x, 0f, player.cam.transform.forward.z).normalized : dir;
            
            player.movementHandler = new DashMovementHandler(player, dir);
            player.ability1Time = player.ability1Cooldown;

            timesDashed++;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dash, transform.position);
        }
    }
}
