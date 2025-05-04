using Managers;
using Unity.VisualScripting;
using UnityEngine;

public class ChargeMovementHandler : MovementHandler
{
    public static float chargeSpeed = 1000f;
    public static float chargeSpeedFalloff = 575f;
    public static float chargeSpeedFalloffPower = 4f;
    public static float swerveSpeed = 280f;
    public static float horizontalDrag = 15f;
    public static float bobbingStrength = 0.6f;
    public static float duration = .7f;
    public static float bounceStrength = 1900f;
    public static float bounceStrengthDamaged = 2200f;
    public static float bounceVerticalAddition = .45f;

    public static float collideEffectTime = .365f;
    public static float collideHitstopTime = .12f;
    public static float collideAftershockTime = .17f;
    public static float collideScreenshakeTime = .2f;
    public static float collideScreenshakeStrength = .8f;

    private float chargeTime = 0f;
    private float bobbingCycle = 0f;
    private Vector3 chargeDirection;

    public ChargeMovementHandler(PlayerCharacterController player, Vector3 chargeDirection)
    {
        this.chargeDirection = chargeDirection;
        player.cam.FovWarp(1f, 2f);
        PostProcessingManager.Instance.ChargeRunEffect(duration);
        player.canSwitch = false;
    }

    public void Move(PlayerCharacterController player)
    {
        //Lateral movement
        Vector3 swerveDirection = (player.orientation.right * player.xInput).normalized;
        swerveDirection = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * swerveDirection;
        player.rb.AddForce(swerveDirection * swerveSpeed, ForceMode.Acceleration);

        //Forward movement
        Vector3 chargeForwardDirection = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * chargeDirection;
        float chargeSpeedWithFalloff = chargeSpeed - Mathf.Pow(chargeTime / duration, 1f / chargeSpeedFalloffPower) * chargeSpeedFalloff;
        player.rb.AddForce(chargeForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

        //Bobbing
        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * player.rb.velocity * bobbingStrength, ForceMode.Acceleration);
        
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        player.rb.AddTorque(chargeDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        //Drag
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

        //Exit by time or if player charges off ledge
        chargeTime += Time.fixedDeltaTime;
        if (!player.onGround || chargeTime >= duration)
        {
            Exit(player);
            return;
        }

        //Exit if hit obstacle
        RaycastHit hit;
        if (Physics.Raycast(player.transform.position + 0.5f * Vector3.up, chargeDirection, out hit, 1f, 
            LayerManager.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore) &&
            Vector3.Angle(hit.normal, Vector3.up) > AirborneMovementHandler.slideAngle)
        {
            Collide(player, hit.normal);
            return;
        }
    }

    private void Collide(PlayerCharacterController player, Vector3 normal)
    {
        PostProcessingManager.Instance.ChargeCollideEffect(collideEffectTime);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.hammerAttack, player.transform.position);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.enemyDamage, player.transform.position);

        player.cam.StopFovWarp();
        player.hitstop.Add(collideHitstopTime);
        player.cam.ScreenShake(collideScreenshakeTime ,collideScreenshakeStrength);
        player.hitstop.AddAftershock(collideAftershockTime);
        player.rb.velocity = Vector3.zero;

        Vector3 bounceForce = (new Vector3(normal.x, Mathf.Max(0f, normal.y), normal.z).normalized + 
            new Vector3(0f, bounceVerticalAddition, 0f)).normalized * (bounceStrength);
        player.rb.AddForce(bounceForce, ForceMode.Acceleration);

        player.canSwitch = true;

        player.movementHandler = new AirborneMovementHandler();
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
