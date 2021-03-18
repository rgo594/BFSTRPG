using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : PlayerMove
{
    void Update()
    {
        //resets character for next phase
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            actionCompleted = false;
            SetCharacterColor(Color.white);
            ToggleEndPhaseButton(false);
        }
        else
        {
            ToggleEndPhaseButton(true);
        }

        if (!turn || actionCompleted)
        {
            return;
        }
        if (!moving && Input.GetMouseButtonUp(1))
        {
            allowEnemyDetection = true;
            ResetCharacterTurn();
        }

        if (!moving && !unitMenuPresent && !attackStep && !actionCompleted)
        {
            ToggleEndPhaseButton(true);
            FindSelectableTiles();
            TargetTileToTravel();
        }
        else
        {
            ToggleEndPhaseButton(false);
            moving = true;
            Move();
        }
        if (unitMenuPresent)
        {
            allowEnemyDetection = false;
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
        if (attackStep)
        {
            ToggleUnitMenu(false);
            StartCoroutine(AttackAction());
        }
    }
}
