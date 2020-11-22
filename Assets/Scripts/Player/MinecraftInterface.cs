using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinecraftInterface : PlayerAbility
{
    public World world;
    public float range;
    public Vector3 blockToDestroy;

    private Movement PM;

    // Start is called before the first frame update
    void Start()
    {
        PM = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit Target;
            if (Physics.Raycast(PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Target, range))
            {
                DestroyBlock(Target);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit Target;
            if (Physics.Raycast(PM.MainCamera.transform.position, PM.MainCamera.transform.forward, out Target, range))
            {
                CreateBlock(Target);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
            world.DestroyBlock(blockToDestroy);
    }

    public void DestroyBlock (RaycastHit target)
    {
        Debug.Log("Target: " + target.point);
        world.DestroyBlock(target.point + new Vector3(0, -1f, 0));
    }

    public void CreateBlock(RaycastHit target)
    {
        world.CreateBlock(target.point);
    }
}
