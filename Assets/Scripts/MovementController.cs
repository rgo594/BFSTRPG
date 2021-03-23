using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public bool turn = false;
    public bool moving = false;

    public int move = 5;
    public float moveSpeed = 10;
    public int attackRange = 2;

    public Tile currentTile;
    //public List<GameObject> detectedEnemies = new List<GameObject>();
    public List<Tile> detectedEnemies = new List<Tile>();

    public Stack<Tile> path = new Stack<Tile>();

    public Vector3 velocity = new Vector3();
    public Vector3 heading = new Vector3();

    public TurnManager turnManager;

    List<Tile> selectableTiles = new List<Tile>();
    GameObject[] tiles;

    public Tile actualTargetTile;

    public bool enemyDetected;

    protected void Init()
    {

        turnManager = FindObjectOfType<TurnManager>();

        tiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    public Tile GetTargetTile(GameObject target)
    {
        Vector3 playerTilePos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + 1);

        //Casts a ray from under the board to tile player character is standing on.
        //Only works because it ignores character layer, cant get raycast to not hit player character
        RaycastHit2D hit = Physics2D.Raycast(playerTilePos, new Vector3(0, 0, 1), 1f, 9);

        Tile tile = null;

        if (hit.collider != null)
        {
            tile = hit.collider.GetComponent<Tile>();
        }

        return tile;
    }

    public void ComputeAdjacencyLists(Tile target, bool attackable)
    {
        foreach (GameObject tile in tiles)
        {
            Tile startTile = tile.GetComponent<Tile>();
            startTile.FindNeighbors(target, attackable);
        }
    }

    public void BFSTileMap(int range, bool detectable = false, bool selectable = false, bool attackable = false)
    {
        if (detectable && enemyDetected) { return; }

        bool unblockable = false;

        if (attackable || detectable)
        {
            unblockable = true;
        }

        ComputeAdjacencyLists(null, unblockable);
        GetCurrentTile();

        List<Tile> adjacencyList = new List<Tile>();

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);

        currentTile.visited = true;

        while (process.Count > 0)
        {
            //remove and return the tile
            Tile dequeuedTile = process.Dequeue();

            if (attackable || detectable)
            {
                adjacencyList = dequeuedTile.unblockableAdjacencyList;
            }
            else if (selectable)
            {
                adjacencyList = dequeuedTile.adjacencyList;
            }

            selectableTiles.Add(dequeuedTile);
            if (dequeuedTile.distance < range)
            {
                //dequeuedTile.selectable = true; (just in case having selectable be in the foreach messes things up)
                foreach (Tile tile in adjacencyList)
                {
                    if (!tile.visited)
                    {
                        if (tile.detectedEnemy != null && tile.detectedEnemy.tag != gameObject.tag && !tile.enemyAdded)
                        {
                            if (detectable)
                            {
                                enemyDetected = true;
                            }
                            if (attackable)
                            {
                                tile.enemyAdded = true;
                                detectedEnemies.Add(tile);
                            }
                        }

                        if (attackable)
                        {
                            tile.attackable = true;
                        }
                        if (selectable)
                        {
                            tile.selectable = true;
                        }

                        tile.visited = true;
                        tile.parent = dequeuedTile;
                        tile.distance = 1 + dequeuedTile.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }
    }

    public void FindAttackAbleTiles()
    {
        BFSTileMap(attackRange, false, false, true);
    }

    public void FindSelectableTiles()
    {
        BFSTileMap(move, false, true);
    }
    public void FindEnemiesInRange()
    {
        BFSTileMap(move + attackRange, true);
    }

    public void MoveToTile(Tile tile)
    {
        path.Clear();
        tile.target = true;
        moving = true;

        Tile next = tile;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
    }

    public void RemoveSelectableTiles()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (Tile tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    //prevents InvalidOperationException error
    public IEnumerator ClearDetectedEnemies()
    {
        yield return new WaitForEndOfFrame();
        detectedEnemies.Clear();
    }

    public IEnumerator DelayFindSelectableTiles()
    {
        yield return new WaitForEndOfFrame();
        FindSelectableTiles();
    }

    public void ResetEnemyAddedTiles()
    {
        foreach (Tile tile in detectedEnemies)
        {
            tile.enemyAdded = false;
        }
    }
}