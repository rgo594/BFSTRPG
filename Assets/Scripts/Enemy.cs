using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AiMove
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
