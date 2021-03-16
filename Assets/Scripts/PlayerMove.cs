using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MovementController
{
    int health = 100;
    int attack = 25;

    public bool attackAction = false;

    void Start()
    {
        //Debug.Log(LayerMask.GetMask("Enemy"));
        Init();
    }

    void Update()
    {  
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            actionCompleted = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }

        if (!turn || actionCompleted)
        {
            return;
        }
        if (!moving && Input.GetMouseButtonUp(1))
        {
            ra = true;
            ResetCharacterTurn();
        }
        if (!moving && !unitMenuPresent && !attackAction && !actionCompleted)
        {
            FindSelectableTiles();
            SelectTile();
        }
        else
        {
            Move();
        }
        if (unitMenuPresent)
        {
            ra = false;
            //detects enemies
            FindAttackAbleTiles(true);
        }
        if(detectedEnemies.Count > 0)
        {
            //if there are enemies in range show attack button
            enemiesInRange = true;
            ToggleAttackButton(true);
        }
        else
        {
            ToggleAttackButton(false);
        }
        if (attackAction)
        {
            //if attack button clicked allow clicking on enemy for damage step
            ToggleUnitMenu(false);
            StartCoroutine(AttackEnemy());
        }
    }

    private void ToggleAttackButton(bool active)
    {
        unitMenuController.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(active);
    }

    private void ResetCharacterTurn()
    {
        TurnManager.attackStep = false;
        gameObject.transform.position = originalPosition;
        ToggleUnitMenu(false);
        turnManager.characterSelected = false;
        attackAction = false;
        detectedEnemies.Clear();
    }

    public void SelectTile()
    {
        if (Input.GetMouseButtonUp(0) && !actionCompleted)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if (hit)
            {
                if (hit.collider.tag == "Tile")
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>();

                    if (clickedTile.selectable)
                    {
                        MoveToTile(clickedTile);
                    }
                }
            }
        }
    }

    IEnumerator AttackEnemy()
    {
        yield return new WaitUntil(() => attackAction == true);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);


        foreach (GameObject enemy in detectedEnemies)
        {
            var enemyMove = enemy.GetComponent<EnemyMove>();

            if (Input.GetMouseButtonUp(0))
            {
               
                if (hit.collider.gameObject == enemy)
                {
                    var targetedEnemy = enemy.GetComponent<EnemyMove>();
                    targetedEnemy.health -= attack;
                    attackAction = false;

                    EndPlayerCharacterTurn();
                    ToggleUnitMenu(false);
                }
            }
        }
    }
}
