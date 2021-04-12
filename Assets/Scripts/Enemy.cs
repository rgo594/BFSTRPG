using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : AiMove
{
    bool enemySelected = false;

/*    private void OnMouseOver()
    {
        if(TurnManager.enemyPhase || enemySelected) { return; }
        ChangeSpriteColor(Color.red);
    }

    private void OnMouseExit()
    {
        if (TurnManager.enemyPhase) { return; }
        ChangeSpriteColor(Color.white);
    }*/

    void ChangeSpriteColor(Color color)
    {
        GetComponentInChildren<SpriteRenderer>().color = color;
    }

    void Update()
    {
        if (healthPoints <= 0)
        {
            //not sure if it works with multiple ai teams
            UnitDeath();
        }

        //shows enemy move range
        if (Input.GetMouseButtonDown(0) && !TurnManager.enemyPhase && !TurnManager.attackStep)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            try
            {
                if (hit.collider.gameObject != gameObject)
                {
                    enemySelected = false;
                    //might need to change to its own function
                    RemoveSelectableTiles();
                }
                else
                {
                    enemySelected = true;
                    ChangeSpriteColor(Color.white);
                    StartCoroutine(DelayFindSelectableTiles());
                }
            }
            //NullReference errors get triggered by ui buttons that are set to inactive
            catch (NullReferenceException) { }
        }


        if (!turn)
        {
            return;
        }
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
