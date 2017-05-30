using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class MoveMessage : IMessage {

	private Vector3 playerPosition;

	private string message;

	public MoveMessage(Vector3 playerPosition, float playerRotation){
		this.playerPosition = playerPosition;

		JSONObject messageJSON = new JSONObject ();
		messageJSON ["type"] = "move";
		messageJSON ["x"].AsDouble = playerPosition.x;
		messageJSON ["y"].AsDouble = playerPosition.z;
		messageJSON ["rotation"].AsDouble = playerRotation;

		message = messageJSON.ToString ();
		//Debug.Log (message);
	}

	public int Size(){
		return System.Text.Encoding.Default.GetByteCount (message);
	}

	public byte[] ToByteArray(){
		return System.Text.Encoding.Default.GetBytes (message);
	}
}
