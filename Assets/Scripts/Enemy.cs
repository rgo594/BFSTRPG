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
        //FindEnemiesInRange();
        FindEnemiesInRange();
        if (!enemyDetected)
        {
            EndTurn();
            return;
        }
        if (!moving)
        {
            FindNearestTarget();
            CalculatePath();
            FindSelectableTiles();
            //BFSTileMap(move, false, true);
            actualTargetTile.target = true;
        }
        else
        {
            Move();
        }
    }
}
