using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : AiMove
{
    void Update()
    {
        if(health <= 0)
        {
            TurnManager.teamUnits[gameObject.tag].Remove(this);
            TurnManager.unitTurnOrder.Clear();
            Destroy(gameObject);
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
