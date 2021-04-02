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
    List<Tile> selectableAttackTiles = new List<Tile>();

    GameObject[] tiles;

    public Tile actualTargetTile;
    public bool enemyDetected;
    public bool ree = false;

    public bool showRange = false;
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
/*            if(attackable)
            {*/
                startTile.FindNeighbors(target, attackable);
            //}
/*            else
            {
                startTile.FindBoth(target);
            }*/
        }
    }

    public void BFSTileMap(int range, bool detectable = false, bool selectable = false, bool attackable = false)
    {
        if (detectable && enemyDetected) { return; }

        ComputeAdjacencyLists(null, attackable);

        bool coo = false;
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);

        currentTile.visited = true;

        while (process.Count > 0)
        {
            //remove and return the tile
            Tile dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);

            foreach (Tile tile in dequeuedTile.adjacencyList)
            {
                if (tile.occupied && dequeuedTile.distance == move - 1)
                {
                    //Debug.Log(dequeuedTile);
                    tile.distance = -1;
                    tile.attackable = true;
                    tile.showAttackableTiles = true;
                }
            }

            if (dequeuedTile.distance < range && !coo && !dequeuedTile.borderTile)
                {
                //if unit is on border tile need to check both of its adjacency lists
                foreach (Tile tile in dequeuedTile.adjacencyList)
                {

                    if (tile.detectedEnemy != null && tile.detectedEnemy.tag != gameObject.tag && !tile.enemyAdded && !tile.visited)
                    {
                        if (detectable)
                        {
                            enemyDetected = true;
                            tile.showAttackableTiles = true;
                        }
                        if (attackable)
                        {
                            tile.enemyAdded = true;
                            detectedEnemies.Add(tile);
                        }
                    }
                    //dequeuedTile.selectable = true; (just in case having selectable be in the foreach messes things up)
                    if (!tile.visited)
                    {
                        if(dequeuedTile.distance == move - 1)
                        {
                            if(tile.occupied)
                            {
                                //TODO needs to make sure that the occupied tile is on the outside of the selectable tile map
                                dequeuedTile.borderTile = true;
                            }
                        }
                        if (!tile.occupied || attackable)
                        {
                            Tile ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, attackable, selectable);
                            if(ModifiedTile.distance == move)
                            {
                                ModifiedTile.borderTile = true;
                            }
                            process.Enqueue(ModifiedTile);
                        }
                    }
                }
            }
        }
        if(process.Count == 0 && selectable)
        {
            ShowAttackRange();
        }
    }

    public void ShowAttackRange()
    {
        Tile[] tiles = FindObjectsOfType<Tile>();
        Queue<Tile> borderTiles = new Queue<Tile>();

        foreach (Tile tile in tiles)
        {
            if (tile.borderTile == true)
            {
                borderTiles.Enqueue(tile);
            }
        }

        while (borderTiles.Count > 0)
        {
            Tile dequeuedTile = borderTiles.Dequeue();
            selectableTiles.Add(dequeuedTile);

            foreach (Tile tile in dequeuedTile.adjacencyList)
            {
                if(!tile.visited)
                {
                    if(dequeuedTile.borderTile || dequeuedTile.distance < attackRange)
                    {
                        if(dequeuedTile.borderTile == true)
                        {
                            Tile ModifiedTile = TileSetFlags(tile, dequeuedTile, 1, true, false, true);
                            borderTiles.Enqueue(ModifiedTile);
                        }
                        else
                        {
                            Tile ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, true, false, true);
                            borderTiles.Enqueue(ModifiedTile);
                        }
                    }
                }
            }
        }
    }

    Tile TileSetFlags(Tile t, Tile dequeuedTile, int distance, bool attackable = false, bool selectable = false, bool showAttackableTiles = false)
    {
        Tile tile = t;
        tile.selectable = selectable;
        tile.attackable = attackable;
        tile.showAttackableTiles = showAttackableTiles;
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

/*        foreach (Tile tile in selectableAttackTiles)
        {
            tile.Reset();
        }*/

        selectableTiles.Clear();
        //selectableAttackTiles.Clear();
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