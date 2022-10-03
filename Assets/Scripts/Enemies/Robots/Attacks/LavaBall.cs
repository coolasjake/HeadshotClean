using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBall : MonoBehaviour
{
    public TPWallExit exit;
    public GameObject lavaPuddlePre;
    public LayerMask raycastLayers = new LayerMask();
    public float maxPuddleDist = 10;

    private bool stoppleaseImbeggingyou = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (stoppleaseImbeggingyou)
            return;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, maxPuddleDist, raycastLayers))
        {
            Quaternion randomRot = new Quaternion();
            randomRot.eulerAngles = new Vector3(0, Random.Range(0f, 180f), 0);
            GameObject GO = Instantiate(lavaPuddlePre, hit.point, randomRot);

            StartCoroutine(MoveToGround(hit.point, GO.transform));

            TPWallExit[] allExits = FindObjectsOfType<TPWallExit>();
            float closest = float.PositiveInfinity;
            foreach (TPWallExit TP in allExits)
            {
                float dist = Vector3.Distance(TP.transform.position, transform.position);
                if (dist < closest)
                {
                    exit = TP;
                    closest = dist;
                }
            }

            GO.GetComponent<TeleportWall>().Exit = exit;
            stoppleaseImbeggingyou = true;

            gameObject.SetActive(false);
        }
    }

    private IEnumerator MoveToGround(Vector3 point, Transform lavaPuddle)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        lavaPuddle.localScale = new Vector3(0, 0, 0);

        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());

        float velocity = 1f;

        while (transform.position != point)
        {
            velocity += Time.deltaTime * Physics.gravity.magnitude;
            transform.position = Vector3.MoveTowards(transform.position, point, velocity * Time.deltaTime);
            yield return wait;
        }
        
        StartCoroutine(GrowLavaPuddle(lavaPuddle));
    }

    private IEnumerator GrowLavaPuddle(Transform lavaPuddle)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float scale = 0f;
        float inverse = 1f;
        while (scale < 1)
        {
            scale += Time.deltaTime;
            if (scale > 1)
                scale = 1;
            inverse = 1 - scale;
            lavaPuddle.localScale = new Vector3(scale, scale, scale);
            transform.localScale = new Vector3(inverse, inverse, inverse);
            yield return wait;
        }
        Destroy(this);
        Destroy(gameObject);
    }
}
