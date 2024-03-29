﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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


    public static bool playerPhase = true;
    public static bool enemyPhase;

    private bool displayEnemyPhaseText = true;

    public static bool attacking = false;

    public bool noneClicked = false;

    public static bool EnemyRangePresent = false;

    //TODO deselected is a flag for clicking on an enemy unit so FindSelectableTiles is not triggered simultaneously between multiple units, might need further testing
    public bool deselected = true;

    public static bool pcActionCompleted = false;

    public bool preventClicking = false;

    public GameObject unitMenu;
    public GameObject levelResult;

    public static bool refresh = true;

    int activeSceneIndex;

    IEnumerator PlayNext()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneLoader.LoadNextScene();
    }

    private void Start()
    {
        //playerphase needs to be reset for if you lose or change level
        playerPhase = true;
        unitMenu = GameObject.Find("UnitMenuController");
        levelResult = GameObject.Find("LevelResultController");
        playerCharacterCount = FindObjectsOfType<PlayerMove>().Length;
        //GameObject phaseController = GameObject.Find("PhaseTextController");
        activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void Update()
    {
        playerCharacterCount = FindObjectsOfType<PlayerMove>().Length;
        if (playerCharacterCount == 0)
        {
            levelResult.transform.GetChild(1).gameObject.SetActive(true);
            PhaseTextAnimation.PhaseTextPresent = false;
            teamUnits["Enemy"].Clear();
            return;
        }
        else if (teamUnits["Enemy"].Count == 0 && SceneManager.sceneCountInBuildSettings - 1 == activeSceneIndex)
        {
            levelResult.transform.GetChild(2).gameObject.SetActive(true);
            PhaseTextAnimation.PhaseTextPresent = false;
            teamUnits["Enemy"].Clear();
            return;
        }
        else if (teamUnits["Enemy"].Count == 0)
        {
            levelResult.transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(PlayNext());
            return;
        }


        aiTeamCount = teamPhaseOrder.Count;
        if (!currentCharacter && playerPhase && displayEnemyPhaseText)
        {
            noneClicked = true;
            ToggleEndPhaseButton(true);
        }
        else
        {
            noneClicked = false;
            ToggleEndPhaseButton(false);
        }
        //prevents being able to select characters during enemy turn
        if(playerCharacterTurnCounter != playerCharacterCount)
        {
            if (PhaseTextAnimation.PhaseTextPresent) { return; }
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
                enemyPhase = true;
                if (displayEnemyPhaseText)
                {
                    PhaseTextAnimation.PhaseTextPresent = true;
                    GameObject.Find("PhaseTextController").transform.GetChild(1).GetComponent<Animator>().SetTrigger("EnemyPhase");
                    displayEnemyPhaseText = false;
                }

                if(PhaseTextAnimation.PhaseTextPresent) { return; }
                ToggleEndPhaseButton(false);
                StartNpcTurn();
            }
    }

    public void ToggleEndPhaseButton(bool active)
    {
        unitMenu.transform.GetChild(1).gameObject.SetActive(active);
    }    
    
    public void ToggleClearedScreen(bool active)
    {
        levelResult.transform.GetChild(0).gameObject.SetActive(active);
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
            else if (currentCharacter != null && !currentCharacter.GetComponent<Player>().moving)
            {
                //not sure if this initial if is necessary
                //if (hit.collider.gameObject.GetComponent<Enemy>() || hit.collider.gameObject.GetComponent<Tile>())
                {
                    if (hit.collider.gameObject.GetComponent<TileFunctions>())
                    {
                        if (hit.collider.gameObject.GetComponent<TileFunctions>().selectable) { return; }
                    }

                    //InitDeselectCharacter(currentCharacter);
                    if (hit.collider.gameObject.GetComponent<Enemy>())
                    {
                        //functions for clicking on an enemy are handled in Enemy and AiMove files
                        //TODO temporary fix, shouldn't  need to refresh player tile map
                        currentCharacter.GetComponent<Player>().startFindTiles = true;
                    }
                    else
                    {
                        InitDeselectCharacter(currentCharacter);
                        currentCharacter = null;
                    }

                }
            }
        }
    }

    public void InitStartTurn(GameObject c)
    {
        deselected = false;
        var character = c.GetComponent<PlayerMove>();

        character.StartTurn();
    }
   
    public void InitDeselectCharacter(GameObject c)
    {
        deselected = true;
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

    public static void StartNpcTurn()
    {
        playerPhase = false;
        if (unitTurnOrder.Count > 0)
        {
            unitTurnOrder.Peek().StartTurn();
        }
    }

    public static void AiTurnRotation()
    {
        GameObject phaseController = GameObject.Find("PhaseTextController");
        unitTurnOrder.Dequeue();

        if (unitTurnOrder.Count > 0)
        {
            StartNpcTurn();
        }
        else
        {
            var turnManager = FindObjectOfType<TurnManager>();

            turnManager.aiPhaseCounter++;

            //switches to player phase
            if(turnManager.aiPhaseCounter == turnManager.aiTeamCount && turnManager.playerCharacterCount != 0)
            {
                turnManager.displayEnemyPhaseText = true;
                enemyPhase = false;

                PhaseTextAnimation.PhaseTextPresent = true;
                phaseController.transform.GetChild(0).GetComponent<Animator>().SetTrigger("PlayerPhase");

                playerPhase = true;
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
