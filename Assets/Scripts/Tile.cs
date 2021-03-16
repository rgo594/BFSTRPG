using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool walkable = true;
    public bool current = false;
    public bool target = false;
    public bool selectable = false;
    public bool moving = false;
    public bool attackable = false;
    public bool enemyAdded = false;

    public Collider2D detectedEnemy = null;

    public List<Tile> adjacencyList = new List<Tile>();
    public List<Tile> attackAdjacencyList = new List<Tile>();

    //Needed BFS (Breadth First Search)
    public bool visited = false;
    public Tile parent = null;
    public int distance = 0;

    //For A*
    //g = cost from the parent to the current tile
    public float g = 0;
    //h = the cost from the processed tile to the destination
    public float h = 0;
    //f = g+h (used for finding the best case path with the minimal amount of time
    public float f = 0;

    // Update is called once per frame
    void Update()
    {
        if(PlayerMove.addDetectedEnemies)
        {
            enemyAdded = false;
        }
        if (current)
        {
            selectable = false;
            GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (target)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }
        else if (selectable)
        {
            GetComponent<SpriteRenderer>().color = new Color32(53, 64, 241, 120);
        }
        else if(attackable)
        {
            DetectEnemy();
            //GetComponent<SpriteRenderer>().color = Color.white;

            if (PlayerMove.attackStep)
            {
                GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    public void Reset()
    {
        adjacencyList.Clear();

        walkable = true;
        current = false;
        target = false;
        selectable = false;
        attackable = false;

        visited = false;
        parent = null;
        distance = 0;

        f = g = h = 0;
    }

    public void FindNeighbors(Tile target, bool attackable)
    {
        Reset();

        CheckTile(new Vector2(0,1), target, attackable);
        CheckTile(new Vector2(0,-1), target, attackable);
        CheckTile(Vector3.right, target, attackable);
        CheckTile(Vector3.left, target, attackable);
    }

    public void DetectEnemy()
    {
        detectedEnemy = Physics2D.OverlapBox(transform.position, new Vector2(0.8f,0.8f), 1f, 1024);
    }

    public void CheckTile(Vector3 direction, Tile target, bool attackable)
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f);

        //creates a box to detect any objects with colliders, not sure what angle parameter does
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + direction, halfExtents, 1f);
        
        foreach (Collider2D item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();

            if(tile != null && attackable)
            {
                //doesn't filter out objects
                adjacencyList.Add(tile);
            }
            else if (tile != null && tile.walkable)
            {
                RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, new Vector3(0, 0, -1), 1);

                //responsible for filtering out tiles blocked by obstacles and other characters
                if (hit.collider.tag == "Tile" || (tile == target))
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }
}
