using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MovementController
{
    public Vector3 originalPosition;
    public GameObject unitMenuController;

    public int health = 100;
    public int attack = 25;

    public bool unitMenuPresent = false;
    public bool actionCompleted = false;
    public static bool attackStep = false;

    void Start()
    {
        Init();
        unitMenuController = GameObject.Find("UnitMenuController");
    }

    public void Move()
    {
        if (path.Count > 0)
        {
            //returns obj at the top of the stack without removing;
            Tile nextTileInPath = path.Peek();
            Vector3 target = nextTileInPath.transform.position;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                CalculateHeading(target);
                SetHorizontalVelocity();

                //Locomotion
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                //temporary fix for keeping unit z axis unaffected by calculateHeading
                transform.position = new Vector3(target.x, target.y, -1);
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;
            var unitMenu = unitMenuController.transform.GetChild(0).GetComponent<UnitMenu>();

            turnManager.characterSelected = true;
            ToggleUnitMenu(true);

            unitMenu.SetUnit(gameObject);

        }
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

    public void ResetCharacterTurn()
    {
        PlayerMove.attackStep = false;
        ToggleUnitMenu(false);
        turnManager.characterSelected = false;

        gameObject.transform.position = originalPosition;
        detectedEnemies.Clear();
    }

    public void EndPlayerCharacterTurn()
    {
        allowEnemyDetection = true;
        PlayerMove.attackStep = false;
        unitMenuPresent = false;
        actionCompleted = true;

        SetCharacterColor(Color.grey);
        turnManager.playerCharacterTurnCounter++;
        turnManager.ResetCharacter(gameObject);

        StartCoroutine(ClearDetectedEnemies());
    }

    public IEnumerator AttackAction()
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

    public void ToggleUnitMenu(bool active)
    {
        var unitMenu = unitMenuController.transform.GetChild(0).gameObject;
        unitMenuPresent = active;
        unitMenu.SetActive(active);
    }

    public void ToggleAttackButton(bool active)
    {
        unitMenuController.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(active);
    }

    public void ToggleEndPhaseButton(bool active)
    {
        unitMenuController.transform.GetChild(1).gameObject.SetActive(active);
    }

    public void SetCharacterColor(Color color)
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().color = color;
    }
}
