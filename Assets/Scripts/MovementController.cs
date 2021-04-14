﻿using System.Collections;
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
    //public List<GameObject> detectedEnemies = new List<GameObject>();
    public List<TileFunctions> detectedEnemies = new List<TileFunctions>();

    public Stack<TileFunctions> path = new Stack<TileFunctions>();

    public Vector3 velocity = new Vector3();
    public Vector3 heading = new Vector3();

    public TurnManager turnManager;

    public List<TileFunctions> selectableTiles = new List<TileFunctions>();
    List<TileFunctions> selectableAttackTiles = new List<TileFunctions>();

    GameObject[] tiles;

    public TileFunctions actualTargetTile;
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
                //Debug.Log(startTile);
                startTile.FindNeighbors(target, attackable);
            }
        }
      /*foreach (GameObject tile in tiles)
        {
            TileFunctions startTile = tile.GetComponent<TileFunctions>();
            startTile.FindNeighbors(target, attackable);
        }*/
    }

    public void BFSTileMap(int range, bool detectable = false, bool selectable = false, bool attackable = false, bool enemyRangeTile = false, bool counterPresent = false)
    {
        //Debug.Log(detectable);
        if (detectable && enemyDetected) { return; }

        ComputeAdjacencyLists(null, attackable);

        bool coo = false;
        GetCurrentTile();

        Queue<TileFunctions> process = new Queue<TileFunctions>();

        process.Enqueue(currentTile);

        currentTile.visited = true;

        while (process.Count > 0)
        {
            //remove and return the tile
            TileFunctions dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);

/*            foreach (TileFunctions tile in dequeuedTile.adjacencyList)
            {
                if (tile.occupied && dequeuedTile.distance == move - 1)
                {
                    //Debug.Log(dequeuedTile);
                    tile.distance = -1;
                    tile.attackable = true;
                    tile.showAttackableTiles = true;
                }
            }*/

            if (dequeuedTile.distance < range && !coo && !dequeuedTile.borderTile)
                {
                //if unit is on border tile need to check both of its adjacency lists
                foreach (TileFunctions tile in dequeuedTile.adjacencyList)
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
                        if (counterPresent)
                        {
                            tile.counter++;
                        }
                        if (tile.occupied)
                        {
                            dequeuedTile.borderTile = true;
                        }
                        if (!tile.occupied || attackable)
                        {
                            TileFunctions ModifiedTile;


                            ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, attackable, selectable, false, enemyRangeTile);
                            //TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, attackable, selectable);
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
            ShowAttackRange(detectable, enemyRangeTile, counterPresent);
        }
    }

    public void ShowAttackRange(bool detectable, bool enemyRangeTile, bool counterPresent)
    {
/*        float rng = ((float)move + (float)attackRange) * 2.4f;
        Collider2D[] tilesInRange = Physics2D.OverlapBoxAll(gameObject.transform.position, new Vector2(rng, rng), 1f);*/

        TileFunctions[] tiles = FindObjectsOfType<TileFunctions>();
        Queue<TileFunctions> borderTiles = new Queue<TileFunctions>();

        foreach (TileFunctions tile in tiles)
        {
            if (tile.borderTile == true)
            {
                borderTiles.Enqueue(tile);
            }
        }

        while (borderTiles.Count > 0)
        {
            TileFunctions dequeuedTile = borderTiles.Dequeue();
            selectableTiles.Add(dequeuedTile);

            foreach (TileFunctions tile in dequeuedTile.adjacencyList)
            {
                if (tile.detectedEnemy && detectable)
                {
                    enemyDetected = true;
                    tile.showAttackableTiles = true;
                }
                if (!tile.visited)
                {
                    if (counterPresent)
                    {
                        tile.counter++;
                    }
                    if(dequeuedTile.borderTile || dequeuedTile.distance < attackRange)
                    {
                        //TODO needs to be refactored
                        if(enemyRangeTile)
                        {
                            if (dequeuedTile.borderTile == true)
                            {
                                TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1, false, false, false, true);
                                borderTiles.Enqueue(ModifiedTile);
                            }
                            else
                            {
                                TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, false, false, false, true);
                                borderTiles.Enqueue(ModifiedTile);
                            }
                        }
                        //
                        else
                        {
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
    }


    public void EnemyRangeMap(int range)
    {
        ComputeAdjacencyLists(null, false);

        bool coo = false;
        GetCurrentTile();

        Queue<TileFunctions> process = new Queue<TileFunctions>();

        process.Enqueue(currentTile);

        currentTile.erVisited = true;

        while (process.Count > 0)
        {
            //remove and return the tile
            TileFunctions dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);

            if (dequeuedTile.distance < range && !coo && !dequeuedTile.borderTile)
            {
                //if unit is on border tile need to check both of its adjacency lists
                foreach (TileFunctions tile in dequeuedTile.adjacencyList)
                {
                    //dequeuedTile.selectable = true; (just in case having selectable be in the foreach messes things up)
                    if (!tile.erVisited)
                    {
                        if (tile.occupied)
                        {
                            dequeuedTile.borderTile = true;
                        }
                        if (!tile.occupied)
                        {
                            //TileFunctions ModifiedTile;


                            //ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, false, false, false, true, false);
                            tile.enemyRange = true;
                            tile.erVisited = true;
                            tile.distance = 1 + dequeuedTile.distance;
                            tile.enemiesUsingTile.Add(gameObject);


                            //TileFunctions ModifiedTile = TileSetFlags(tile, dequeuedTile, 1 + dequeuedTile.distance, attackable, selectable);
/*                            if (tile.distance == move)
                            {
                                tile.borderTile = true;
                            }*/
                            process.Enqueue(tile);
                        }
                    }
                }
            }
        }
        if (process.Count == 0)
        {
            //ShowAttackRange(detectable, enemyRangeTile, counterPresent);
        }
    }

    TileFunctions TileSetFlags(TileFunctions t, TileFunctions dequeuedTile, int distance, bool attackable = false, bool selectable = false, bool showAttackableTiles = false, bool enemyRange = false, bool visited = true)
    {
        TileFunctions tile = t;
        tile.selectable = selectable;
        tile.attackable = attackable;
        tile.showAttackableTiles = showAttackableTiles;
        tile.visited = visited;
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

    public void FindEnemyRangeTiles()
    {
        EnemyRangeMap(move);
        //BFSTileMap(move, false, false, false, true, true);
    }


    public void FindEnemiesInRange()
    {
        //BFSTileMap(move + attackRange, true);
        BFSTileMap(move, true);
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

/*        foreach (Tile tile in selectableAttackTiles)
        {
            tile.Reset();
        }*/

        selectableTiles.Clear();
    }

    public void TestRemoveSelect()
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

        /*        foreach (Tile tile in selectableAttackTiles)
                {
                    tile.Reset();
                }*/

        //selectableTiles.Clear();
    }

    public void WoogaBooga()
    {
        if (currentTile != null)
        {
            currentTile.current = false;
            currentTile = null;
        }

        foreach (TileFunctions tile in selectableTiles)
        {
            //Debug.Log(tile);
            tile.HideEnemyRange();
            tile.enemiesUsingTile.Remove(gameObject);
        }

        /*        foreach (Tile tile in selectableAttackTiles)
                {
                    tile.Reset();
                }*/
        selectableTiles.Clear();
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

//test commit