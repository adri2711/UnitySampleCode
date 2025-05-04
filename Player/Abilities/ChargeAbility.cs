using UnityEngine;

public class ChargeAbility : MonoBehaviour
{
    private PlayerCharacterController player;
    
    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
    }

    private void Update()
    {
        if (!player.braincell) return;

        if (player.ability1Input && player.ability1Time == 0f && player.onGround)
        {
            player.movementHandler = new ChargeMovementHandler(player, player.orientation.forward);
            player.ability1Time = player.ability1Cooldown;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.charge, transform.position);
        }
    }
}
