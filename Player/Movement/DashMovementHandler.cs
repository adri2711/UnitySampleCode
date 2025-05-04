using Managers;
using UnityEngine;

public class DashMovementHandler : MovementHandler
{
    public static float dashSpeed = 2700f;
    public static float dashSpeedFalloff = 800f;
    public static float dashSpeedFalloffPower = 2f;
    public static float horizontalDrag = 35f;
    public static float horizontalAirDrag = 25f;
    public static float bobbingStrength = 0.6f;
    public static float duration = .09f;

    private float dashTime = 0f;
    private Vector3 dashDirection;

    public DashMovementHandler(PlayerCharacterController player, Vector3 dashDirection)
    {
        this.dashDirection = dashDirection;
        player.cam.FovWarp(.8f / duration, 1.22f);
        player.canSwitch = false;
    }

    public void Move(PlayerCharacterController player)
    {
        //Forward momentum
        Quaternion groundNormalRotation = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal);
        Vector3 dashForwardDirection = (player.onGround ? groundNormalRotation : Quaternion.identity) * dashDirection;
        float chargeSpeedWithFalloff = dashSpeed - Mathf.Pow(dashTime / duration, 1f / dashSpeedFalloffPower) * dashSpeedFalloff;
        player.rb.AddForce(dashForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

        //Bobbing
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * player.rb.velocity * bobbingStrength, ForceMode.Acceleration);

        //Drag
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddForce(-horizontalVelocity * (player.onGround ? horizontalDrag : horizontalAirDrag), ForceMode.Acceleration);

        //Exit by time or if player charges off ledge
        dashTime += Time.fixedDeltaTime;
        if (dashTime >= duration)
        {
            Exit(player);
            return;
        }

        //Exit if hit obstacle
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, dashForwardDirection, out hit, 1f,
        LayerManager.GetRaycastLayers(), QueryTriggerInteraction.Ignore))
        {
            Exit(player);
            return;
        }
    }

    private void Exit(PlayerCharacterController player)
    {
        player.canSwitch = true;

        if (!player.onGround)
        {
            player.movementHandler = new AirborneMovementHandler();
        }
        else
        {
            player.movementHandler = new GroundedMovementHandler();
        }
    }
}
