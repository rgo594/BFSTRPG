using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiMove : MovementController
{
    GameObject target;
    public int healthPoints = 100;
    public int attack = 25;
    Slider healthBar;
    public GameObject targetedPlayer;
    public bool attacking = false;

    void Start()
    {
        Init();

        healthBar = gameObject.transform.GetChild(1).GetComponentInChildren<Slider>();
        healthBar.maxValue = healthPoints;
        healthBar.value = healthPoints;

        TurnManager.AddNpcUnit(this);
    }


    public void FindPath(TileFunctions target)
    {
        ComputeAdjacencyLists(target, false);
        GetCurrentTile();

        List<TileFunctions> openList = new List<TileFunctions>();
        List<TileFunctions> closedList = new List<TileFunctions>();

        openList.Add(currentTile);

        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        //29:14 part 6: explains the three steps of A*
        while (openList.Count > 0)
        {
            TileFunctions lowest = FindLowestF(openList);

            closedList.Add(lowest);

            if (lowest == target)
            {
                actualTargetTile = FindEndTile(lowest);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (TileFunctions tile in lowest.adjacencyList)
            {

                if (closedList.Contains(tile))
                {
                    //do nothing, already processed
                }
                else if (openList.Contains(tile))
                {
                    //this case is triggered when multiple paths exist in the open list, and compares them to find the shortest path among them
                    //determines which path is better if multiple exists
                    float tempG = lowest.g + Vector3.Distance(tile.transform.position, lowest.transform.position);
                    if (tempG < tile.g)
                    {
                        tile.parent = lowest;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                }
                else
                {
                    //completely unprocessed tile =

                    tile.parent = lowest;

                    //distance to the beginning
                    //the g of the parent plus the distance to the parent
                    tile.g = lowest.g + Vector3.Distance(tile.transform.position, lowest.transform.position);
                    //estimated distance to the end
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h;

                    openList.Add(tile);
                }
            }
        }

        //18:20 part 6
        //triggers when there is no path to the target
        //TODO need to modify so that player gets as close as possible even when there is no path
        EndTurn();
        Debug.Log("Path not found");
    }

    public void FindNearestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");

        GameObject nearest = null;
        float distance = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);

            if (d < distance)
            {
                distance = d;
                nearest = obj;
            }
        }

        target = nearest;
    }

    protected TileFunctions FindEndTile(TileFunctions t)
    {
        Stack<TileFunctions> tempPath = new Stack<TileFunctions>();

        TileFunctions next = t.parent;
        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;

        }

        if (tempPath.Count <= move)
        {
            //if the tile is within range this is the tile we move towards
            return t.parent;
        }

        TileFunctions endTile = null;
        for (int i = 0; i <= move; i++)
        {
            //if the tile is out of range use this tile
            endTile = tempPath.Pop();
        }
        return endTile;
    }

    protected TileFunctions FindLowestF(List<TileFunctions> list)
    {

        TileFunctions lowest = list[0];

        foreach (TileFunctions t in list)
        {
            if (t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);

        //returns tile with shortest distance
        return lowest;
    }

    //TODO test if works with multiple teams. (probably will, UNTIL all of one teams units are killed);
    public void UnitDeath()
    {
        TurnManager.teamUnits[gameObject.tag].Remove(this);
        TurnManager.unitTurnOrder.Clear();
        Destroy(gameObject);
    }

    public void Move()
    {
        if (attacking) { return; }

        //FindAttackAbleTiles(); //works just as well as AIDetectPlayers, but I think AiDetectPlayers might be less costly
        AiDetectPlayerCharacters();
        if (path.Count > 0)
        {
            //returns obj at the top of the stack without removing;
            TileFunctions nextTileInPath = path.Peek();
            Vector3 target = nextTileInPath.transform.position;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                //TODO find a way to travel to point without affecting z axis
                var targetPos = new Vector3(target.x, target.y, -1f);
                gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                //temporary fix for z axis
                transform.position = new Vector3(target.x, target.y, -1f);
                path.Pop();

                //so ai attacks player when in range
                if (detectedEnemies.Count > 0)
                {
                    AiAttackAction();
                }
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;

            EndTurn();
        }
    }

    public void AiDetectPlayerCharacters()
    {
        float detectRange = (attackRange + 0.95f) + attackRange;
        Vector3 rng = new Vector3(detectRange, detectRange, -1f);
        Collider2D[] detectedPlayerCharacters = Physics2D.OverlapBoxAll(gameObject.transform.position, rng, 1f, 512);

        if (detectedPlayerCharacters.Length > 0)
        {
            FindAttackAbleTiles();
            //BFSTileMap(attackRange, false, false, true);
        }
    }

    public void AiAttackAction()
    {
        RemoveSelectableTiles();
        moving = false;

        targetedPlayer = detectedEnemies[0].detectedEnemy.gameObject;

        Vector3 playerPos = targetedPlayer.transform.position;
        Vector3 enemyPos = gameObject.transform.position;

        Animator animator = gameObject.GetComponent<Animator>();

        attacking = true;
        if (enemyPos.x < playerPos.x)
        {
            animator.SetTrigger("AttackRight");
        }
        else if (enemyPos.x > playerPos.x)
        {
            animator.SetTrigger("AttackLeft");
        }
        else if (enemyPos.y < playerPos.y)
        {
            animator.SetTrigger("AttackUp");
        }
        else if (enemyPos.y > playerPos.y)
        {
            animator.SetTrigger("AttackDown");
        }
    }

    public void DamageStep()
    {
        targetedPlayer.transform.GetChild(1).GetComponentInChildren<Slider>().value -= attack;
        targetedPlayer.GetComponent<PlayerMove>().healthPoints -= attack;
    }

    public void CalculatePath()
    {
        TileFunctions targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }

    public void StartTurn()
    {
        turn = true;
    }

    public void EndTurn()
    {
        ResetEnemyAddedTiles();
        attacking = false;
        targetedPlayer = null;
        turn = false;
        detectedEnemies.Clear();
        RemoveSelectableTiles();
        moving = false;
        TurnManager.AiTurnRotation();
    }

}
