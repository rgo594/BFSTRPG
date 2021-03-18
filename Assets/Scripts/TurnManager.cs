using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //contains a key for each team and a value thats a list, which contains  each unit of that team. (ex. { "Enemy" : [ NPC, NPC(1) ] } )
    public static Dictionary<string, List<MovementController>> teamUnits = new Dictionary<string, List<MovementController>>();

    //Queue of each team (ex. ["Enemy", "Ally"] )
    public static Queue<string> teamPhaseOrder = new Queue<string>();

    //Queue of each unit in current team (ex. [ NPC, NPC(1) ] )
    static Queue<MovementController> unitTurnOrder = new Queue<MovementController>();

    public int playerCharacterCount;
    public int playerCharacterTurnCounter = 0;

    //increases everytime an ai switches turn ex. Ally turn -> Enemy turn; aiTurnCounter = 1;
    public int aiPhaseCounter = 0;
    //When the aiTeamCount and aiTurnCounter match, the player turn starts
    public int aiTeamCount = 0;

    GameObject currentCharacter = null;
    public bool characterSelected = false;

    public static bool attackStep = false;
    public static bool allowEnemyDetection = false;

    private void Start()
    {
        playerCharacterCount = FindObjectsOfType<PlayerMove>().Length;
        aiTeamCount = teamPhaseOrder.Count;
    }

    // Update is called once per frame
    void Update()
    {
        //prevents being able to select characters during enemy turn
        if(playerCharacterTurnCounter != playerCharacterCount)
        {
            if (!characterSelected)
            {
                SelectPlayerCharacter();
            }
        }

        if (unitTurnOrder.Count == 0)
        {
            InitUnitTurnOrder();
        }

        //player phase ends when playerCharacterTurnCounter matches playerCharacterCount
        if (playerCharacterTurnCounter == playerCharacterCount)
        {
            StartNpcPhase();
        }
    }

    public void SelectPlayerCharacter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

        if (Input.GetMouseButtonUp(0) && hit.collider != null)
        {
            bool characterMoving = false;

            //so you can't select a character while another one is already moving
            if (currentCharacter != null)
            {
                characterMoving = currentCharacter.GetComponent<PlayerMove>().moving;
            }

            if (hit.collider.gameObject.tag == "Player" && !characterMoving)
            {
                var player = hit.collider.gameObject.GetComponent<PlayerMove>();

                if (currentCharacter != null)
                {
                    //prevents being able to click multiple units at the same time
                    if (hit.collider.gameObject.name != currentCharacter.name)
                    {
                        InitDeselectCharacter(currentCharacter);
                    }
                }

                if (!player.turn && !player.actionCompleted)
                {
                    currentCharacter = player.gameObject;
                    InitStartTurn(player.gameObject);
                }
                else
                {
                    if(!player.actionCompleted)
                    {
                        player.ToggleUnitMenu(true);
                    }
                }
            }
        }
    }

    public void InitStartTurn(GameObject c)
    {
        var character = c.GetComponent<PlayerMove>();

        character.StartTurn();
    }
   
    public void InitDeselectCharacter(GameObject c)
    {
        var character = c.GetComponent<PlayerMove>();

        character.DeselectCharacter();
        characterSelected = false;
    }

    static void InitUnitTurnOrder()
    {
        //Grabs first teams unit list in teamUnits
        List<MovementController> teamList = teamUnits[teamPhaseOrder.Peek()];

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

            if (!teamPhaseOrder.Contains(unit.tag))
            {
                //adds team to phase order
                teamPhaseOrder.Enqueue(unit.tag);
            }
        }
        else
        {
            //makes the list variable equal the value of a team key (which is a tag like "Enemy") in the teamUnits dictionary. ex. teamUnits = {"Enemy" : [] } list = [] <-- same list as the value of "Enemy"
            list = teamUnits[unit.tag];
        }

        //adds unit to teams unit list {"Enemy" : [ NPC ] }
        list.Add(unit);

    }

    public static void StartNpcPhase()
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
        unit.detectedEnemies.Clear();

        if (unitTurnOrder.Count > 0)
        {
            StartNpcPhase();
        }
        else
        {
            var turnManager = FindObjectOfType<TurnManager>();

            turnManager.aiPhaseCounter++;

            //switches to player phase
            if(turnManager.aiPhaseCounter == turnManager.aiTeamCount)
            {
                turnManager.playerCharacterTurnCounter = 0;
                turnManager.aiPhaseCounter = 0;
            }

            //responsible for resetting ai phase
            string team = teamPhaseOrder.Dequeue();
            teamPhaseOrder.Enqueue(team);
            InitUnitTurnOrder();
        }
    }
}
