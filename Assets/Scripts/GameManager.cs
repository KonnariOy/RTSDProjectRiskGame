using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public class Tile
    {
        public bool hasTree;
        public int owner;
        public int bees;
        public GameObject beeObject;

        public Tile(bool tree, int own, int bee)
        {
            hasTree = tree;
            owner = own;
            bees = bee;
        }
    }

    public int columns = 20;
    public int rows = 20;
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    private StartGame startScript;
    private Transform mapHolder;
    public GameObject GrassTile;
    public GameObject TreeTile;
    public GameObject BeeTile;
    int[,] treePos = { { 0, 3 }, { 11, 10 }, { 19, 14 } };
    int[,] beePos = { { 10, 10 }, {12, 12} , { 19, 14 } };
    int[] beeOwners = { 0, 0, 1 };
    private bool myTurn = false;
    public GameObject selectedArmy = null;
    public float tileSize = 0.32f;
    private Vector3 armyDestination;
    public bool selectDestination = false;
    private readonly int maxMoveDistance = 3;
    public int myIndex = 0;
    public int currentIndex = 0;
    public int playerCount = 4;
    public Color[] playerColors = { new Color(0f, 0.4f, 1f), new Color(1f, 0f, 0f), new Color(1f, 0f, 0f), new Color(1f, 0f, 0f) };
    public int maxBeeCount = 40;

    public List<List<Tile>> tiles;

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        startScript = GetComponent<StartGame>();

        //Call the InitGame function to initialize the map 
        InitGame();
    }

    void Start () {
		
	}

    void InitGame()
    {
        tiles = new List<List<Tile>>();
        MapSetup();
    }

    // Update is called once per frame
    void Update()
    {
        if (!myTurn & currentIndex == myIndex)
        {
            Debug.Log("Your turn.");
            myTurn = true;
            IncreaseBeeCount(myIndex);
        }
        if (!myTurn)
        {
            Debug.Log("Turn of player " + currentIndex);
            IncreaseBeeCount(currentIndex);
            currentIndex++;
            currentIndex %= playerCount;
        }
        if (Input.GetButtonDown("Fire2") & myTurn & selectDestination)
        {
            armyDestination = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
            Debug.Log(armyDestination);
            int destX = (int)System.Math.Round(armyDestination.x / tileSize, 0);
            int destY = (int)System.Math.Round(armyDestination.y / tileSize, 0);
            int locX = (int)System.Math.Round(selectedArmy.transform.position.x / tileSize, 0);
            int locY = (int)System.Math.Round(selectedArmy.transform.position.y / tileSize, 0);

            if (System.Math.Abs(destX - locX) > maxMoveDistance | System.Math.Abs(destY - locY) > maxMoveDistance |
                destX < 0 | destY < 0 | destX > (columns - 1) | destY > (rows - 1) | (destX == locX & destY == locY))
            {
                Debug.Log("Invalid move.");
            }
            else
            {
                tiles[locX][locY].bees = 0;
                bool moveToDestination = true;
                if (tiles[destX][destY].bees > 0)
                {
                    if (tiles[destX][destY].owner == myIndex)
                    {
                        // Combine the 2 friendly bee armies
                        tiles[destX][destY].bees += selectedArmy.GetComponent<BeeScript>().count;
                        tiles[destX][destY].beeObject.GetComponent<BeeScript>().AddCount(selectedArmy.GetComponent<BeeScript>().count);
                        Destroy(selectedArmy);
                        moveToDestination = false;
                    }
                    else
                    {
                        // The 2 bee armies fight
                        int result = selectedArmy.GetComponent<BeeScript>().count - tiles[destX][destY].bees;
                        if (result > 0)
                        {
                            Debug.Log("Your army won.");
                            Destroy(tiles[destX][destY].beeObject);
                            selectedArmy.GetComponent<BeeScript>().SetCount(result);
                        }
                        else if (result < 0)
                        {
                            Debug.Log("Other army won.");
                            Destroy(selectedArmy);
                            tiles[destX][destY].beeObject.GetComponent<BeeScript>().SetCount(-result);
                            tiles[destX][destY].bees = -result;
                            moveToDestination = false;
                        }
                        else
                        {
                            Debug.Log("Fight wiped out both armies.");
                            Destroy(tiles[destX][destY].beeObject);
                            Destroy(selectedArmy);
                            tiles[destX][destY].bees = 0;
                            moveToDestination = false;
                        }
                    }
                }
                if (moveToDestination)
                {
                    // Move selected army to destination and update tile list
                    tiles[destX][destY].owner = myIndex;
                    tiles[destX][destY].beeObject = selectedArmy;
                    tiles[destX][destY].bees = selectedArmy.GetComponent<BeeScript>().count;
                    tiles[locX][locY].beeObject = null;
                    tiles[locX][locY].bees = 0;
                    armyDestination.x = tileSize * (float)System.Math.Round(armyDestination.x / tileSize, 0);
                    armyDestination.y = tileSize * (float)System.Math.Round(armyDestination.y / tileSize, 0);
                    armyDestination.z = 0;
                    selectedArmy.transform.position = armyDestination;
                    currentIndex++;
                    currentIndex %= playerCount;
                    myTurn = false;
                }
                selectDestination = false;
            }
        }
    }

    //Creates the map
    void MapSetup()
    {
        mapHolder = new GameObject("Map").transform;

        for (int x = 0; x < columns; x++)
        {
            tiles.Add(new List<Tile>());
            for (int y = 0; y < rows; y++)
            {
                GameObject toInstantiate = GrassTile;

                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                GameObject instance =
                    Instantiate(toInstantiate, new Vector3(x*tileSize, y*tileSize, 0f), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(mapHolder);
                tiles[x].Add(new Tile(false,0,0));
            }
        }
        for (int i = 0; i < treePos.Length/2; i++)
        {
            GameObject toInstantiate = TreeTile;
            {
                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                GameObject instance =
                    Instantiate(toInstantiate, new Vector3(treePos[i,0] * tileSize, treePos[i,1] * tileSize, 0f), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(mapHolder);
                tiles[treePos[i, 0]][treePos[i, 1]].hasTree = true;
            }
        }
        for (int i = 0; i < beePos.Length / 2; i++)
        {
            GameObject toInstantiate = BeeTile;
            {
                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                GameObject instance =
                    Instantiate(toInstantiate, new Vector3(beePos[i, 0] * tileSize, beePos[i, 1] * tileSize, 0f), Quaternion.identity) as GameObject;
                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                //instance.transform.SetParent(mapHolder);
                tiles[beePos[i, 0]][beePos[i, 1]].bees = 10;
                tiles[beePos[i, 0]][beePos[i, 1]].beeObject = instance;
                tiles[beePos[i, 0]][beePos[i, 1]].owner = beeOwners[i];
                instance.GetComponent<BeeScript>().owner = beeOwners[i];
                instance.GetComponent<BeeScript>().countText.color = playerColors[beeOwners[i]];
            }
        }
    }

    public void SelectBee(GameObject bee)
    {
        Debug.Log(bee);
    }

    void IncreaseBeeCount(int player)
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (tiles[x][y].hasTree & tiles[x][y].owner == player & tiles[x][y].bees > 0 & tiles[x][y].bees < maxBeeCount)
                {
                    tiles[x][y].bees++;
                    tiles[x][y].beeObject.GetComponent<BeeScript>().AddCount(1);
                    Debug.Log("Increasing bee count in: " + x + ", " + y);
                }
            }
        }
    }
}
