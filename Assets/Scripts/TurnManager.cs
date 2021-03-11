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

        //only gets called once
        if (turnTeam.Count == 0)
        {
            InitTeamTurnQueue();
        }
        
        if (characterTurnCounter == playerTeamCount)
        {
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

            //GameObject previousCharacter = null;

            
            if (hit.collider.gameObject.tag == "Character")
            {
                var player = hit.collider.gameObject.GetComponent<PlayerMove>();

                if(currentCharacter != null)
                {
                    if (hit.collider.gameObject.name != currentCharacter.name)
                    {
                        currentCharacter.GetComponent<PlayerMove>().selected = false;
                        currentCharacter.GetComponent<PlayerMove>().turn = false;
                        currentCharacter.GetComponent<PlayerMove>().RemoveSelectableTiles();
                        characterSelected = false;
                    }
                }


                if (player.turn == false)
                {
                    currentCharacter = hit.collider.gameObject;
                    player.turn = true;
                    player.selected = true;
                    characterSelected = true;
                }
                else
                {
                    player.selected = false;
                    player.turn = false;
                    player.RemoveSelectableTiles();
                    characterSelected = false;
                }
                Debug.Log(currentCharacter);
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

            //resets player turn
            //if there are three teams wont work properly
            turnManager.characterTurnCounter = 0;

            //responsible for resetting ai
            string team = turnKey.Dequeue();
            turnKey.Enqueue(team);
            InitTeamTurnQueue();
        }
    }
}
