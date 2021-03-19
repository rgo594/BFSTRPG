using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiMove : MovementController
{
    GameObject target;
    public int health = 100;
    public int attack = 25;

    void Start()
    {
        Init();
        TurnManager.AddNpcUnit(this);
    }

    public void Move()
    {
        AiDetectPlayerCharacters();
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

    public void AiDetectPlayerCharacters()
    {
        float detectRange = (attackRange + 0.8f) + attackRange;
        Vector3 rng = new Vector3(detectRange, detectRange, -1f);
        Collider2D[] detectedPlayerCharacters = Physics2D.OverlapBoxAll(gameObject.transform.position, rng, 1f, 512);

        if (detectedPlayerCharacters.Length > 0)
        {
            FindAttackAbleTiles();
        }
    }

    public void AiAttackAction()
    {
        detectedEnemies[0].gameObject.transform.GetChild(1).GetComponentInChildren<Slider>().value -= attack;
        detectedEnemies[0].gameObject.GetComponent<PlayerMove>().health -= 25;
        detectedEnemies.Clear();
        RemoveSelectableTiles();
        moving = false;
        TurnManager.EndNpcTurn();
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
        turn = false;
        TurnManager.allowEnemyDetection = true;
        StartCoroutine(ClearDetectedEnemies());
    }
}
