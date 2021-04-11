using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "My Assets/Create Action Tile")]
public class TilePlacer : Tile
{
    //public Sprite spriteA;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.gameObject = gameObject;
        tileData.sprite = sprite;

        //tileData.gameObject.GetComponent<SpriteRenderer>().sprite = tileData.sprite;
        //gameObject.GetComponent<SpriteRenderer>().sprite = spriteA;
        //tileData.sprite = spriteA;
    }

}


