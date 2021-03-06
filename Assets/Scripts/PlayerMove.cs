﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MovementController
{


    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Update()
    {
        if (turnManager.playerCharacterTurnCounter == turnManager.playerCharacterCount)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }

        if (!turn)
        {
            return;
        }
        if (Input.GetMouseButtonUp(1))
        {
            gameObject.transform.position = originalPosition;
            ToggleUnitMenu(false);
            turnManager.characterSelected = false;
        }
        if (!moving && !UnitMenuPresent)
        {
            FindSelectableTiles();
            CheckMouse();
        }
        else
        {
            Move();
        }
    }

    public void CheckMouse()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if (hit)
            {
                if (hit.collider.tag == "Tile")
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>();

                    if (clickedTile.selectable)
                    {
                        MoveToTile(clickedTile);
                    }
                }
            }
        }
    }
}
