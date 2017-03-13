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

	private Sender sender;
	private Receiver receiver;

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

	private IEnumerator HandleReceivedMessages(){
		Queue messagesToHandle = receiver.GetMessages();
		while (clientSocket.Connected) {
			while (messagesToHandle.Count > 0) {
				// handle messages here
				JSONClass message = messagesToHandle.Dequeue() as JSONClass;
				Debug.Log("Handling message type: " + message["type"]);
				switch (message ["type"]) {
				case "map":
					MSG_map (message);
					break;
				case "change_position":
					MSG_change_position (message);
					break;
				case "players":
					MSG_players (message);
					break;
				default:
					Debug.LogError ("Message of unknown type.");
					break;
				}
			}
			messagesToHandle = receiver.GetMessages ();
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield break;
	}

	private void MSG_map(JSONClass message){
		List<Level.Wall> levelData = new List<Level.Wall> ();
		JSONArray wallsJSON = message["walls"].AsArray;
		foreach (JSONClass wall in wallsJSON) {
			Vector2 startPoint = new Vector2 (wall ["x1"].AsFloat, wall ["y1"].AsFloat);
			Vector2 endPoint = new Vector2 (wall ["x2"].AsFloat, wall ["y2"].AsFloat);
			levelData.Add (new Level.Wall (startPoint, endPoint, wall ["width"].AsFloat));
		}
		level.LoadLevel (levelData);
	}

	private void MSG_change_position(JSONClass message){
		if (localPlayer == null) {
			localPlayer = ObjectPool.instance.GetInstance<LocalPlayer> ().GetComponent<LocalPlayer>();
			localPlayer.transform.position = new Vector3 (message ["x"].AsFloat, localPlayer.transform.position.y, message ["y"].AsFloat);
			localPlayer.transform.parent = null;
		} else {
			localPlayer.transform.position = new Vector3(message["x"].AsFloat, localPlayer.transform.position.y, message["y"].AsFloat);
		}
	}

		private void MSG_players(JSONClass message){
		JSONArray players = message ["players"].AsArray;
		foreach (JSONClass player in players) {
			RemotePlayer playerInstance = remotePlayers.Find (x => x.name == player ["name"].ToString());
			if (playerInstance == null) {
				Debug.Log (player["name"].ToString());

				playerInstance = ObjectPool.instance.GetInstance<RemotePlayer> ().GetComponent<RemotePlayer>();
				playerInstance.SetName (player ["name"].ToString());
				remotePlayers.Add (playerInstance);
			}
			playerInstance.transform.position = Vector3.Lerp(playerInstance.transform.position,new Vector3 (player ["x"].AsFloat - playerInstance.transform.position.x, 1, player ["y"].AsFloat - playerInstance.transform.position.z), 0.1f);
			//playerInstance.transform.position = new Vector3 (player["x"].AsFloat, playerInstance.transform.position.y, player["y"].AsFloat);
		}
	}

}
