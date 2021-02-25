using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool walkable = true;
    public bool current = false;
    public bool target = false;
    public bool selectable = false;

    public List<Tile> adjacencyList = new List<Tile>();

    //Needed BFS (Breadth First Search)
    public bool visited = false;
    public Tile parent = null;
    public int distance = 0;

    // Update is called once per frame
    void Update()
    {
        if (current)
        {
            GetComponent<Renderer>().material.color = Color.magenta;
        }
        else if (target)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else if (selectable)
        {
            GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
    public void Reset()
    {
        adjacencyList.Clear();

        walkable = true;
        current = false;
        target = false;
        selectable = false;

        visited = false;
        parent = null;
        distance = 0;

        //f = g = h = 0;
    }

    public void FindNeighbors(Tile target)
    {
        Reset();
        //float jumpHeight = 1f;

        CheckTile(new Vector2(0,1), target);
        CheckTile(new Vector2(0,-1), target);
        CheckTile(Vector3.right, target);
        CheckTile(Vector3.left, target);

    }

    public void CheckTile(Vector3 direction, Tile target)
    {
        Vector3 halfExtents = new Vector3(0.25f, 0.25f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position + direction, halfExtents, 1f);
        
        foreach (Collider2D item in colliders)
        {
            Tile tile = item.GetComponent<Tile>();
            if (tile != null && tile.walkable)
            {
                RaycastHit2D hit = Physics2D.Raycast(tile.transform.position, Vector2.up);
                if (hit || (tile == target))
                {
                    adjacencyList.Add(tile);
                }
            }
        }
    }
}
