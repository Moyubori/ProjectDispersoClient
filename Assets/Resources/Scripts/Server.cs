using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// Class responsible for communication with server
public class Server : MonoBehaviour {

	public static Server instance = null;

	public static bool isConnected { get; private set; }
	public static readonly int tickrate = 20;

	private static string clientName;
	private static string ip;
	private static int port;

	private static Socket clientSocket;


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

	public static void ConnectToServer(){
		if (ip == null || port == -1) {
			Debug.LogError ("IP or Port were not set.");
			return;
		} else {
			Debug.Log ("Trying to connect to " + ip + ":" + port);

			clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			System.IAsyncResult result = clientSocket.BeginConnect(IPAddress.Parse(ip), port, new System.AsyncCallback(ReceiveCallback), null);
			isConnected = result.AsyncWaitHandle.WaitOne (3000, true);
			Debug.Log ("Connection status: " + isConnected);
		}
	}

	public static void SetIP(string newIP){
		ip = newIP;
	}

	public static void SetPort(int newPort){
		port = newPort;
	}

	public static void SetClientName(string name){
		clientName = name;
	}

	private static void ReceiveCallback(System.IAsyncResult AR){

	}
		
}
