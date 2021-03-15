using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MovementController
{
    int health = 100;
    int attack = 25;

    public bool enemiesInRange = false;
    public bool attackAction = false;

    public Collider2D[] detectedEnemies;

    public float attackRange = 1f;

    void Start()
    {
        //Debug.Log(LayerMask.GetMask("Enemy"));
        Init();
    }

    private void OnDrawGizmos()
    {
        float detectRange = (attackRange + 0.8f) + attackRange;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectRange, detectRange));
    }
    void Update()
    {  
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }

        if (!turn)
        {
            return;
        }
        if (!moving && Input.GetMouseButtonUp(1))
        {
            gameObject.transform.position = originalPosition;
            ToggleUnitMenu(false);
            turnManager.characterSelected = false;
        }
        if (!moving && !unitMenuPresent && !attackAction)
        {
            FindSelectableTiles();
            SelectTile();
        }
        else
        {
            Move();
        }
        if(unitMenuPresent)
        {
            DetectEnemies();
        }
        if (attackAction)
        {
            StartCoroutine(AttackEnemy());
        }
    }

    public void SelectTile()
    {
        if (Input.GetMouseButtonUp(0))
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

    public void DetectEnemies()
    {
        float detectRange = (attackRange + 0.8f) + attackRange;
        Vector3 halfExtents = new Vector3(detectRange, detectRange, -1f);

        //creates a box to detect any objects with colliders, not sure what angle parameter does
        detectedEnemies = Physics2D.OverlapBoxAll(transform.position, halfExtents, 1f, 1024);
        if (detectedEnemies.Length > 0)
        {
            enemiesInRange = true;
        }
    }

    IEnumerator AttackEnemy()
    {
        yield return new WaitUntil(() => attackAction == true);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);


        foreach (Collider2D enemy in detectedEnemies)
        {
            var enemyMove = enemy.GetComponent<EnemyMove>();
            enemyMove.standingTile.gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.red;

            if (Input.GetMouseButtonUp(0))
            {
               
                if (hit.collider.gameObject == enemy.gameObject)
                {
                    var targetedEnemy = enemy.gameObject.GetComponent<EnemyMove>();
                    targetedEnemy.health -= attack;
                    attackAction = false;

                    EndPlayerCharacterTurn();
                    ToggleUnitMenu(false);
                }
            }
        }
    }
}
