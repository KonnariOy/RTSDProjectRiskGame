using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {

    /*public Texture2D tex;
    public SpriteRenderer sr;*/

    bool firstSceneClick = false;
    bool lastSceneClick = false;
    // Use this for initialization

    void Start () {
        //this.InitStartScene();
        /*Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        sr = gameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.color = new Color(0.9f, 0.9f, 0.9f);
        transform.position = new Vector3(1.5f, 1.5f, 0.0f);
        sr.sprite = mySprite;*/
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            firstSceneClick = true;
        }
        if (firstSceneClick) {
            this.InitLoginScene();
        }
    }

    void InitLoginScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    void InitStartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
}