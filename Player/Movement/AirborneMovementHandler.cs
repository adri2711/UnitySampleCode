using Managers;
using UnityEngine;

public class AirborneMovementHandler : MovementHandler
{
    public static float strafeSpeed = 30f;
    public static float torqueStrength = 0.6f;
    public static float horizontalDrag = 2.85f;
    public static float slideSpeed = 50f;
    public static float slideDistance = 2.5f;
    public static float slideAngle = 37.5f;

    public static float jumpStrength = 18f;
    public static float jumpForwardStrength = 11f;

    public static float doubleJumpDelay = 0.8f;
    public static float doubleJumpFovWarpIntensity = 2.8f;
    public static float doubleJumpFovWarpDuration = 0.35f;

    private float groundedTimer = 0f;
    private float jumpTimer = 0.6f;

    private float maxVelocity = 30f;

    private int doubleJumps = 0;
    private bool canDoubleJump = false;

    public void Move(PlayerCharacterController player)
    {
        // Mid-air strafe, with less control than grounded movement
        Vector3 direction = (player.orientation.right * player.xInput + player.orientation.forward * player.yInput).normalized;

        //Avoid friction with walls
        RaycastHit hit;
        var playerCollider = player.capsuleCollider;
        Vector3 p1 = player.transform.position + playerCollider.center + Vector3.up * -playerCollider.height * 0.5f;
        Vector3 p2 = p1 + Vector3.up * playerCollider.height;

        if (Physics.CheckCapsule(p1, p2, playerCollider.radius + 0.075f, LayerManager.GetRaycastLayers(), QueryTriggerInteraction.Ignore) &&
            Physics.CapsuleCast(p1, p2, playerCollider.radius, direction, out hit, playerCollider.radius + 1f,
            LayerManager.GetRaycastLayers(), QueryTriggerInteraction.Ignore))
        {
            Vector3 newDir = (direction + new Vector3(hit.normal.x, 0f, hit.normal.z)).normalized;
            newDir *= Vector3.Dot(direction, newDir);
            direction = newDir;
        }

        player.rb.AddForce(direction * strafeSpeed * player.moveSpeedMultiplier, ForceMode.Acceleration);

        // Lean in the direction we are moving
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * horizontalVelocity * torqueStrength, ForceMode.Acceleration);
        player.rb.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

        // If we aren't moving upwards and hit the ground, we are grounded
        if (player.onGround)
        {
            groundedTimer += 0.1f;
            if (player.rb.velocity.y < -player.gravityStrength / 2f + 0.01f || groundedTimer > 0.1f &&
            player.rb.velocity.y < 0.01f || groundedTimer > 0.3f)
            {
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.land, player.transform.position);
                player.movementHandler = new GroundedMovementHandler();
            }            
        }
        else
        {
            groundedTimer = 0f;
        }

        // Double jump
        if (jumpTimer > 0.0f) {
            jumpTimer -= 0.1f;
        }
        else 
        {
            if (canDoubleJump && doubleJumps < player.jumps - 1 && player.jumpInput)
            {
                Rigidbody rb = player.rb;
                Vector3 velocity = rb.velocity;
                velocity.y = jumpStrength;
                
                Vector3 forwardVelocity = player.orientation.transform.forward * jumpForwardStrength * player.yInput;
                velocity += forwardVelocity;
                rb.velocity = velocity;

                jumpTimer = doubleJumpDelay;
                doubleJumps++;
                canDoubleJump = false;
                
                player.cam.FovWarp(doubleJumpFovWarpIntensity, doubleJumpFovWarpDuration);
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dash, player.transform.position);
            }
            else if (!canDoubleJump && !player.jumpInput)
            {
                canDoubleJump = true;
            }
        }

        // Slide down steep slopes
        if (player.groundHit.distance < slideDistance && Vector3.Angle(player.groundHit.normal, Vector3.up) > slideAngle)
        {
            player.rb.AddForce(Vector3.down * slideSpeed, ForceMode.Acceleration);
        }

        // Cap velocity
        if (player.rb.velocity.magnitude > maxVelocity)
        {
            player.rb.AddForce(player.rb.velocity.normalized * (maxVelocity - player.rb.velocity.magnitude), ForceMode.VelocityChange);
        }
    }

    public bool ShouldHoverApply(PlayerCharacterController player)
    {
        return false;
    }
}