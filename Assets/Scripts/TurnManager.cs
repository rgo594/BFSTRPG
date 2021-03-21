using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    //contains a key for each team and a value thats a list, which contains  each unit of that team. (ex. { "Enemy" : [ NPC, NPC(1) ] } )
    public static Dictionary<string, List<AiMove>> teamUnits = new Dictionary<string, List<AiMove>>();

    //Queue of each team (ex. ["Enemy", "Ally"] )
    public static Queue<string> teamPhaseOrder = new Queue<string>();

    //Queue of each unit in current team (ex. [ NPC, NPC(1) ] )
    public static Queue<AiMove> unitTurnOrder = new Queue<AiMove>();

    public int playerCharacterCount;
    public int playerCharacterTurnCounter = 0;

    //increases everytime an ai switches turn ex. Ally turn -> Enemy turn; aiTurnCounter = 1;
    public int aiPhaseCounter = 0;
    //When the aiTeamCount and aiTurnCounter match, the player turn starts
    public int aiTeamCount = 0;

    public GameObject currentCharacter = null;
    public bool characterSelected = false;

    public static bool attackStep = false;
    public static bool allowEnemyDetection = false;
    public GameObject unitMenu;

    public static bool playerTurn = true;

    private void Start()
    {
        unitMenu = GameObject.Find("UnitMenuController");
    }

    // Update is called once per frame
    void Update()
    {
        playerCharacterCount = FindObjectsOfType<PlayerMove>().Length;
        aiTeamCount = teamPhaseOrder.Count;
        if (!currentCharacter)
        {
            ToggleEndPhaseButton(true);
        }
        else
        {
            ToggleEndPhaseButton(false);
        }
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
            ToggleEndPhaseButton(false);
            StartNpcPhase();
        }
    }

    public void ToggleEndPhaseButton(bool active)
    {
        unitMenu.transform.GetChild(1).gameObject.SetActive(active);
    }

    public void SelectPlayerCharacter()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

        if (Input.GetMouseButtonUp(0) && hit.collider != null)
        {
            bool characterMoving = false;

            
            if (currentCharacter != null)
            {
                characterMoving = currentCharacter.GetComponent<PlayerMove>().moving;
            }

            //can't select a character while one is moving or if its not a player character
            if (hit.collider.gameObject.tag == "Player" && !characterMoving)
            {
                var clickedCharacter = hit.collider.gameObject.GetComponent<PlayerMove>();

                if (currentCharacter != null)
                {
                    //deselects currently selected character once you click on a different character
                    if (hit.collider.gameObject.name != currentCharacter.name)
                    {
                        InitDeselectCharacter(currentCharacter);
                    }
                }

                //if character isn't selected and hasn't hasn't performed an action yet, then start that units turn and set it to current character
                if (!clickedCharacter.turn && !clickedCharacter.actionCompleted)
                {
                    currentCharacter = clickedCharacter.gameObject;
                    InitStartTurn(clickedCharacter.gameObject);
                }
                else
                {
                    //show unit menu if you double click on a unit who hasn't performed an action yet
                    if(!clickedCharacter.actionCompleted)
                    {
                        clickedCharacter.ToggleUnitMenu(true);
                    }
                }
            }
            else if(currentCharacter != null && hit.collider.gameObject.GetComponent<Tile>() && !currentCharacter.GetComponent<Player>().moving)
            {
                if (hit.collider.gameObject.GetComponent<Tile>().selectable == false)
                {
                    InitDeselectCharacter(currentCharacter);
                    currentCharacter = null;
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
        //characterSelected = false;
    }

    public static void InitUnitTurnOrder()
    {
        //Grabs first teams unit list in teamUnits
        List<AiMove> teamList = teamUnits[teamPhaseOrder.Peek()];

        foreach (AiMove unit in teamList)
        {
            unitTurnOrder.Enqueue(unit);
        }

    }

    public static void AddNpcUnit(AiMove unit)
    {
        List<AiMove> list;

       ;
        if (!teamUnits.ContainsKey(unit.tag))
        {
            list = new List<AiMove>();

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
        playerTurn = false;
        if (unitTurnOrder.Count > 0)
        {
            unitTurnOrder.Peek().StartTurn();
        }
    }

    public static void EndNpcTurn()
    {
        AiMove unit = unitTurnOrder.Dequeue();
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
                playerTurn = true;
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
