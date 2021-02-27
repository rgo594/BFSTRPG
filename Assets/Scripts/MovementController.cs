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

    //public Tile actualTargetTile;

    protected void Init()
    {
        tiles = GameObject.FindGameObjectsWithTag("Tile");

        TurnManager.AddUnit(this);
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

    public void ComputeAdjacencyLists(Tile target)
    {
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

        while (process.Count > 0)
        {
            //remove and return the tile
            Tile dequeuedTile = process.Dequeue();

            selectableTiles.Add(dequeuedTile);
            dequeuedTile.selectable = true;

            if (dequeuedTile.distance < move)
            {
                //changes flags of tiles adjacent to the currentTile (the one the character is on), adds those tiles to the queue, then iterates through newly queued tiles adjacent tiles.
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
                transform.position = target;
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;

            TurnManager.EndTurn();
        }
    }

    void SetHorizontalVelocity()
    {
        velocity = heading * moveSpeed;
    }

    void CalculateHeading(Vector3 target)
    {
        heading = target - transform.position;
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

    public void BeginTurn()
    {
        turn = true;
    }
    public void EndTurn()
    {
        turn = false;
    }
}
