﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeeScript : MonoBehaviour {

    public int count = 10;
    public int owner;
    public Text countText;
    public SpriteRenderer playerSprite;
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnMouseDown()
    {
        if (owner == GameManager.instance.currentIndex) // myIndex
        {
            GameManager.instance.SelectArmy(gameObject);
            GameManager.instance.selectedBeeCount = count;
            GameManager.instance.selectedBeeCountText.text = "Bees selected: "+count.ToString();
            //GameManager.instance.selectedArmy = gameObject;
            //GameManager.instance.selectDestination = true;
        }
    }

    public void SetCount(int newCount)
    {
        count = newCount;
        countText.text = count.ToString();
    }
    public void AddCount(int newCount)
    {
        count += newCount;
        countText.text = count.ToString();
    }
}
