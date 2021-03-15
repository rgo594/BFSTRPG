using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitMenu : MonoBehaviour
{
    public GameObject selectedUnit;

    public void InitEndTurn()
    {
        selectedUnit.GetComponent<PlayerMove>().EndPlayerCharacterTurn();
        gameObject.SetActive(false);
    }

    public void InitAttack()
    {
        TurnManager.attackStep = true;
        selectedUnit.GetComponent<PlayerMove>().attackAction = true;
    }

    public void SetUnit(GameObject unit)
    {
        selectedUnit = unit;
    }
}
