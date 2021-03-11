using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //contains a key for each team and a value thats a list, which contains  each unit of that team. (ex. { "Enemy" : [ NPC, NPC(1) ] } )
    public static Dictionary<string, List<MovementController>> teamUnits = new Dictionary<string, List<MovementController>>();

    //Queue of each team (ex. ["Enemy", "Ally"] )
    public static Queue<string> teamTurnOrder = new Queue<string>();

    //Queue of each unit in current team (ex. [ NPC, NPC(1) ] )
    static Queue<MovementController> unitTurnOrder = new Queue<MovementController>();

    int playerTeamCount;
    public int characterTurnCounter = 0;

    GameObject currentCharacter = null;

    private void Start()
    {
        playerTeamCount = FindObjectsOfType<PlayerMove>().Length;
    }

    // Update is called once per frame
    void Update()
    {
        //prevents being able to select characters during enemy turn
        if(characterTurnCounter != playerTeamCount)
        {
            SelectPlayerCharacter();
        }

        if (unitTurnOrder.Count == 0)
        {
            InitUnitTurnOrder();
        }
        
        if (characterTurnCounter == playerTeamCount)
        {
            StartNpcTurn();
        }

    }

    public void SelectPlayerCharacter()
    {

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if (hit.collider.gameObject.tag == "Character")
            {
                var player = hit.collider.gameObject;

                if (currentCharacter != null)
                {
                    //prevents being able to click multiple units at the same time
                    if (hit.collider.gameObject.name != currentCharacter.name)
                    {
                        EndPlayerCharacterTurn(currentCharacter);
                    }
                }

                if (player.GetComponent<PlayerMove>().turn == false)
                {
                    currentCharacter = player;
                    StartPlayerCharacterTurn(player);
                }
                else
                {
                    EndPlayerCharacterTurn(player);
                }
            }
        }
    }

    public void StartPlayerCharacterTurn(GameObject character)
    {
        character.GetComponent<PlayerMove>().turn = true;
    }

    public void EndPlayerCharacterTurn(GameObject character)
    {
        character.GetComponent<PlayerMove>().turn = false;
        character.GetComponent<PlayerMove>().RemoveSelectableTiles();
    }

    static void InitUnitTurnOrder()
    {
        //Grabs first teams unit list in teamUnits
        List<MovementController> teamList = teamUnits[teamTurnOrder.Peek()];

        foreach (MovementController unit in teamList)
        {
            unitTurnOrder.Enqueue(unit);
        }

    }

    public static void AddNpcUnit(MovementController unit)
    {
        List<MovementController> list;

       ;
        if (!teamUnits.ContainsKey(unit.tag))
        {
            list = new List<MovementController>();

            //add list value to key (currently will be null instead of empty list). ex. {"Enemy" : [] }
            teamUnits[unit.tag] = list;

            if (!teamTurnOrder.Contains(unit.tag))
            {
                //adds team to turn order
                teamTurnOrder.Enqueue(unit.tag);
            }
        }
        else
        {
            //makes the list variable equal the specific list value tied to the team key in the teamUnits dictionary. ex. unitTeams = {"Enemy" : [] } list = []
            list = teamUnits[unit.tag];
        }

        //adds unit to teams unit list {"Enemy" : [ NPC ] }
        list.Add(unit);

    }

    public static void StartNpcTurn()
    {
        if (unitTurnOrder.Count > 0)
        {
            unitTurnOrder.Peek().BeginTurn();
        }
    }

    public static void EndNpcTurn()
    {
        MovementController unit = unitTurnOrder.Dequeue();
        unit.EndTurn();

        if (unitTurnOrder.Count > 0)
        {
            StartNpcTurn();
        }
        else
        {
            var turnManager = FindObjectOfType<TurnManager>();

            //switches to players turn (if there are three teams wont work properly)
            turnManager.characterTurnCounter = 0;

            //responsible for resetting ai
            string team = teamTurnOrder.Dequeue();
            teamTurnOrder.Enqueue(team);
            InitUnitTurnOrder();
        }
    }
}
