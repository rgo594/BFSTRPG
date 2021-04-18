using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerMove
{

/*    private void OnMouseOver()
    {
        if(turn) { return; }
        GetComponentInChildren<SpriteRenderer>().color = Color.blue;
    }

    private void OnMouseExit()
    {
        if (turn) { return; }
        GetComponentInChildren<SpriteRenderer>().color = Color.white;
    }*/

    void Update()
    {
        if (healthPoints <= 0)
        {
            Destroy(gameObject);
        }
        //resets character for next phase
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            actionCompleted = false;
            SetCharacterColor(Color.white);
        }

        if (!turn || actionCompleted)
        {
            return;
        }
        if (moving)
        {
            turnManager.preventClicking = true;
        }
        else
        {
            turnManager.preventClicking = false;
        }
        if (turn)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        if (!moving && Input.GetMouseButtonUp(1))
        {
            ResetCharacterTurn();
        }

        if (!moving && !unitMenuPresent && !TurnManager.attackStep && !actionCompleted)
        {
            //BFSTileMap(move, false, true);
            FindSelectableTiles();
            TargetTileToTravel();
        }
        else
        {
            moving = true;
            Move();
        }
        if (unitMenuPresent && !attacking)
        {
            FindAttackAbleTiles();
        }
        //if there are enemies in range show attack button
        if (detectedEnemies.Count > 0)
        {
            ToggleAttackButton(true);
        }
        else
        {
            ToggleAttackButton(false);
        }
        //if attack button clicked allow clicking on enemy for damage step
        if (TurnManager.attackStep && !attacking)
        {
            ToggleUnitMenu(false);
            StartCoroutine(AttackAction());
        }
    }
}
