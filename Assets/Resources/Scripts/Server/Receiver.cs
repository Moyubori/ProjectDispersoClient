using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using SimpleJSON;

public class Receiver : MonoBehaviour {

	private Socket clientSocket;
	private int tickrate = Server.tickrate;

	private const int bufferSize = 1024;
	private byte[] buffer = new byte[bufferSize];

	private Queue savedMessages = new Queue ();

	public void SetSocket(Socket socket){
		clientSocket = socket;
	}

	public void Run(){
		if (!clientSocket.Connected) {
			Debug.LogError ("Socket not connected to a server");
			return;
		}
		clientSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new System.AsyncCallback(ReceiveCallback), null);
	}

	private void ReceiveCallback(System.IAsyncResult ar){
		int bytesReceived = clientSocket.EndReceive (ar);
		Debug.Log ("Received message: " + System.Text.Encoding.Default.GetString (buffer));
		JSONClass message = JSON.Parse (System.Text.Encoding.Default.GetString (buffer)).AsObject;
		lock (savedMessages.SyncRoot) {
			savedMessages.Enqueue (message);
			buffer = new byte[bufferSize];
			clientSocket.BeginReceive (buffer, 0, bufferSize, SocketFlags.None, new System.AsyncCallback (ReceiveCallback), null);
		}
	}

	public Queue GetMessages(){
		return savedMessages;
	}

}
