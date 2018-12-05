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
		public GameObject paintTile;

        public Tile(bool tree, int own, int bee)
        {
            hasTree = tree;
            owner = own;
            bees = bee;
        }
    }

    public class Player
    {
        public string name = "Default player";

        public Player(string _name)
        {
            name = _name;
        }
    }

    public class Move
    {
        public int player;
        public int fromX;
        public int fromY;
        public int toX;
        public int toY;
        public int beeCount;

        public Move(int index, int fX, int fY, int tX, int tY, int bees)
        {
            player = index;
            fromX = fX;
            fromY = fY;
            toX = tX;
            toY = tY;
            beeCount = bees;
        }
    }

	/* Game Values */
	
	public int turnCount = 0;
	
    public int columns = 20;
    public int rows = 20;
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    private StartGame startScript;
    public Player player = new Player("Local player 2");

	/* Game Field Holder */
    private Transform mapHolder;
    private Transform paintHolder;
	
	/* GameObjects, prefabricated */
    public GameObject GrassTile;
    public GameObject TreeTile;
    public GameObject BeeTile;
	public GameObject MoveToTile;
	// Army Script object
	public GameObject selectedArmy = null;
	
	/* Coordinates */
    int[,] treePos = { { 0, 3 }, { 11, 10 }, { 8, 8 }, { 3, 8 } };
    int[,] beePos = { { 10, 10 }, {11, 11} , { 7, 4 }, { 6, 10 } };
    int[] beeOwners = { 0, 0, 1, 3 };
	int[] treeOwners = { 0, 0, 1 };
	
	int[,] tempLayoutPos = {{1,1},{0,0}};
    
	/* Move Paint Logic Variables */
	int paintX;
	int paintY;
	public bool paintOn = false;

    /* Game Logic */
    public bool MapInitialized = false;
	private bool myTurn = false;
    public float tileSize = 0.32f;
    private Vector3 armyDestination;
    public bool selectDestination = false;
    private readonly int maxMoveDistance = 3;
    public int myIndex = 0;
    public int currentIndex = 0;
    public JSONObject players;
    public JSONObject map;
    public Color[] playerColors = { new Color(0f, 0.4f, 1f), new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(1f, 0f, 0f), new Color(1f, 1f, 0f), new Color(1f, 1f, 1f) };
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
        players = new JSONObject();
        map = new JSONObject();
        tiles = new List<List<Tile>>();
        //MapSetup();
    }

    // Update is called once per frame
    void Update()
    {
		if (!MapInitialized)
        {
            return;
        }
		
        if (!myTurn & currentIndex == myIndex)
        {
            Debug.Log("Your turn.");
            myTurn = true;
            IncreaseBeeCount(myIndex);
        }
        if (!myTurn)
        {
            return;
            Debug.Log("Turn of player " + currentIndex);
            IncreaseBeeCount(currentIndex);
            currentIndex++;
            currentIndex %= players.Count;
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
				if(!paintOn)
					paintMovePresentTile(paintX, paintY, false);
				
            }
            else
            {
                NetworkManager.instance.MakeMove(new Move(myIndex,locX,locY,destX,destY,selectedArmy.GetComponent<BeeScript>().count));
                selectDestination = false;
				paintOn = false;
                selectedArmy = null;
            }
			if(!paintOn)
				paintMovePresentTile(paintX, paintY, false);
        }
    }
	
	bool cTreePosit(int x, int y) {
		return tiles[x][y].hasTree;
	}
	
	bool cBeePosit(int x, int y) {
		if(tiles[x][y].bees > 0)
			return true;
		else
			return false;
	}
	
	void paintMovePresentTile(int x, int y, bool createTiles) {

        // Destroy all move tiles
        if (!createTiles)
        {
            foreach (Transform child in paintHolder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            return;
        }

        // Calculate positions for possible moves and create tiles
		int a = x + maxMoveDistance + 1;
		int b = y + maxMoveDistance + 1;
		int c = x - maxMoveDistance;
		int d = y - maxMoveDistance;

        if (a > columns)
			a = columns;
		if(b > rows)
			b = rows;
		if(c < 0)
			c = 0;
		if(d < 0)
			d = 0;
        int startD = d;
		while(c < a) {
			while(d < b) {
                if (!(x == c && y == d))
                {
                    makeTempTile(c, d);
				}
				++d;
			}
            d = startD;
		++c;
		}
    }
	
	void makeTempTile(int x, int y) {
		
		//Debug.Log("MakeTempTile to: x: "+x+ " y: "+y);
		GameObject instance;
		GameObject toInstantiateTile = MoveToTile;
        Color tmpColor = playerColors[myIndex];
        tmpColor.a = 0.4f;
        MoveToTile.GetComponent<SpriteRenderer>().color = tmpColor;

        {
			//Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.

			instance = Instantiate(toInstantiateTile, new Vector3(x * tileSize, y * tileSize, -2f), Quaternion.identity) as GameObject;

			//Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
			//mapHolder = new GameObject("Overlay").transform;
			instance.transform.SetParent(paintHolder);
			//tiles[x][y].paintTile = instance;
		}
	}
	
	void removeTempTile(int x, int y) {
		Debug.Log("DestroyTempTile1_3");
		Destroy(tiles[x][y].paintTile);
		
	}

    //Creates the map
    void MapSetup()
    {
        mapHolder = new GameObject("Map").transform;
        paintHolder = new GameObject("MoveTiles").transform;

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
                tiles[x].Add(new Tile(false,-1,0));
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
                //instance.GetComponent<BeeScript>().countText.color = playerColors[beeOwners[i]];
                Color tmpColor = playerColors[beeOwners[i]];
                tmpColor.a = 0.5f;
                instance.GetComponent<BeeScript>().playerSprite.color = tmpColor;
            }
        }
    }

    //Creates the map
    public void NetworkMapSetup()
    {
        mapHolder = new GameObject("Map").transform;
        paintHolder = new GameObject("MoveTiles").transform;
        int index = 0;
        for (int x = 0; x < columns; x++)
        {
            tiles.Add(new List<Tile>());
            for (int y = 0; y < rows; y++)
            {
                //int.Parse(socketIOevent.data["columns"].ToString()
                GameObject toInstantiate = GrassTile;

                //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                GameObject instance =
                    Instantiate(toInstantiate, new Vector3(x * tileSize, y * tileSize, 0f), Quaternion.identity) as GameObject;

                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(mapHolder);
                tiles[x].Add(new Tile(false, -1, 0));

                if(int.Parse(map[index]["hasTree"].ToString()) == 1)
                {
                    Debug.Log("There is a tree at x: " + x + " y: " + y);
                    GameObject treeToInstantiate = TreeTile;
                    {
                        //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                        GameObject treeInstance =
                            Instantiate(treeToInstantiate, new Vector3(x * tileSize, y * tileSize, 0f), Quaternion.identity) as GameObject;

                        //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                        treeInstance.transform.SetParent(mapHolder);
                        tiles[x][y].hasTree = true;
                    }
                }

                int owner = int.Parse(map[index]["owner"].ToString());
                int bees = int.Parse(map[index]["bees"].ToString());
                if (owner != -1 && bees > 0)
                {
                    Debug.Log("Creating bee:" + map[index].ToString());
                    GameObject beeToInstantiate = BeeTile;
                    {
                        //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                        GameObject beeInstance =
                            Instantiate(beeToInstantiate, new Vector3(x * tileSize, y * tileSize, 0f), Quaternion.identity) as GameObject;
                        //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                        tiles[x][y].bees = bees;
                        tiles[x][y].beeObject = beeInstance;
                        tiles[x][y].owner = owner;
                        beeInstance.GetComponent<BeeScript>().owner = owner;
                        beeInstance.GetComponent<BeeScript>().SetCount(bees);
                        Color tmpColor = playerColors[owner];
                        tmpColor.a = 0.5f;
                        beeInstance.GetComponent<BeeScript>().playerSprite.color = tmpColor;
                    }
                }

                index++;
            }
        }
        MapInitialized = true;
    }

    public void UpdateMapFromServer()
    {
        int index = 0;
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                //if tiles[x][y].bees !=
                //if tiles[x][y].owner !=
                index++;
            }
        }
    }

    public void PlayMoveFromServer(JSONObject moveData)
    {
        JSONObject move = moveData["tiles"];
        for (int i = 0; i < move.Count; i++)
        {
            int index = int.Parse(move.keys[i]);
            int xIndex = index / columns;
            int yIndex = index - xIndex * columns;
            Debug.Log("x: " + xIndex + " y: " + yIndex);
            int newOwner = int.Parse(move[move.keys[i]]["owner"].ToString());
            int newBeeCount = int.Parse(move[move.keys[i]]["bees"].ToString());

            Debug.Log("x: " + xIndex + " y: " + yIndex + " owner: " + tiles[xIndex][yIndex].owner + " new owner: " + newOwner);

            if (tiles[xIndex][yIndex].owner != newOwner)
            {
                Debug.Log("Owner changed.");
                Destroy(tiles[xIndex][yIndex].beeObject);

                tiles[xIndex][yIndex].owner = newOwner;

                if (newOwner != -1)
                {
                    GameObject beeToInstantiate = BeeTile;
                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    GameObject beeInstance =
                        Instantiate(beeToInstantiate, new Vector3(xIndex * tileSize, yIndex * tileSize, 0f), Quaternion.identity) as GameObject;
                    //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                    tiles[xIndex][yIndex].beeObject = beeInstance;
                    beeInstance.GetComponent<BeeScript>().owner = newOwner;

                    Color tmpColor = playerColors[newOwner];
                    tmpColor.a = 0.5f;
                    beeInstance.GetComponent<BeeScript>().playerSprite.color = tmpColor;
                }
            }

            if (tiles[xIndex][yIndex].bees != newBeeCount)
            {
                Debug.Log("Beecount changed.");
                tiles[xIndex][yIndex].beeObject.GetComponent<BeeScript>().SetCount(newBeeCount);
            }
        }

        currentIndex = int.Parse(moveData["turnIndex"].ToString());
        if (currentIndex == myIndex)
        {
            myTurn = true;
        }
        else
        {
            myTurn = false;
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
	
	public void turnChange() 
	{
			IncreaseBeeCount(1);
			turnCount = turnCount + 1;
	}

    public void SelectArmy(GameObject army)
    {
        if (!myTurn)
        {
            return;
        }
        if (army == selectedArmy)
        {
            Debug.Log("Same army selected");
        }
        else
        {
            if (paintOn)
            {
                paintMovePresentTile(0, 0, false);
                paintOn = false;
            }
            instance.selectedArmy = army;
            instance.selectDestination = true;
            if (myTurn & !paintOn)
            {
                armyDestination = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
                paintX = (int)System.Math.Round(selectedArmy.transform.position.x / tileSize, 0);
                paintY = (int)System.Math.Round(selectedArmy.transform.position.y / tileSize, 0);
                paintMovePresentTile(paintX, paintY, true);
                paintOn = true;
            }
            Debug.Log("Different army selected");
        }
    }
}
