using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : EnemyMove
{
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
            FindSelectableTiles();
            actualTargetTile.target = true;
        }
        else
        {
            Move();
        }
    }
}
