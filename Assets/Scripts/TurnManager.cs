using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    static Dictionary<string, List<MovementController>> units = new Dictionary<string, List<MovementController>>();
    static Queue<string> turnKey = new Queue<string>();
    static Queue<MovementController> turnTeam = new Queue<MovementController>();

    // Update is called once per frame
    void Update()
    {
        if (turnTeam.Count == 0)
        {
            InitTeamTurnQueue();
        }
        StartTurn();
    }

    static void InitTeamTurnQueue()
    {
        List<MovementController> teamList = units[turnKey.Peek()];

        foreach (MovementController unit in teamList)
        {
            turnTeam.Enqueue(unit);
        }

    }

    public static void AddUnit(MovementController unit)
    {
        List<MovementController> list;

        if (!units.ContainsKey(unit.tag))
        {
            list = new List<MovementController>();
            units[unit.tag] = list;

            if (!turnKey.Contains(unit.tag))
            {
                turnKey.Enqueue(unit.tag);
            }
        }
        else
        {
            list = units[unit.tag];
        }

        list.Add(unit);
    }

    public static void StartTurn()
    {
        if (turnTeam.Count > 0)
        {
            turnTeam.Peek().BeginTurn();
        }
    }

    public static void EndTurn()
    {
        MovementController unit = turnTeam.Dequeue();
        unit.EndTurn();

        if (turnTeam.Count > 0)
        {
            StartTurn();
        }
        else
        {
            string team = turnKey.Dequeue();
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }
}
