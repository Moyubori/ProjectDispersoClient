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

	public Level level;

	[SerializeField]
	private bool isConnected;
	private bool coroutinesStarted = false;

	public const int tickrate = 20;	// messages to send/receive to server per second
	private const int buffersize = 512;
	private const int timeoutLimit = 3000;
	private  float lastPackageReceivedTime;


	private string clientName;
	private string ip;
	private int port;

	private Socket clientSocket;
	private byte[] buffer = new byte[buffersize];

	private List<Level.Wall> levelData = new List<Level.Wall>();
	private List<RemotePlayer> remotePlayer = new List<RemotePlayer>();
	private LocalPlayer localPlayer;

	private Queue<JSONArray> messagesToSend = new Queue<JSONArray> ();

	private int receivedMessages;
	private Queue<JSONClass> savedMessages = new Queue<JSONClass> ();

	public GameObject connectScreen;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);  
		}
		DontDestroyOnLoad(gameObject);

		isConnected = false;

		clientName = null;
		ip = null;
		port = -1;
	}

	void Start(){
		level = GameObject.FindObjectOfType<Level> ().GetComponent<Level>();
		lastPackageReceivedTime = 0f;
		receivedMessages = 0;
	}

	public void ConnectToServer(){
		if (ip == null || port == -1) {
			Debug.LogError ("IP or Port were not set.");
			return;
		} else {
			Debug.Log ("Trying to connect to " + ip + ":" + port);
			clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			System.IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ip), port, new System.AsyncCallback(ConnectCallback), null);
			isConnected = result.AsyncWaitHandle.WaitOne (3000, true);
			Debug.Log ("Connection status: " + isConnected);
			if (isConnected) {
				connectScreen.SetActive (false);


				// change that so it sends a message to messages queue in stead of sending it directly to server
				JSONClass message = new JSONClass ();
				message ["type"] = "set_name";
				message ["name"] = clientName;
				clientSocket.Send (System.Text.Encoding.Default.GetBytes (message.ToString()));
				try{
					ReceiveMessages();
					StartCoroutine(SendMessages());
					StartCoroutine(HandleReceivedMessages());
				} catch (System.NullReferenceException e){
					Debug.LogError ("Couldn't download level from server");
				}
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
		return isConnected;
	}

	public List<Level.Wall> GetLevelData(){
		return levelData;
	}

	private void ConnectCallback(System.IAsyncResult ar){
		clientSocket.EndConnect (ar);
	}

	private void ReceiveCallback(System.IAsyncResult ar){
		int bytesReceived = clientSocket.EndReceive (ar);
		Debug.Log (bytesReceived.ToString() + " bytes received from server.");
		Debug.Log ("Received message: " + System.Text.Encoding.Default.GetString (buffer));
		JSONClass message = JSON.Parse (System.Text.Encoding.Default.GetString (buffer)).AsObject;
		savedMessages.Enqueue (message);
		receivedMessages++;
		buffer = new byte[buffersize];
		clientSocket.BeginReceive(buffer,0,buffersize,SocketFlags.None,new System.AsyncCallback(ReceiveCallback),null);
	}

	private void SendCallback(System.IAsyncResult ar){
		int bytesSend = clientSocket.EndSend (ar);
		Debug.Log (bytesSend + " bytes sent to server.");
	}

	private void DownloadLevelData(JSONClass message){
		try{
			Debug.Log("Downloading Level");
			JSONArray wallsJSON = message["walls"].AsArray;
			foreach (JSONClass wall in wallsJSON) {
				Vector2 startPoint = new Vector2 (wall ["x1"].AsFloat, wall ["y1"].AsFloat);
				Vector2 endPoint = new Vector2 (wall ["x2"].AsFloat, wall ["y2"].AsFloat);
				levelData.Add (new Level.Wall (startPoint, endPoint, wall ["width"].AsFloat));
			}
		} catch (System.NullReferenceException e){
			throw new System.NullReferenceException ("Couldn't download level from server.");
		}
		level.LoadLevel ();
	}

	private void ReceiveMessages(){
		try {
			clientSocket.BeginReceive(buffer,0,buffersize,SocketFlags.None,new System.AsyncCallback(ReceiveCallback),null);
			lastPackageReceivedTime = Time.time;
		} catch (System.NullReferenceException e) {
			Debug.Log ("No message received.");

		}
	}

	private IEnumerator HandleReceivedMessages(){
		while (isConnected) {
			if (receivedMessages > 0) {
				JSONClass message = savedMessages.Dequeue ();
				Debug.Log ("Message type: " + message ["type"]);

				switch (message ["type"]) {
				case "change_position":
					{
						Debug.Log ("changeposition");
						if (localPlayer == null) {
							MSG_SpawnPlayer (message);
							break;
						}
						MSG_ChangePosition (message);
						break;
					}
				case "map":
					{
						Debug.Log ("map");
						DownloadLevelData (message);
						break;
					}
				case "ping":
					{
						Debug.Log ("ping");
						break;
					}
				}
				receivedMessages--;
			}
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield return new WaitForSeconds (1.0f / tickrate);
	}

	private IEnumerator SendMessages(){
		while (isConnected) {
			if (messagesToSend.Count == 0) {
				clientSocket.BeginSend (System.Text.Encoding.Default.GetBytes ("{\"type\":\"ping\"}"), 0, System.Text.Encoding.Default.GetByteCount ("{\"type\":\"ping\"}"), SocketFlags.None, new System.AsyncCallback (SendCallback), null);
			} else {

			}
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield return new WaitForSeconds (1.0f / tickrate);
	}

	private void MSG_ChangePosition(JSONClass message){
		localPlayer.transform.position = new Vector3(message["x"].AsFloat, localPlayer.transform.position.y, message["y"].AsFloat);
	}

	private void MSG_SpawnPlayer(JSONClass message){
		localPlayer = ObjectPool.instance.GetInstance<LocalPlayer> ().GetComponent<LocalPlayer>();
		localPlayer.transform.position = new Vector3 (message ["x"].AsFloat, localPlayer.transform.position.y, message ["y"].AsFloat);
		localPlayer.transform.parent = null;
	}
}
