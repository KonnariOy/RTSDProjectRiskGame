using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class NetworkManager : MonoBehaviour {

    public static NetworkManager instance = null;
    public SocketIOComponent socket;
    public string authStatus = string.Empty;

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

    }
    // Use this for initialization
    void Start () {
        socket.On("player connected", OnPlayerConnected);
        socket.On("other player connected", OnOtherPlayerConnected);
        socket.On("other player disconnected", OnPlayerDisconnected);
        socket.On("move ok", OnMoveOk);
        socket.On("login ok", OnAuthenticationOk);
        socket.On("login fail", OnAuthenticationFail);
        socket.On("create_account ok", OnCreateAccountOk);
        socket.On("create_account fail", OnCreateAccountFail);
        FirstConnect();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator ConnectToServer(string name)
    {
        yield return new WaitForSeconds(2f);
        socket.Emit("player connect", new JSONObject(name));
        yield return new WaitForSeconds(1f);
    }

    void OnPlayerConnected(SocketIOEvent socketIOevent)
    {
        GameManager.instance.players = socketIOevent.data["players"];
        Debug.Log(socketIOevent);
        GameManager.instance.map = socketIOevent.data["map"];
        GameManager.instance.columns = int.Parse(socketIOevent.data["columns"].ToString());
        GameManager.instance.myIndex = int.Parse(socketIOevent.data["myIndex"].ToString());
        GameManager.instance.player.index = int.Parse(socketIOevent.data["myIndex"].ToString());
        GameManager.instance.currentIndex = int.Parse(socketIOevent.data["turnIndex"].ToString());
        GameManager.instance.rows = int.Parse(socketIOevent.data["rows"].ToString());
        GameManager.instance.NetworkMapSetup();
        string data = socketIOevent.data.ToString();
        Debug.Log(data);
    }

    void OnOtherPlayerConnected(SocketIOEvent socketIOevent)
    {
        GameManager.instance.players = socketIOevent.data["players"];
        Debug.Log(GameManager.instance.players.Count);
        Debug.Log(socketIOevent.data.ToString());
        GameManager.instance.PlayMoveFromServer(socketIOevent.data);
    }


    void OnPlayerDisconnected(SocketIOEvent socketIOevent)
    {
        Debug.Log(socketIOevent.data["1"]);
        string data = socketIOevent.data.ToString();
        Debug.Log(data);
    }

    void OnMoveOk(SocketIOEvent socketIOevent)
    {
        Debug.Log("Sent move was legal: "+ socketIOevent.data.ToString());
        GameManager.instance.PlayMoveFromServer(socketIOevent.data);
    }

    void OnAuthenticationOk(SocketIOEvent socketIOevent)
    {
        Debug.Log("login ok");
        authStatus = "Successfully logged in";
        FirstConnect();
    }

    void OnAuthenticationFail(SocketIOEvent socketIOevent)
    {
        Debug.Log("login fail");
        authStatus = "Failed to log in";
    }

    void OnCreateAccountOk(SocketIOEvent socketIOevent)
    {
        Debug.Log("create_account ok");
        authStatus = "Account created";
    }

    void OnCreateAccountFail(SocketIOEvent socketIOevent)
    {
        Debug.Log("create_account fail");
        authStatus = "Failed to create account";
    }


    public void FirstConnect()
    {
        //string data = JsonUtility.ToJson(GameManager.instance.player);
        string data = JsonUtility.ToJson("abc");
        Debug.Log(data);
        StartCoroutine(ConnectToServer(data));
    }

    public void MakeMove(GameManager.Move move)
    {
        string data = JsonUtility.ToJson(move);
        socket.Emit("make move", new JSONObject(data));
        Debug.Log("emit make move");
    }

    public void PassTurn(GameManager.Player player)
    {
        string data = JsonUtility.ToJson(player);
        socket.Emit("pass turn", new JSONObject(data));
        Debug.Log("emit pass turn");
    }

    public void LoginRequest(PlayerCredentials cred)
    {
        string data = JsonUtility.ToJson(cred);
        socket.Emit("player login", new JSONObject(data));
        Debug.Log("emit player login " + data);
    }

    public void CreateAccountRequest(PlayerCredentials cred)
    {
        string data = JsonUtility.ToJson(cred);
        socket.Emit("player create_account", new JSONObject(data));
        Debug.Log("emit player create_account " + data);
    }
}
