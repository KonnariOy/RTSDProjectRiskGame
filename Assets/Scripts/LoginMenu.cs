using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCredentials {
	public string username;
	public string password;

	public PlayerCredentials(string user, string pass)
	{
		username = user;
		password = pass;
	}
}

public class LoginMenu : MonoBehaviour {
	private string username = string.Empty;
	private string password = string.Empty;

	private Rect windowRect = new Rect(0, 0, Screen.width, Screen.height);
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnGUI()
	{
		GUI.Window(0, windowRect, WindowFunction, "Login");
		GUI.Label(new Rect(Screen.width / 3, 5 * Screen.height / 6, Screen.width / 3, Screen.height / 8), NetworkManager.instance.authStatus);
	}

	void WindowFunction(int windowId)
	{
		username = GUI.TextField(new Rect(Screen.width / 3, 2 * Screen.height / 5, Screen.width / 3 + 10, Screen.height / 10), username, 10);
		password = GUI.PasswordField(new Rect(Screen.width / 3, 2 * Screen.height / 3 - 40, Screen.width / 3 + 10, Screen.height / 10), password, '*', 10);

		if (GUI.Button(new Rect(Screen.width / 2 + 30, 3 * Screen.height / 4 - 20, Screen.width / 8, Screen.height / 8), "Log in")) {
			NetworkManager.instance.LoginRequest(new PlayerCredentials(username, password));
			//if username and password ok play game
		}

		if (GUI.Button(new Rect(Screen.width / 3, 3 * Screen.height / 4 - 20, Screen.width / 5, Screen.height / 8), "Create\naccount")) {
			NetworkManager.instance.CreateAccountRequest(new PlayerCredentials(username, password));
			//create account
		}

		GUI.Label(new Rect(Screen.width / 3, 35 * Screen.height / 100, Screen.width / 5, Screen.height / 8), "Username");
		GUI.Label(new Rect(Screen.width / 3, 62 * Screen.height / 100 - 50, Screen.width / 5, Screen.height / 8), "Password");
	}
}
