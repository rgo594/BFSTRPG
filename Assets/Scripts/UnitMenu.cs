using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitMenu : MonoBehaviour
{
    Color originalColor;
    public GameObject selectedUnit;

    public void EndTurn()
    {
        selectedUnit.GetComponent<PlayerMove>().EndPlayerCharacterTurn();
        gameObject.SetActive(false);
    }

    public void SetUnit(GameObject unit)
    {
        selectedUnit = unit;
    }

}
