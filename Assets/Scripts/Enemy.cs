﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Enemy : AiMove
{
    void Update()
    {
        if (reset)
        {
            reset = false;
            SetTileDetectCharacter();
        }
        if (turnManager.preventClicking) { return; }
        //refresh enemy range after player completes an action
        if (TurnManager.enemyPhase)
        {
            allowEnemyRange = true;
            RemoveEnemyRangeTiles();
        }
        else if (enemySelected && !TurnManager.enemyPhase)
        {
            if (allowEnemyRange)
            {
                allowEnemyRange = false;
                StartCoroutine(DelayRefreshEnemyRange());
            }
        }
        if (enemySelected && TurnManager.pcActionCompleted && TurnManager.playerPhase)
        {
            if (TurnManager.refresh)
            {
                TurnManager.refresh = false;
                RefreshEnemyRange();
            }
        }
        if (healthPoints <= 0)
        {
            //not sure if it works with multiple ai teams
            UnitDeath();
        }

        //shows enemy move range
        if (Input.GetMouseButtonDown(0) && !TurnManager.enemyPhase && !TurnManager.attackStep) //&& turnManager.noneClicked)
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
                    //FindEnemyRangeTiles();
                    StartCoroutine(DelayShowEnemyRangeTiles());
                }
                else if (hit.collider.gameObject.tag == "Enemy" && enemySelected)
                {
                    //might need to change to its own function
                    RemoveEnemyRangeTiles();
                    enemySelected = false;
                }
            }
            //NullReference errors get triggered by ui buttons that are set to inactive
            catch (NullReferenceException) { }
        }

        if (!turn) { return; }

        if (!aggro)
        {
            FindPcsInRange();
            if (!enemyDetected)
            {
                EndTurn();
                return;
            }
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
