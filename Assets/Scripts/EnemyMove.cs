using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MovementController
{
    GameObject target;
    public int health = 100;
    public int attack = 25;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (!turn)
        {
            return;
        }

        if (!moving)
        {
            FindNearestTarget();
            CalculatePath();
            FindSelectableTiles(false);
            actualTargetTile.target = true;
        }
        else
        {
            Move();
        }
    }

    void CalculatePath()
    {
        Tile targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }

    void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);

            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }

        target = nearest;
    }
}
