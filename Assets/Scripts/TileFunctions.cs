using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFunctions : MonoBehaviour
{
    public bool walkable = true;
    public bool current = false;
    public bool target = false;
    public bool selectable = false;
    public bool moving = false;
    public bool attackable = false;
    public bool enemyAdded = false;
    public bool showAttackableTiles = false;
    public bool occupied = false;
    public bool borderTile = false;

    //public bool playerTurn = false;

    //public bool attackVisited = false;

    public Collider2D detectedEnemy = null;

    public List<TileFunctions> adjacencyList = new List<TileFunctions>();
    public List<TileFunctions> unblockableAdjacencyList = new List<TileFunctions>();

    //Needed BFS (Breadth First Search)
    public bool visited = false;
    public TileFunctions parent = null;
    public int distance = 0;

    //For A*
    //g = cost from the parent to the current tile
    public float g = 0;
    //h = the cost from the processed tile to the destination
    public float h = 0;
    //f = g+h (used for finding the best case path with the minimal amount of time
    public float f = 0;

    SpriteRenderer ActionColor;

    // Update is called once per frame
    private void Start()
    {
        ActionColor = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        DetectEnemy();
        if (TurnManager.playerTurn)
        {
            if (current)
            {
                selectable = false;
               //GetComponent<SpriteRenderer>().color = Color.yellow;
                ActionColor.color = new Color32(255, 235, 4, 100);
            }
            else if (target)
            {
                //GetComponent<SpriteRenderer>().color = Color.green;
                ActionColor.color = Color.green;
            }
            else if (selectable)
            {
                //GetComponent<SpriteRenderer>().color = new Color32(53, 64, 241, 120);
                ActionColor.color = new Color32(0, 0, 170, 180);
                //Color.blue
            }
            else if (attackable)
            {
                //GetComponent<SpriteRenderer>().color = Color.white;
                ActionColor.color = new Color32(0, 0, 0, 0);

                if (TurnManager.attackStep || showAttackableTiles)
                {
                    //GetComponent<SpriteRenderer>().color = Color.red;
                    ActionColor.color = new Color32(200, 0, 0, 180);
                }
                else
                {
                    //GetComponent<SpriteRenderer>().color = Color.white;
                    ActionColor.color = new Color32(0, 0, 0, 0);
                }
            }
            else
            {
                gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
                //GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
    }

    public void Reset()
    {
        //gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
        adjacencyList.Clear();
        unblockableAdjacencyList.Clear();

        walkable = true;
        current = false;
        target = false;
        selectable = false;
        attackable = false;
        showAttackableTiles = false;

        borderTile = false;
        occupied = false;
        visited = false;
        parent = null;
        distance = 0;

        f = g = h = 0;
    }

    public void FindNeighbors(TileFunctions target, bool attackable)
    {
        Reset();

        CheckTile(new Vector2(0, 1), target, attackable);
        CheckTile(new Vector2(0, -1), target, attackable);
        CheckTile(Vector3.right, target, attackable);
        CheckTile(Vector3.left, target, attackable);
    }

    public void FindBoth(TileFunctions target)
    {
        Reset();

        CheckTile(new Vector2(0, 1), target, false);
        CheckTile(new Vector2(0, -1), target, false);
        CheckTile(Vector3.right, target, false);
        CheckTile(Vector3.left, target, false);

        CheckTile(new Vector2(0, 1), target, true);
        CheckTile(new Vector2(0, -1), target, true);
        CheckTile(Vector3.right, target, true);
        CheckTile(Vector3.left, target, true);
    }

    public void DetectEnemy()
    {
        Collider2D DetectedObject = Physics2D.OverlapBox(transform.position, new Vector2(0.8f, 0.8f), 1f);

        if (DetectedObject.gameObject.tag != "Tile")
        {
            detectedEnemy = DetectedObject;
        }
        else
        {
            detectedEnemy = null;
        }
    }

    public void CheckTile(Vector3 direction, TileFunctions target, bool attackable)
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f);

        //creates a box to detect any objects with colliders, not sure what angle parameter does
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + direction, halfExtents, 1f);
        
        foreach (Collider2D item in colliders)
        {
            TileFunctions tile = item.GetComponent<TileFunctions>();

/*            if(tile != null && attackable)
            {
                unblockableAdjacencyList.Add(tile);
            }*/
            if (tile != null && tile.walkable)
            {
                RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, new Vector3(0, 0, -1), 1);

                if (hit.collider.tag == "Tile" || (tile == target))
                {
                    adjacencyList.Add(tile);
                }
                else
                {
                    tile.occupied = true;
                    adjacencyList.Add(tile);
                }
            }
        }
    }

}
