using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using SimpleJSON;

public class Sender : MonoBehaviour {

	private Socket clientSocket;
	private int tickrate;

	private Queue<IMessage> messagesToSend = new Queue<IMessage>();

	public Sender(Socket socket){
		clientSocket = socket;
		tickrate = Server.tickrate;
	}

	public void Run(){
		StartCoroutine (SendMessages ());
	}

	public void EnqueueMessage(IMessage message){
		messagesToSend.Enqueue (message);
	}

	private IEnumerator SendMessages(){
		while (clientSocket.Connected) {
			IMessage message;
			if (messagesToSend.Count == 0) {
				// create a ping message here
				messagesToSend.Enqueue(message);
			}
			while (messagesToSend.Count > 0) {
				message = messagesToSend.Dequeue ();
				clientSocket.BeginSend(message.ToByteArray, 0, message.Size(), SocketFlags.None, new System.AsyncCallback(SendCallback), null);
			}
			yield return new WaitForSeconds (1.0f / tickrate);
		}
		yield break;
	}

	private void SendCallback(System.IAsyncResult ar){
		int bytesSend = clientSocket.EndSend (ar);
		Debug.Log (bytesSend + " bytes sent to server.");
	}

}
