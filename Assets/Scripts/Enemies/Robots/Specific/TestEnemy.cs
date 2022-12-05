using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

class TestEnemy : EnemyFramework
{
    public Transform head;
    public FiringPoint FP;
    public LayerMask raycastShootingMask;

    private bool LookAround = false;
    private float LookAroundStarted = -1000;
    private Vector3 LookAroundDirection = Vector3.zero;

    public float LazerDPS = 1f;
    private bool Freeze = false;

    private Quaternion HeadRotationBeforeFiring;
    private Quaternion PlayerDirectionBeforeFiring;
    private Vector3 LastDecalPoint = new Vector3();

    protected override void Alarmed()
    {
        base.Alarmed();

        if (detection._playerVisibility == 0)
            StartLookingAround();
        else
            LookAround = false;
    }

    protected override void Searching()
    {
        base.Searching();

        if (detection._playerVisibility == 0)
            StartLookingAround();
        else
            LookAround = false;

        if ((Vector3.Distance(transform.position, detection._lastPlayerGroundedPosition) < 5))
        {
            StartLookingAround();
        }
    }

    protected override void StartCharging()
    {
        base.StartCharging();

        FP.DangerLight.enabled = true;
        Freeze = true;
        SFXPlayer.PlaySound("Charge", 1, 15, 30, 1, 1, false);
    }

    protected override void StartFiring()
    {
        base.StartFiring();

        FP.DangerLight.intensity = 1.5f;
        FP.EffectLine.enabled = true;
        //FP.ParticleEffect.Play ();
        HeadRotationBeforeFiring = head.transform.rotation;
        PlayerDirectionBeforeFiring = Quaternion.LookRotation(PlayerMovement.ThePlayer.transform.position - head.transform.position);
        SFXPlayer.PlaySound("Fire", 1, 15, 30, 1, 1, false);
    }

    protected override void StopFiring()
    {
        base.StopFiring();

        FP.DangerLight.enabled = false;
        FP.EffectLine.enabled = false;
        Freeze = false;
    }

    protected override void Firing()
    {
        base.Firing();

        Fire();
    }

    private void Fire()
    {
        //Rotate by: Time since this started firing, divided by half of the fire duration.
        float RotationFactor = (Time.time - combatAndStates._startedFiring) / (combatAndStates.fireDuration * 0.5f);
        head.transform.rotation = Quaternion.LerpUnclamped(HeadRotationBeforeFiring, PlayerDirectionBeforeFiring, RotationFactor);

        RaycastHit Hit;
        if (Physics.Raycast(FP.transform.position - FP.transform.up, -FP.transform.up, out Hit, 600, raycastShootingMask))
        {
            Vector3 LinePoint = FP.EffectLine.transform.InverseTransformPoint(Hit.point);
            FP.EffectLine.SetPosition(1, LinePoint);

            Shootable SH = Hit.collider.GetComponentInParent<Shootable>();
            if (SH)
                SH.Hit(LazerDPS * Time.deltaTime);
            else if (Time.timeScale > 0)
                SpawnBurnDecal(Hit);
        }
    }

    private void SpawnBurnDecal(RaycastHit Hit)
    {
        Quaternion DecalRotation = Quaternion.LookRotation(Hit.normal);
        GameObject LaserBurn;
        LastDecalPoint = Hit.point + (Hit.normal * 0.001f);
        LaserBurn = Instantiate(Resources.Load<GameObject>("Prefabs/LaserBurn"), LastDecalPoint, DecalRotation);
    }

    private void LookForPlayer(Transform body)
    {
        if (Time.time > LookAroundStarted + 0.8f)
        {
            LookAroundStarted = Time.time;
            LookAroundDirection = body.forward;
            LookAroundDirection += body.right * (Random.value - 0.5f) * 5;
            LookAroundDirection += body.up * (Random.value - 0.5f) * 3;
        }
        head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, Quaternion.LookRotation(LookAroundDirection), (90 * Time.deltaTime));
    }

    private void FacePlayer()
    {
        Quaternion targetRot = Quaternion.LookRotation(PlayerMovement.ThePlayer.MainCamera.transform.position - head.transform.position);
        head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, targetRot, (90 + (90 * detection._playerVisibility)) * Time.deltaTime);
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - head.transform.position;
        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, targetRot, (90 * Time.deltaTime));
    }

    private void DoHeadRotation()
    {
        if (detection._playerVisibility > 0.5f)
        {
            //Turn to face the player (lerp relative to PV).
            FacePlayer();
        }
        else if (combatAndStates.state == AIState.Searching || combatAndStates.state == AIState.Alarmed)
        {
            if (LookAround)
            {
                LookForPlayer(transform);
            }
            else
                //Turn to face the players assumed position.
                head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, Quaternion.LookRotation(detection._lastPlayerPosition - head.transform.position), (90 * Time.deltaTime));
        }
        else
        {
            //Turn to face forwards.
            head.transform.rotation = Quaternion.RotateTowards(head.transform.rotation, Quaternion.LookRotation(transform.forward), (90 * Time.deltaTime));
        }
    }

    public void StartLookingAround()
    {
        if (LookAround == false)
        {
            LookAroundStarted = Time.time - 0.8f;
            LookAround = true;
        }
    }
}
