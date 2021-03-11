using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectController : MonoBehaviour
{
    public bool characterSelected = false;

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if(hit.collider.gameObject.tag == "Character" && characterSelected == false)
            {
                hit.collider.gameObject.GetComponent<PlayerMove>().turn = true;
                characterSelected = true;
            }
        }
    }

}
