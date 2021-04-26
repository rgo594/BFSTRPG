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
        selectedUnit.GetComponent<PlayerMove>().startFindTiles = true;

    }

    public void InitAttack()
    {
        TurnManager.attackStep = true;
    }

    public void InitEndPhase()
    {
        TurnManager turnManager = FindObjectOfType<TurnManager>();

        turnManager.playerCharacterTurnCounter = turnManager.playerCharacterCount;
    }

    public void SetUnit(GameObject unit)
    {
        selectedUnit = unit;
    }
}
