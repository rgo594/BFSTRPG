using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public bool turn = false;

    List<Tile> selectableTiles = new List<Tile>();
    GameObject[] tiles;

    Stack<Tile> path = new Stack<Tile>();
    Tile currentTile;

    public bool moving = false;
    public int move = 5;
    public float moveSpeed = 5;

    Vector3 velocity = new Vector3();
    Vector3 heading = new Vector3();

    public Tile actualTargetTile;

    protected void init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");
    }

    public void GetCurrentTile()
    {
        currentTile = GetTargetTile(gameObject);
        currentTile.current = true;
        //currentTile.selectable = false;
    }

    public Tile GetTargetTile(GameObject target)
    {
        Vector3 playerTilePos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z + 1);

        RaycastHit2D hit = Physics2D.Raycast(playerTilePos, new Vector3(0, 0, 1), 1f, 9);

        Tile tile = null;

        if (hit.collider != null)
        {
            tile = hit.collider.GetComponent<Tile>();
            
        }

        return tile;
    }

    public void ComputeAdjacencyLists(Tile target)
    {
        //tiles = GameObject.FindGameObjectsWithTag("Tile");

        foreach (GameObject tile in tiles)
        {
            Tile startTile = tile.GetComponent<Tile>();
            startTile.FindNeighbors(target);
        }
    }

    public void FindSelectableTiles()
    {
        ComputeAdjacencyLists(null);
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();

        process.Enqueue(currentTile);
        currentTile.visited = true;
        //currentTile.parent = ??  leave as null 


        while (process.Count > 0)
        {
            Tile dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);
            dequeuedTile.selectable = true;

            if (dequeuedTile.distance < move)
            {
                foreach (Tile tile in dequeuedTile.adjacencyList)
                {
                    if (!tile.visited)
                    {
                        tile.parent = dequeuedTile;
                        tile.visited = true;
                        tile.distance = 1 + dequeuedTile.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }
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

    public void Move()
    {
        if (path.Count > 0)
        {
            //returns obj at the top of the stack without removing;
            Tile t = path.Peek();
            Vector3 target = t.transform.position;

            //Calculate the units position on top of the target tile
            //target.y += halfHeight + t.GetComponent<Collider>().bounds.extents.y;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                //bool jump = transform.position.y != target.y;

                CalculateHeading(target);
                SetHorizontalVelocity();
           

                //Locomotion
                //transform.forward = heading;
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                transform.position = target;
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;

            //TurnManager.EndTurn();
        }
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    void CalculateHeading(Vector3 target)
    {
        heading = new Vector3(target.x - transform.position.x, target.y - transform.position.y, target.z - transform.position.z);
        heading.Normalize();
    }

    protected void RemoveSelectableTiles()
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
