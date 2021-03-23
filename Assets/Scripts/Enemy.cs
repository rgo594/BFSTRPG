using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AiMove
{
    bool showMoveTileMap = false;

    private void OnMouseDown()
    {
        if (TurnManager.enemyPhase) { return; }
        showMoveTileMap = !showMoveTileMap;
    }

    void Update()
    {
        if(healthPoints <= 0)
        {
            //not sure if it works with multiple ai teams
            UnitDeath();
        }

/*        if (Input.GetMouseButtonDown(0) && !TurnManager.enemyPhase)
        {
            RemoveSelectableTiles();
        }*/
        if (showMoveTileMap)
        {
            FindSelectableTiles();
        }
        else
        {
            RemoveSelectableTiles();
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
