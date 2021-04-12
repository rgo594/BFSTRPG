﻿using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[CreateAssetMenu(menuName = "My Assets/Create Action Tile")]
public class TilePlacer : Tile
{
    public Sprite actionColorSprite;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        Vector3 correctPosition = new Vector3(position.x + 1, position.y + 1);

        tileData.sprite = sprite;
        if (Event.current != null)
        {
            GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
            if (tiles.Any(tile => tile.transform.position == correctPosition))
            {
                return;
            }
            if (Event.current.isMouse)
            {
                GameObject mappedtiles = GameObject.Find("MappedTiles");

                if (mappedtiles == null)
                {
                    mappedtiles = new GameObject();
                    mappedtiles.name = "MappedTiles";
                }

                GameObject tileObject = new GameObject();


                GameObject actionColor = new GameObject();
                actionColor.transform.SetParent(tileObject.transform);
                tileObject.transform.GetChild(0).transform.position = new Vector3(0, 0, 0);

                tileObject.AddComponent<TileFunctions>().walkable = true;
                tileObject.transform.SetParent(mappedtiles.transform);
                tileObject.AddComponent<BoxCollider2D>();
                tileObject.transform.position = correctPosition;
                tileObject.tag = "Tile";
                tileObject.name = correctPosition.x + " " + correctPosition.y;
                //tileObject.AddComponent<SpriteRenderer>().sprite = sprite;
                actionColor.AddComponent<SpriteRenderer>().sprite = actionColorSprite;
                actionColor.GetComponent<SpriteRenderer>().sortingOrder = 1;
                actionColor.GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 0);
                actionColor.name = "Action Color";

            }
        }
    }


/*    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        Debug.Log(position);
    }*/
    //will create the tile object on runtime, worried for its scalability
    //tileData.gameObject = gameObject;

}


