using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AiMove
{
    void Update()
    {
        if(healthPoints <= 0)
        {
            //not sure if it works with multiple ai teams
            UnitDeath();
        }
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
