using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		
		public int whoseBee() 
		{
			return owner;
		}
    }

	/* Game Values */
	
	public int turnCount = 0;
	
    public int columns = 20;
    public int rows = 20;
    public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.
    private StartGame startScript;
	
	/* Game UI */
	
	//public GameObject CanvasUI;
	public GameObject buttonPassTurn;
	public GameObject imageWhosTurn;
	public GameObject imageWhoseTurnText;
	
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
	private bool myTurn = false;
    public float tileSize = 0.32f;
    private Vector3 armyDestination;
    public bool selectDestination = false;
    private readonly int maxMoveDistance = 3;
    public int myIndex = 0;
    public int currentIndex = 0;
    public int playerCount = 4;
    public Color[] playerColors = { new Color(0f, 0.4f, 1f), new Color(1f, 0f, 0f), new Color(1f, 0f, 0f), new Color(1f, 0f, 0f) };
	public Color32[] playerTurnColors = { new Color32(255, 255, 0, 100), new Color32(255, 0, 255, 100), new Color32(0, 255, 0, 100), new Color32(255, 0, 0, 100) };
    public int maxBeeCount = 40;
	public bool passTurn = false;

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
		UISetup();
    }

		
	void TurnChange() 
	{
			IncreaseBeeCount(currentIndex);
			turnCount = turnCount + 1;
			currentIndex++;
			if(currentIndex >= playerCount)
				currentIndex = 0;
			imageWhosTurn.GetComponent<Image>().color = playerTurnColors[currentIndex];
			imageWhoseTurnText.GetComponent<Text>().text = "Whose turn: " + (currentIndex + 1);
			Debug.Log("SwitchTurn" + (currentIndex + 1));
	}
	
    // Update is called once per frame
    void Update()
    {
		
		bool moveToDestination = false;
		/*
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
        }*/
	
        if (Input.GetButtonDown("Fire2") & selectDestination) // myTurn
        {
            armyDestination = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
			
            Debug.Log(armyDestination);
            int destX = (int)System.Math.Round(armyDestination.x / tileSize, 0);
            int destY = (int)System.Math.Round(armyDestination.y / tileSize, 0);
            int locX = (int)System.Math.Round(selectedArmy.transform.position.x / tileSize, 0);
            int locY = (int)System.Math.Round(selectedArmy.transform.position.y / tileSize, 0);
			Debug.Log(destX + " " + destY + " " + locX + " " + locY);
			
            if (System.Math.Abs(destX - locX) > maxMoveDistance | System.Math.Abs(destY - locY) > maxMoveDistance |
                destX < 0 | destY < 0 | destX > (columns - 1) | destY > (rows - 1) | (destX == locX & destY == locY))
            {
                Debug.Log("Invalid move.");
				if(!paintOn)
					paintMovePresentTile(paintX, paintY, false);
				
            }
            else
            {
				Debug.Log("I " + tiles[locX][locY].whoseBee() + " " + currentIndex);
				if((tiles[locX][locY].whoseBee() == currentIndex)) {
					moveToDestination = true;
					tiles[locX][locY].bees = 0;
					if (tiles[destX][destY].bees > 0)
					{
						if (tiles[destX][destY].owner == currentIndex) // myIndex
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
								Debug.Log("Attacker won.");
								Destroy(tiles[destX][destY].beeObject);
								selectedArmy.GetComponent<BeeScript>().SetCount(result);
							}
							else if (result < 0)
							{
								Debug.Log("Defender won.");
								Destroy(selectedArmy);
								tiles[destX][destY].beeObject.GetComponent<BeeScript>().SetCount(-result);
								tiles[destX][destY].bees = -result;
								moveToDestination = false;
							}
							else
							{
								Debug.Log("Mutual wipe out.");
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
						tiles[destX][destY].owner = currentIndex; //myIndex;
						tiles[destX][destY].beeObject = selectedArmy;
						tiles[destX][destY].bees = selectedArmy.GetComponent<BeeScript>().count;
						tiles[locX][locY].beeObject = null;
						tiles[locX][locY].bees = 0;
						armyDestination.x = tileSize * (float)System.Math.Round(armyDestination.x / tileSize, 0);
						armyDestination.y = tileSize * (float)System.Math.Round(armyDestination.y / tileSize, 0);
						armyDestination.z = 0;
						selectedArmy.transform.position = armyDestination;
						/*currentIndex++;
						currentIndex %= playerCount;*/
						//myTurn = false;
						passTurn = true;
					}
					selectDestination = false;
					paintOn = false;
				}
				selectedArmy = null;
            }
			if(!paintOn)
				paintMovePresentTile(paintX, paintY, false);
			if(passTurn) {
				passTurn = false;
				TurnChange();
			}
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
        Color tmpColor = playerColors[currentIndex]; //myIndex
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
                //instance.GetComponent<BeeScript>().countText.color = playerColors[beeOwners[i]];
                Color tmpColor = playerColors[beeOwners[i]];
                tmpColor.a = 0.5f;
                instance.GetComponent<BeeScript>().playerSprite.color = tmpColor;
            }
        }
    }
	
	void UISetup() 
	{

		//CanvasUI = GameObject.FindGameObjectWithTag("CanvasUI");
		imageWhosTurn = GameObject.Find("ImageWhosTurn"); //      buttonPassTurn;
		imageWhoseTurnText = GameObject.Find("ImageWhoseTurnText"); //      buttonPassTurn;
		buttonPassTurn = GameObject.Find("ButtonPassTurn");
		imageWhosTurn.GetComponent<Image>().color = playerTurnColors[currentIndex];
		buttonPassTurn.GetComponent<Button>().onClick.AddListener(turnPass);

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

    public void SelectArmy(GameObject army)
    {

		Debug.Log("My army." + instance.currentIndex);
		if (army == selectedArmy)
		{
			Debug.Log("Same army selected");
		}
		else
		{
			instance.selectedArmy = army;
			armyDestination = Camera.main.ScreenPointToRay(Input.mousePosition).origin;
			paintX = (int)System.Math.Round(selectedArmy.transform.position.x / tileSize, 0);
			paintY = (int)System.Math.Round(selectedArmy.transform.position.y / tileSize, 0);
			Debug.Log(paintX + " " + paintY + " " + instance.tiles[paintX][paintY].whoseBee() + " " + instance.currentIndex );
			if((instance.tiles[paintX][paintY].whoseBee() == instance.currentIndex)) 
			{
				if (paintOn)
				{
					paintMovePresentTile(0, 0, false);
					paintOn = false;
				}
				
				instance.selectDestination = true;
				if (!paintOn) // myTurn
				{
					paintMovePresentTile(paintX, paintY, true);
					paintOn = true;
				}
				Debug.Log("Different army selected " + paintX + " " + paintY);
			}
			else {
				Debug.Log("Not my army.");
			}
		}
    }
	
	public void turnPass() {
		Debug.Log("New Player Turn " + (currentIndex + 1));
		passTurn = true;
		TurnChange();
	}
}
