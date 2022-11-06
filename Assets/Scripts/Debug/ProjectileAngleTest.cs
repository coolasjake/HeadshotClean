using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileAngleTest : MonoBehaviour
{
    public Transform target;

    public float lateralSpeed;
    public float gravity = 9.8f;
    public Transform gun;

    public GameObject projectilePre;

    public Vector3 calculatedVelocity;
    public float velocityMag;

    bool autoAim = false;
    bool autoFire = false;
    float lastShot = 0;
    public float autoShootCooldown = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Aim();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            autoAim = !autoAim;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            autoFire = !autoFire;
        }

        if (autoAim)
            Aim();

        if (autoFire)
        {
            if (Time.time > lastShot + autoShootCooldown)
            {
                lastShot = Time.time;
                Shoot();
            }
        }
    }

    private void Aim()
    {
        Vector3 targetPos = target.position;
        Vector3 diff = targetPos - gun.position;
        Vector3 diffGround = new Vector3(diff.x, 0f, diff.z);

        if (fts.solve_ballistic_arc_lateral(gun.position, lateralSpeed, gravity, target.position, Vector3.zero, out calculatedVelocity))
        {
            velocityMag = calculatedVelocity.magnitude;

            float angle = Vector3.Angle(Vector3.right, calculatedVelocity);
            Quaternion newRot = new Quaternion();
            newRot.eulerAngles = new Vector3(0, 0, angle);
            gun.localRotation = newRot;
        }
    }

    private void Shoot()
    {
        GameObject GO = Instantiate(projectilePre, gun.position, Quaternion.identity);
        StartCoroutine(MoveProjectile(GO, calculatedVelocity));
    }

    private IEnumerator MoveProjectile(GameObject projectile, Vector3 startVel)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float startTime = Time.time;

        Vector3 velocity = startVel;

        while (Time.time < startTime + 10f)
        {
            velocity += new Vector3(0, -gravity * Time.deltaTime, 0);
            //velocity += (Vector2)(Physics.gravity * Time.deltaTime);
            projectile.transform.position += velocity * Time.deltaTime;
            yield return wait;
        }

        Destroy(projectile);
    }
}
