using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitMenu : MonoBehaviour
{
    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = gameObject.GetComponent<TextMeshPro>().color;
    }

    public void OnMouseDown()
    {
        TurnManager.EndNpcTurn();
    }

    public void OnMouseOver()
    {
        //gameObject.GetComponent<MeshRenderer>().materials[0].color = Color.green;
        gameObject.GetComponent<TextMeshPro>().color = Color.green;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<TextMeshPro>().color = originalColor;
    }

}
