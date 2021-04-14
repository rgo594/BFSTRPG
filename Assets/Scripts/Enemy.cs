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
        if (Input.GetMouseButtonDown(0) && !TurnManager.enemyPhase && !TurnManager.attackStep && turnManager.noneClicked)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);
            try
            {
                //if (hit.collider.gameObject.transform.position != gameObject.transform.position) { return; }

                if (hit.collider.gameObject.transform.position == gameObject.transform.position && !enemySelected)
                {
                    enemySelected = true;
                    ChangeSpriteColor(Color.white);
                    StartCoroutine(DelayFindSelectableTiles());
                }
                else if (hit.collider.gameObject.tag == "Enemy" && enemySelected)
                {
                    enemySelected = false;
                    //might need to change to its own function
                    WoogaBooga();
                }
                else
                {
                    RemoveSelectableTiles();
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
            actualTargetTile.target = true;
        }
        else
        {
            Move();
        }
    }
}
