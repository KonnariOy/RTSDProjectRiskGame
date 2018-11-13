using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {

    BarracksObject MyBarracks;


    public Texture2D tex;
    public SpriteRenderer sr;

    string currentScene;

    int amountOfTreesOwned = 1;
    int amountOfTreesOwnedEnemy = 1;

    bool firstSceneClick = false;
    bool lastSceneClick = false;
    // Use this for initialization

    public Sprite GameTreeOne;

    void Start () {
        //this.InitStartScene();
        Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        sr = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.color = new Color(0.9f, 0.9f, 0.9f);
        transform.position = new Vector3(1.5f, 1.5f, 0.0f);
        sr.sprite = mySprite;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            firstSceneClick = true;
        }
        if (amountOfTreesOwned == 4 || amountOfTreesOwnedEnemy == 4) {
            this.InitEndScene();
         
        }
        if (firstSceneClick) {
            this.InitGameScene();
        }
    }

    void InitGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        MyBarracks = new BarracksObject(3);
    }

    void InitStartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

    void InitEndScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");
    }

    void createBee()
    {
        // on tree click create Bee, addArmy to treeobject
    }

    void resolveTurn()
    {
        MyBarracks.addArmy(1 + MyBarracks.landsOwnedByMe());
    }

    void chooseObject()
    {
        //onclick, treeId gained. 
    }
}

public class ResolverDice 
{
    System.Random rnd = new System.Random();

    public int rollDice(int sideA, int sideB)
    {
          return rnd.Next(sideA, sideB);
    }
}



public class BarracksObject
{ // This is personal account of player. Holds id, ArmyReserves nad Lands Owned for accounting purposes.
    int ArmyReserves = 0;
    int landsOwned = 1;
    int currentTreeChosen;

    public BarracksObject(int startArmy)
    {
        ArmyReserves = startArmy;
    }
    
    public void addArmy(int increaseArmy)
    {
        ArmyReserves += increaseArmy;
    }

    public void putArmy(int objectCoord)
    {
        if (currentTreeChosen.Equals(null))
            ;
        else
        {
            ArmyReserves -= 1;

        }

        // put onto the tree , create bee on tree
    }

    public int landsOwnedByMe()
    {
        return landsOwned;
    }

    public void giveArmy(int treeId)
    {
        //getTreeObjectWithId(treeId).addArmyOwn();
    }
}

public class TreeObject
{
    int treeId;
    int ArmyOwned = 0;
    int EnemyArmies = 0;
    int treeCoordX = 0;
    int treeCoordY = 0;

    ResolverDice tempResolver;

    public TreeObject(int treeIdned)
    {
        treeId = treeIdned;
    }

    public int whichTreeIam()
    {
        return treeId;
    }

    public void resolveConflict(ResolverDice c)
    {
        tempResolver = c;
        int a = tempResolver.rollDice(1, 6);
        int b = tempResolver.rollDice(1, 6);
        if (a < b)
            ArmyOwned--;
        else if (a > b)
            EnemyArmies--;
        else
        {
            EnemyArmies--;
            ArmyOwned--;
        }
            
    }


}
