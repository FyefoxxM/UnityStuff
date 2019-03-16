using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum State {Select, Placement, PlayerTurn, EnemyTurn } //Finite State Machine for turns
    public State state;
    public enum playerTurn {NotPlayerTurn = 0, Select, Movement, Attack, Damage }
    public playerTurn turnPhase;
    public static GameManager instance; //Global GM instance
    public Player player;
    public MapGenerator mapgen;
    public GameObject carriedUnit;
    public GameObject selectedUnit;
    public List<CustomUnit> deployedUnits = new List<CustomUnit>(); //player units deployedto the field. This will be updates as the player loses units.
    public GameObject placementCanvas;
    public Notifer notifier;
    public AudioSource audioSource;
    public MessagePanel messenger;
    public List<GameObject> waypoints = new List<GameObject>();
    public List<Node> nodeList = new List<Node>();
    public int movecount = 0;
    public int nodecount = 0;
    bool nodeinit = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        print("AudioSource Assigned");
        state = State.Select;
        notifier.FlashMessage("Selection Phase!");
        messenger.Message("Select a Unit to Place from the Buttons on the Left");
    }

    private void InitNodes()
    {
        Transform MapBase = GameObject.Find("MapBase").transform;
        foreach (Transform T in MapBase)
        {
            if (T.childCount != 0)
            {
                Node node = T.Find("Centre").GetComponent<Node>();
                if (node == null)
                {
                    //print("Node Not Found at location" + T.GetComponent<CustomTile>().mapLocation);
                }
                else
                {
                    node.assigninfo(nodecount, T.GetComponent<CustomTile>().mapLocation.x, T.GetComponent<CustomTile>().mapLocation.y);
                    //print("Node Added: " + node.id);
                    nodeList.Add(node);
                    nodecount++;
                }
            }
        }
        nodeinit = true;        
    }

    // Update is called once per frame
    void Update()
    {
        if(state == State.Select)
        {
            placementCanvas.GetComponent<CanvasFade>().FadeInCanvas();
            
        }else
            if (state == State.Placement)
        {
            placementCanvas.GetComponent<CanvasFade>().FadeOutCanvas();
        }

        if (!nodeinit){
            InitNodes();
        }
    }


    //Go through the game board and initialize each highlight as necessary
    //Mountain tiles have no highlight because they have no movement
    public void InitializeHighlights()
    {
        foreach(Transform t in GameObject.Find("MapBase").transform)
        {
            if (t.Find("Highlight"))
            {
                t.Find("Highlight").GetComponent<Highlight>().Init();
                t.Find("Highlight").GetComponent<Highlight>().TurnOffHighlight();
                if (t.GetComponent<CustomTile>())
                {
                    if (t.GetComponent<CustomTile>().mapLocation.x < 4)
                    {
                        if (t.GetComponent<CustomTile>().mapLocation.y < 4)
                        {                            
                            t.Find("Highlight").GetComponent<Highlight>().TurnOnHighlight(Color.blue);
                        }
                    }
                }
            }
        }

        
    }
    //read in which button on the deployment panel was pressed, copy the unit to the carried unit slot
    public void PlaceUnit(int count)
    {
        notifier.FlashMessage("Placement Phase!");
        instance.messenger.Message("Select a highlighted tile to place the unit on");
        GameObject tempunit = Instantiate(player.deployment[count].gameObject);
        tempunit.GetComponent<CustomUnit>().followMouse = true;
        carriedUnit = tempunit;
        placementCanvas.GetComponent<CanvasGroup>().alpha = 0;
        print("State = " + state.ToString());
    }

    //Changes state to the next actor's turn
    public void EndTurn()
    {
        if (state == State.EnemyTurn)
        {
            state = State.PlayerTurn;
        } else
            if (state == State.PlayerTurn)
        {
            state = State.EnemyTurn;
        }

        if(turnPhase == playerTurn.Movement)
        {

            MoveCalculate();
        }
    }

    private void MoveCalculate()
    {
        messenger.Message("Select a Tile to move to.");
    }

    //Determines whether the PC or the Player goes first
    public void DetermineFirstPlayer()
    {
        int q = UnityEngine.Random.Range(0, 20);
        if (q < 18)
        {
            state = State.PlayerTurn;
            turnPhase = playerTurn.Select;
            notifier.FlashMessage("Player Turn!");
        }
        else
        {
            state = State.EnemyTurn;
            turnPhase = playerTurn.NotPlayerTurn;
            notifier.FlashMessage("Enemy Turn!");
        }
    }


    public void CalculateMoveCost(Vector2Int tileLocation)
    {

    }
}
