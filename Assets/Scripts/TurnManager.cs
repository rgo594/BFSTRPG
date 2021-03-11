using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{

    public static Dictionary<string, List<MovementController>> units = new Dictionary<string, List<MovementController>>();
    public static Queue<string> turnKey = new Queue<string>();
    static Queue<MovementController> turnTeam = new Queue<MovementController>();
    int playerTeamCount;

    public bool characterSelected = false;
    public bool teamTurn = true;
    public int characterTurnCounter = 0;

    private void Start()
    {
        playerTeamCount = FindObjectsOfType<PlayerMove>().Length;
    }

    // Update is called once per frame
    void Update()
    {
        SelectPlayerCharacter();
        //only gets called once
        if (turnTeam.Count == 0)
        {
            InitTeamTurnQueue();
        }
        
        if (characterTurnCounter == playerTeamCount)
        {
            Debug.Log(characterTurnCounter == playerTeamCount);
            StartTurn();
        }

    }

    static void InitTeamTurnQueue()
    {
        List<MovementController> teamList = units[turnKey.Peek()];

        foreach (MovementController unit in teamList)
        {
            turnTeam.Enqueue(unit);
        }

    }

    public void SelectPlayerCharacter()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if (hit.collider.gameObject.tag == "Character" && characterSelected == false)
            {
                hit.collider.gameObject.GetComponent<PlayerMove>().turn = true;
                characterSelected = true;
            }
        }
    }

    public void EndCharacterTurn(GameObject character)
    {
        character.GetComponent<MovementController>().turn = false;
        characterSelected = false;
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
            var turnManager = FindObjectOfType<TurnManager>();
            turnManager.characterTurnCounter = 0;
            string team = turnKey.Dequeue();
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }
}
