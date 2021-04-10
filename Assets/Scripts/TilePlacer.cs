using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "My Assets/COCKASDFA")]
public class TilePlacer : Tile
{


    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        tileData.gameObject = gameObject;
    }

}


