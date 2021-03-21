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


    public void FindPath(Tile target)
    {
        ComputeAdjacencyLists(target, false);
        GetCurrentTile();

        List<Tile> openList = new List<Tile>();
        List<Tile> closedList = new List<Tile>();

        openList.Add(currentTile);

        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        //29:14 part 6: explains the three steps of A*
        while (openList.Count > 0)
        {
            Tile lowest = FindLowestF(openList);

            closedList.Add(lowest);

            if (lowest == target)
            {
                actualTargetTile = FindEndTile(lowest);
                MoveToTile(actualTargetTile);
                return;
            }

            foreach (Tile tile in lowest.adjacencyList)
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
        //todo - what do you do if there is no path to the target tile
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

    protected Tile FindEndTile(Tile t)
    {
        Stack<Tile> tempPath = new Stack<Tile>();

        Tile next = t.parent;
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

        Tile endTile = null;
        for (int i = 0; i <= move; i++)
        {
            //if the tile is out of range use this tile
            endTile = tempPath.Pop();
        }
        return endTile;
    }

    protected Tile FindLowestF(List<Tile> list)
    {

        Tile lowest = list[0];

        foreach (Tile t in list)
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

        //AiDetectPlayerCharacters();

        if(attacking)
        { return; }
        FindAttackAbleTiles();
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

            TurnManager.EndNpcTurn();
        }
    }

    //need to make this all more asynchronous
    public void AiDetectPlayerCharacters()
    {
        float detectRange = (attackRange + 0.95f) + attackRange;
        Vector3 rng = new Vector3(detectRange, detectRange, -1f);
        Collider2D[] detectedPlayerCharacters = Physics2D.OverlapBoxAll(gameObject.transform.position, rng, 1f, 512);

        if (detectedPlayerCharacters.Length > 0)
        {
            FindAttackAbleTiles();
        }
    }

    public void AiAttackAction()
    {
        RemoveSelectableTiles();
        moving = false;
        //trying to add it too many times.
        targetedPlayer = detectedEnemies[0].detectedEnemy.gameObject;

        Vector3 playerPos = targetedPlayer.transform.position;
        Vector3 enemyPos = gameObject.transform.position;

        Animator animator = gameObject.GetComponent<Animator>();

        //attacking = true;
        if (enemyPos.x < playerPos.x)
        {
            attacking = true;
            animator.SetTrigger("AttackRight");
        }
        else if (enemyPos.x > playerPos.x)
        {
            attacking = true;
            animator.SetTrigger("AttackLeft");
        }
        else if (enemyPos.y < playerPos.y)
        {
            attacking = true;
            animator.SetTrigger("AttackUp");
        }
        else if (enemyPos.y > playerPos.y)
        {
            attacking = true;
            animator.SetTrigger("AttackDown");
        }
        /*        detectedEnemies[0].gameObject.transform.GetChild(1).GetComponentInChildren<Slider>().value -= attack;
                detectedEnemies[0].gameObject.GetComponent<PlayerMove>().healthPoints -= 25;
                detectedEnemies.Clear();
                RemoveSelectableTiles();
                moving = false;
                TurnManager.EndNpcTurn();*/
    }

    public void EndTurnAfterAttack()
    {
        foreach(Tile tile in detectedEnemies)
        {
            tile.enemyAdded = false;
        }
        attacking = false;
        targetedPlayer = null;
        turn = false;
        detectedEnemies.Clear();
        RemoveSelectableTiles();
        moving = false;
        TurnManager.EndNpcTurn();
    }

    public void DamageStep()
    {
        //todo remove attack tile map while this step is going
        targetedPlayer.transform.GetChild(1).GetComponentInChildren<Slider>().value -= attack;
        targetedPlayer.GetComponent<PlayerMove>().healthPoints -= attack;
        //TurnManager.attackStep = false;
        //EndTurn();
    }


    public void CalculatePath()
    {
        Tile targetTile = GetTargetTile(target);
        FindPath(targetTile);
    }

    public void StartTurn()
    {
        turn = true;
    }

    public void EndTurn()
    {
        foreach (Tile tile in detectedEnemies)
        {
            tile.enemyAdded = false;
        }
        TurnManager.allowEnemyDetection = true;
        turn = false;
        StartCoroutine(ClearDetectedEnemies());
    }
}
