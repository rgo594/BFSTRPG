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

    public TileFunctions currentTile;
    public List<TileFunctions> detectedEnemies = new List<TileFunctions>();

    public Stack<TileFunctions> path = new Stack<TileFunctions>();

    public Vector3 velocity = new Vector3();
    public Vector3 heading = new Vector3();

    public TurnManager turnManager;

    public List<TileFunctions> selectableTiles = new List<TileFunctions>();
    public List<TileFunctions> attackableTiles = new List<TileFunctions>();

    GameObject[] tiles;

    public TileFunctions actualTargetTile;
    public bool enemyDetected;

    public GameObject preventClicking;

    public Collider2D detectedTile;

    public bool reset = true;

    //public bool selectableReady = true;

    public void SetTileDetectCharacter()
    {
        //reset = false;
        if (detectedTile == null)
        {
            //TODO give tiles their own layer
            detectedTile = Physics2D.OverlapBox(transform.position, new Vector3(0.8f, 0.8f, 0), 1f, 1);
            TileFunctions tile = detectedTile.gameObject.GetComponent<TileFunctions>();
            tile.detectedEnemy = gameObject.GetComponent<Collider2D>();
        }
        else if (new Vector2(detectedTile.transform.position.x, detectedTile.transform.position.y) != new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))
        {
            TileFunctions tileDetected = detectedTile.gameObject.GetComponent<TileFunctions>();
            tileDetected.detectedEnemy = null;
            detectedTile = null;
        }
        reset = true;
    }

    protected void Init()
    {
        //preventClicking = GameObject.Find("PreventClicking");

        turnManager = FindObjectOfType<TurnManager>();

        tiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
    }

    public TileFunctions GetTargetTile(GameObject target)
    {
        Vector3 playerTilePos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + 1);

        //Casts a ray from under the board to tile player character is standing on.
        //Only works because it ignores character layer, cant get raycast to not hit player character
        RaycastHit2D hit = Physics2D.Raycast(playerTilePos, new Vector3(0, 0, 1), 1f, 9);

        TileFunctions tile = null;

        if (hit.collider != null)
        {
            tile = hit.collider.GetComponent<TileFunctions>();
        }

        return tile;
    }

    public void ComputeAdjacencyLists(TileFunctions target, bool attackable)
    {
        float rng = ((float)move + (float)attackRange) * 2.4f;
        Collider2D[] tilesInRange = Physics2D.OverlapBoxAll(gameObject.transform.position, new Vector2(rng, rng), 1f);

        foreach(Collider2D tile in tilesInRange)
        {
            if (tile.gameObject.GetComponent<TileFunctions>())
            {
                TileFunctions startTile = tile.gameObject.GetComponent<TileFunctions>();
                startTile.FindNeighbors(target, attackable);
            }
        }
    }

    public void BFSTileMap(int range, bool detectable = false, bool selectable = false, bool attackable = false, bool enemyRangeTile = false)
    {
        if (detectable && enemyDetected) { return; }

        ComputeAdjacencyLists(null, attackable);
        GetCurrentTile();

        Queue<TileFunctions> process = new Queue<TileFunctions>();

        process.Enqueue(currentTile);

        currentTile.visited = true;

        while (process.Count > 0)
        {
            //remove and return the tile
            TileFunctions dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);

            if (attackable)
            {
                attackableTiles.Add(dequeuedTile);
            }

            if (dequeuedTile.distance < range && !dequeuedTile.borderTile)
                {
                //if unit is on border tile need to check both of its adjacency lists
                foreach (TileFunctions tile in dequeuedTile.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        if (tile.detectedEnemy != null && tile.detectedEnemy.tag != gameObject.tag && !tile.enemyAdded)
                        {
                            if (detectable)
                            {
                                enemyDetected = true;
                                tile.colorAttackable = true;
                            }
                            if (attackable)
                            {
                                tile.enemyAdded = true;
                                detectedEnemies.Add(tile);
                            }
                        }

                        if (tile.occupied)
                        {
                            dequeuedTile.borderTile = true;
                        }
                        if (!tile.occupied || attackable)
                        {
                            TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, attackable, selectable, false);

                            if (ModifiedTile.distance == move)
                            {
                                ModifiedTile.borderTile = true;
                            }
                            process.Enqueue(ModifiedTile);
                        }
                    }
                }
            }
        }
        if(process.Count == 0)
        {
            ShowAttackRange(detectable, enemyRangeTile);
        }
    }

    public void ShowAttackRange(bool detectable, bool enemyRangeTile)
    {
        float rng = ((float)move + (float)attackRange) * 2.4f;
        Collider2D[] tilesInRange = Physics2D.OverlapBoxAll(gameObject.transform.position, new Vector2(rng, rng), 1f);

        Queue<TileFunctions> borderTiles = new Queue<TileFunctions>();

        foreach (Collider2D tile in tilesInRange)
        {
            if (tile.gameObject.GetComponent<TileFunctions>())
            {
                if (tile.gameObject.GetComponent<TileFunctions>().borderTile == true)
                {
                    borderTiles.Enqueue(tile.gameObject.GetComponent<TileFunctions>());
                }
            }
        }

        while (borderTiles.Count > 0)
        {
            TileFunctions dequeuedTile = borderTiles.Dequeue();
            selectableTiles.Add(dequeuedTile);

            foreach (TileFunctions tile in dequeuedTile.adjacencyList)
            {
                if (!tile.visited)
                {
                    tile.selectAttackable = true;
                    if(dequeuedTile.borderTile || dequeuedTile.distance < attackRange)
                    {
                        if (tile.detectedEnemy && detectable && tile.detectedEnemy.gameObject.tag != gameObject.tag)
                        {
                            enemyDetected = true;
                            tile.colorAttackable = true;
                        }
                        //TODO needs to be refactored
                        if (dequeuedTile.borderTile == true)
                        {
                            TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1, true, false, true);
                            borderTiles.Enqueue(ModifiedTile);
                        }
                        else
                        {
                            TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, true, false, true);
                            borderTiles.Enqueue(ModifiedTile);
                        }
                    }
                }
            }
        }
    }

    public TileFunctions TileSetFlags(TileFunctions t, TileFunctions dequeuedTile, int distance, bool attackable = false, bool selectable = false, bool showAttackableTiles = false)
    {
        TileFunctions tile = t;
        tile.selectable = selectable;
        tile.attackable = attackable;
        tile.colorAttackable = showAttackableTiles;
        tile.visited = true;
        tile.parent = dequeuedTile;
        tile.distance = distance;

        return tile;
    }

    public void FindAttackAbleTiles()
    {
        BFSTileMap(attackRange, false, false, true);
    }

    public void FindSelectableTiles()
    {
        BFSTileMap(move, false, true);
    }


    public void MoveToTile(TileFunctions tile)
    {
        path.Clear();
        tile.target = true;
        moving = true;

        TileFunctions next = tile;
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
        foreach (TileFunctions tile in selectableTiles)
        {
            tile.Reset();
        }

        selectableTiles.Clear();
    }

    public void RemoveAttackableTiles()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }
        foreach (TileFunctions tile in attackableTiles)
        {
            tile.ResetAttackable();
        }

        attackableTiles.Clear();
    }

    //prevents InvalidOperationException error
    public IEnumerator ClearDetectedEnemies()
    {
        yield return new WaitForEndOfFrame();
        detectedEnemies.Clear();
    }

    public void ResetEnemyAddedTiles()
    {
        foreach (TileFunctions tile in detectedEnemies)
        {
            tile.enemyAdded = false;
        }
    }
}