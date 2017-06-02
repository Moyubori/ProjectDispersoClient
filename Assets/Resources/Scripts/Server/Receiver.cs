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

	private Queue<JSONObject> savedMessages = new Queue<JSONObject> ();

	private string invalidMessage;

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
		clientSocket.EndReceive (ar);
		//Debug.Log ("Received message: " + System.Text.Encoding.Default.GetString (buffer));

		ValidateData ();

		long timeStart = System.DateTime.Now.Ticks;
		buffer = new byte[bufferSize];
		clientSocket.BeginReceive (buffer, 0, bufferSize, SocketFlags.None, new System.AsyncCallback (ReceiveCallback), null);
	}

	public Queue<JSONObject> GetMessages(){
		return savedMessages;
	}

	private void ValidateData(){
		string receivedData = System.Text.Encoding.Default.GetString (buffer).TrimEnd(new char[] {(char)0});
		if (invalidMessage != null) {
			receivedData = invalidMessage + receivedData;
			invalidMessage = null;
		}
		string correctMessage;
		int bracketCounter = 0;
		for (int i = 0; i < receivedData.Length; i++) {
			if (receivedData [i] == '{') {
				bracketCounter++;
			} else if (receivedData [i] == '}') {
				bracketCounter--;
			}
			if (bracketCounter == 0) {
				correctMessage = receivedData.Substring (0, i+1);
				JSONObject message = JSON.Parse (correctMessage).AsObject;
				savedMessages.Enqueue (message);
				receivedData = receivedData.Substring (i+1);
				if (receivedData.Length == 0)
					break;
				i = -1;
			}
		}
		invalidMessage = receivedData;
	}
		
}
