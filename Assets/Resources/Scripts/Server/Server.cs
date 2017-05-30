using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

// Class responsible for communication with server
public class Server : MonoBehaviour {

	public static Server instance = null;

	public const int tickrate = 20;
	private const int timeoutLimit = 3000;

	private string clientName;
	private string ip;
	private int port;
	private Socket clientSocket;

	public Sender sender { get; private set; }
	public Receiver receiver { get; private set; }

	private List<RemotePlayer> remotePlayers = new List<RemotePlayer>();
	private LocalPlayer localPlayer;
	private Level level;

	public GameObject connectScreen;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);  
		}
		//DontDestroyOnLoad(gameObject);

		clientName = null;
		ip = null;
		port = -1;
	}

	void Start(){	
		level = GameObject.FindObjectOfType<Level> ();
		if (level == null) {
			Debug.LogError ("Can't fint level object.");
		}
	}

	public void ConnectToServer(){
		if (ip == null || port == -1) {
			Debug.LogError ("IP or Port were not set.");
			return;
		} else {
			Debug.Log ("Trying to connect to " + ip + ":" + port);
			clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			System.IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ip), port, new System.AsyncCallback(ConnectCallback), null);
			bool isConnected = result.AsyncWaitHandle.WaitOne (timeoutLimit, true);
			Debug.Log ("Connection status: " + isConnected);
			if (isConnected) {
				connectScreen.SetActive (false);

				receiver = gameObject.AddComponent<Receiver> ();
				receiver.SetSocket (clientSocket);
				receiver.Run ();

				sender = gameObject.AddComponent<Sender> ();
				sender.SetSocket (clientSocket);
				sender.Run ();

				sender.EnqueueMessage (new CustomMessage("{\"type\":\"set_name\",\"name\":\"" + clientName + "\"}"));

				StartCoroutine (SendPlayerStatus ());

				StartCoroutine (HandleReceivedMessages ());
			} else {
				Debug.LogError ("Connection error.");
			}
		}
	}
		
	public void SetIP(string newIP){
		ip = newIP;
	}

	public void SetPort(int newPort){
		port = newPort;
	}

	public void SetClientName(string name){
		clientName = name;
	}

	public bool IsConnectedToServer(){
		return clientSocket.Connected;
	}

	private void ConnectCallback(System.IAsyncResult ar){
		clientSocket.EndConnect (ar);
	}

	private IEnumerator SendPlayerStatus(){
		while (clientSocket.Connected) {
			if (localPlayer != null) {
				sender.EnqueueMessage (new MoveMessage (localPlayer.transform.position, localPlayer.transform.rotation.eulerAngles.y));
			}
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield break;
	}

	private IEnumerator HandleReceivedMessages(){
		Queue<JSONObject> messagesToHandle = receiver.GetMessages();
		while (clientSocket.Connected) {
			float timeStart = Time.time;
			while (messagesToHandle.Count > 0) {
				// handle messages here
				JSONObject message = messagesToHandle.Dequeue() as JSONObject;
				string msgType = message ["type"];
				Debug.Log("Handling message type: " + message["type"]);
				switch (msgType) {
				case "map":
					MSG_map (message);
					break;
				case "change_position":
					MSG_change_position (message);
					break;
				case "players":
					MSG_players (message);
					break;
				case "shot_fired":
					MSG_shotFired (message);
					break;
				case "player_is_dead":
					MSG_player_is_dead (message);
					break;
				case "player_respawn":
					MSG_respawn_player (message);
					break;
				default:
					Debug.LogError ("Message of unknown type." + msgType);
					break;
				}
			}
			messagesToHandle = receiver.GetMessages ();
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield break;
	}

	private void MSG_map(JSONObject message){
		List<Level.Wall> levelData = new List<Level.Wall> ();
		JSONArray wallsJSON = message["walls"].AsArray;
		foreach (JSONObject wall in wallsJSON) {
			Vector2 startPoint = new Vector2 (wall ["x1"].AsFloat, wall ["y1"].AsFloat);
			Vector2 endPoint = new Vector2 (wall ["x2"].AsFloat, wall ["y2"].AsFloat);
			levelData.Add (new Level.Wall (startPoint, endPoint, wall ["width"].AsFloat));
		}
		level.LoadLevel (levelData);
	}

	private void MSG_change_position(JSONObject message){
		if (localPlayer == null) {
			localPlayer = ObjectPool.instance.GetInstance<LocalPlayer> ().GetComponent<LocalPlayer>();
			localPlayer.transform.position = new Vector3 (message ["x"].AsFloat, localPlayer.transform.position.y, message ["y"].AsFloat);
			localPlayer.transform.rotation = Quaternion.AngleAxis (message ["rotation"].AsFloat, Vector3.up);
			localPlayer.transform.parent = null;
		} else {
			localPlayer.transform.position = new Vector3(message["x"].AsFloat, localPlayer.transform.position.y, message["y"].AsFloat);
			localPlayer.transform.rotation = Quaternion.AngleAxis (message ["rotation"].AsFloat, Vector3.up);
		}
	}


	private void MSG_players(JSONObject message){
		JSONArray players = message ["players"].AsArray;
		foreach (JSONObject player in players) {
			RemotePlayer playerInstance = remotePlayers.Find (x => x.name == player ["name"].AsString);
			if (playerInstance == null) {
				playerInstance = ObjectPool.instance.GetInstance<RemotePlayer> ().GetComponent<RemotePlayer>();
				playerInstance.SetName (player ["name"].AsString);
				remotePlayers.Add (playerInstance);
			}
			playerInstance.SetPosition (new Vector3 (player["x"].AsFloat, playerInstance.transform.position.y, player["y"].AsFloat));
			playerInstance.SetRotation (player ["rotation"].AsFloat);
		}
	}

	private void MSG_shotFired(JSONObject message){
		try{
		RemotePlayer playerInstance = remotePlayers.Find (x => x.name == message ["player"].AsString);
			playerInstance.GetComponent<RemoteWeaponController> ().Shoot ();
		} catch(System.NullReferenceException e){
			Debug.Log("exception");
		}
	}

	private void MSG_player_is_dead(JSONObject message){
		var player = remotePlayers.Find ((x) => x.name == message["name"].AsString);
		if (player == null) {
			if (localPlayer.name == message ["name"].AsString) {
				localPlayer.Kill ();
			} else {
				Debug.LogError ("Couldn't find player to kill.");
			}
		} else {
			player.Kill ();
		}
	}


	private void MSG_respawn_player(JSONObject message){
		localPlayer.Respawn (new Vector3(message["x"].AsFloat, 1.01f, message["y"].AsFloat));
	}

	void Update(){
		remotePlayers.RemoveAll ((x) => x.isDead || !x.gameObject.activeSelf);
	}

}
