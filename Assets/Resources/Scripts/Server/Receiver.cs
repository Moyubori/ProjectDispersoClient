using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using SimpleJSON;

public class Receiver : MonoBehaviour {

	private Socket clientSocket;
	private int tickrate;

	private const int bufferSize = 1024;
	private byte[] buffer = new byte[bufferSize];

	private Queue<JSONClass> savedMessages = new Queue<JSONClass> ();

	public Receiver(Socket socket){
		clientSocket = socket;
		tickrate = Server.tickrate;
	}

	public void Run(){
		if (!clientSocket.Connected) {
			throw new System.Net.Sockets.SocketException ("Socket not connected to a server");
		}
		clientSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new System.AsyncCallback(ReceiveCallback), null);
	}

	private void ReceiveCallback(System.IAsyncResult ar){
		int bytesReceived = clientSocket.EndReceive (ar);
		Debug.Log ("Received message: " + System.Text.Encoding.Default.GetString (buffer));
		JSONClass message = JSON.Parse (System.Text.Encoding.Default.GetString (buffer)).AsObject;
		savedMessages.Enqueue (message);
		buffer = new byte[bufferSize];
		clientSocket.BeginReceive(buffer, 0, bufferSize, SocketFlags.None, new System.AsyncCallback(ReceiveCallback), null);
	}

	public Queue<JSONClass> GetMessages(){
		return savedMessages;
	}

}
