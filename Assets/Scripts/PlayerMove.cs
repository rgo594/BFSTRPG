using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MovementController
{
    public static bool attackStep = false;

    public int health = 100;
    public int attack = 25;

    void Start()
    {
        //Debug.Log(LayerMask.GetMask("Enemy"));
        Init();
    }

    void Update()
    {  
        //starts player phase
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            actionCompleted = false;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
            unitMenuController.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            unitMenuController.transform.GetChild(1).gameObject.SetActive(true);
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
            FindSelectableTiles();
            TargetTileToTravel();
        }
        else
        {
            Move();
        }
        if (unitMenuPresent)
        {
            allowEnemyDetection = false;
            FindAttackAbleTiles();
        }
        //if there are enemies in range show attack button
        if (detectedEnemies.Count > 0 && gameObject.tag == "Player")
        {
            enemiesInRange = true;
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

    private void ToggleAttackButton(bool active)
    {
        unitMenuController.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(active);
    }

    public void TargetTileToTravel()
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

    IEnumerator AttackAction()
    {
        yield return new WaitUntil(() => PlayerMove.attackStep == true);
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
                    PlayerMove.attackStep = false;

                    EndPlayerCharacterTurn();
                    ToggleUnitMenu(false);
                }
            }
        }
    }
}
