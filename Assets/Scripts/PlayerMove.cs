using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerMove : MovementController
{
    public Vector3 originalPosition;
    public GameObject unitMenuController;

    public int healthPoints = 100;
    Slider healthBar;

    public int attack = 25;

    public bool unitMenuPresent = false;
    public bool actionCompleted = false;

    public GameObject targetedEnemy;

    public bool attacking = false;

    GameObject card;

    //public GameObject preventClicking;
    public bool startFindTiles = true;
    public bool attackTiles = true;

    Vector3[] startLevelLocations = new Vector3[] { new Vector3(7f, -10f), new Vector3(9f, -10f), new Vector3(7f, -12f), new Vector3(9f, -12f) };

    Player[] playerCharacters;


    private void OnMouseOver()
    {
        //card = gameObject.transform.GetChild(2).gameObject;
        if (!turnManager.currentCharacter)
        {
            card.transform.GetChild(0).gameObject.SetActive(true);
            TextMeshProUGUI cardText = card.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            cardText.text = "<3 : " + healthPoints + "\n \n A  : " + attack;
        }
    }

    private void OnMouseExit()
    {
        //card = gameObject.transform.GetChild(2).gameObject;
        card.transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Awake()
    {
        playerCharacters = FindObjectsOfType<Player>();
        //DontDestroyOnLoad(this.gameObject);
        card = gameObject.transform.GetChild(2).gameObject;
        healthBar = gameObject.transform.GetChild(1).GetComponentInChildren<Slider>();
        healthBar.maxValue = healthPoints;
        healthBar.value = healthPoints;
        unitMenuController = GameObject.Find("UnitMenuController");
    }

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        actionCompleted = false;
        SetCharacterColor(Color.white);
        Init();
        card = gameObject.transform.GetChild(2).gameObject;
        healthBar = gameObject.transform.GetChild(1).GetComponentInChildren<Slider>();
        healthBar.maxValue = healthPoints;
        healthBar.value = healthPoints;
        unitMenuController = GameObject.Find("UnitMenuController");

        if (scene.name == "Level 2")
        {
            for(int i = 0; i < playerCharacters.Length; i++)
            {
                playerCharacters[i].gameObject.transform.position = startLevelLocations[i];
            }
        }
    }

void Start()
    {
        Init();
        
    }

    public void Move()
    {
        if (path.Count > 0)
        {
            //returns obj at the top of the stack without removing;
            TileFunctions nextTileInPath = path.Peek();
            Vector3 target = nextTileInPath.transform.position;

            if (Vector3.Distance(transform.position, target) >= 0.05f)
            {
                //TODO find a way to travel to point without affecting z axis
                var targetPos = new Vector3(target.x, target.y, -1f);
                gameObject.transform.position = Vector2.MoveTowards(gameObject.transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                //temporary fix for z axis
                transform.position = new Vector3(target.x, target.y, -1f);
                path.Pop();
            }
        }
        else
        {
            RemoveSelectableTiles();
            moving = false;
            var unitMenu = unitMenuController.transform.GetChild(0).GetComponent<UnitMenu>();

            turnManager.characterSelected = true;
            if(!attacking)
            {
                ToggleUnitMenu(true);
            }


            unitMenu.SetUnit(gameObject);

        }
    }

    public void TargetTileToTravel()
    {
        if (Input.GetMouseButtonUp(0) && !actionCompleted)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);

            if (hit)
            {
                if (hit.collider.tag == "Tile")
                {
                    TileFunctions clickedTile = hit.collider.GetComponent<TileFunctions>();

                    if (clickedTile.selectable)
                    {
                        MoveToTile(clickedTile);
                    }
                }
            }
        }
    }

    public void ResetCharacterTurn()
    {
        attackTiles = true;
        ResetEnemyAddedTiles();
        TurnManager.attackStep = false;
        ToggleUnitMenu(false);
        turnManager.characterSelected = false;
        gameObject.transform.position = originalPosition;
        detectedEnemies.Clear();
    }

    public void StartTurn()
    {
        attackTiles = true;
        startFindTiles = true;
        TurnManager.pcActionCompleted = false;
        TurnManager.attackStep = false;
        TurnManager.refresh = true;
        turn = true;
        originalPosition = gameObject.transform.position;
    }



    public void EndTurn()
    {
        turnManager.currentCharacter = null;
        TurnManager.pcActionCompleted = true;
        currentTile = null;
        ResetEnemyAddedTiles();
        targetedEnemy = null;
        attacking = false;
        TurnManager.attackStep = false;
        unitMenuPresent = false;
        actionCompleted = true;
        startFindTiles = false;
        SetCharacterColor(Color.grey);
        turnManager.playerCharacterTurnCounter++;
        turnManager.InitDeselectCharacter(gameObject);

        StartCoroutine(ClearDetectedEnemies());
    }

    public void DeselectCharacter()
    {
        startFindTiles = true;
        turn = false;

        //not sure RemoveSelectableTiles needs to be here?
        RemoveSelectableTiles();
        RemoveAttackableTiles();
        turnManager.characterSelected = false;
        startFindTiles = true;
    }

    public IEnumerator AttackAction()
    {
        yield return new WaitUntil(() => TurnManager.attackStep == true);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, Vector2.up);


        foreach (TileFunctions enemyTile in detectedEnemies)
        {
            AiMove enemyMove = enemyTile.detectedEnemy.gameObject.GetComponent<AiMove>();

            if (Input.GetMouseButtonUp(0))
            {
               
                if (hit.collider.gameObject == enemyTile.detectedEnemy.gameObject)
                {
                    targetedEnemy = enemyTile.detectedEnemy.gameObject;

                    Vector3 enemyPos = targetedEnemy.transform.position;
                    Vector3 playerPos = gameObject.transform.position;

                    Animator animator = gameObject.GetComponent<Animator>();

                    attacking = true;
                    if (enemyPos.x > playerPos.x)
                    {
                        animator.SetTrigger("AttackRight");
                    }
                    else if(enemyPos.x < playerPos.x)
                    {
                        animator.SetTrigger("AttackLeft");
                    }
                    else if (enemyPos.y > playerPos.y)
                    {
                        animator.SetTrigger("AttackUp");
                    }
                    else if (enemyPos.y < playerPos.y)
                    {
                        animator.SetTrigger("AttackDown");
                    }
                    ToggleUnitMenu(false);
                }
            }
        }
    }

    //called in attack animation
    public void DamageStep()
    {
        targetedEnemy.transform.GetChild(1).GetComponentInChildren<Slider>().value -= attack;
        targetedEnemy.GetComponent<AiMove>().healthPoints -= attack;
    }

    public void ToggleUnitMenu(bool active)
    {
        var unitMenu = unitMenuController.transform.GetChild(0).gameObject;
        unitMenuPresent = active;
        unitMenu.SetActive(active);
    }

    public void ToggleAttackButton(bool active)
    {
        unitMenuController.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(active);
    }

    public void SetCharacterColor(Color color)
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().color = color;
    }

    public IEnumerator DelayFindSelectable()
    {
        yield return new WaitUntil(() => reset);
        yield return new WaitForFixedUpdate();
        FindSelectableTiles();
    }
}
