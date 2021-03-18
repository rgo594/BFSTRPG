using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UnitMenu : MonoBehaviour
{
    public GameObject selectedUnit;

    public void InitEndTurn()
    {
        selectedUnit.GetComponent<PlayerMove>().EndTurn();
        gameObject.SetActive(false);
    }

    public void InitAttack()
    {
        TurnManager.attackStep = true;
    }

    public void InitEndPhase()
    {
        FindObjectOfType<TurnManager>().playerCharacterTurnCounter = 2;
    }

    public void SetUnit(GameObject unit)
    {
        selectedUnit = unit;
    }
}
