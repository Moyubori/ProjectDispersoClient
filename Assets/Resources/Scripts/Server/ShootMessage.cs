using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class ShootMessage : IMessage {

	Vector3 playerPosition;
	float shootAngle;
	string hitPlayerName;

	string message;

	public ShootMessage(Vector3 playerPosition, float shootAngle, string hitPlayerName = null){
		this.playerPosition = playerPosition;
		this.shootAngle = shootAngle;
		this.hitPlayerName = hitPlayerName;

		JSONObject messageJSON = new JSONObject ();
		messageJSON ["type"] = "shot_fired";
		messageJSON ["x"].AsDouble = playerPosition.x;
		messageJSON ["y"].AsDouble = playerPosition.z;
		messageJSON ["rotation"].AsDouble = shootAngle;
		if (hitPlayerName != null) {
			messageJSON ["hitPlayerName"] = hitPlayerName;
		} else {
			messageJSON ["hitPlayerName"] = new JSONNull();
		}
		message = messageJSON.ToString ();
		Debug.Log (message);
	}

	public int Size(){
		return System.Text.Encoding.Default.GetByteCount (message);
	}

	public byte[] ToByteArray(){
		return System.Text.Encoding.Default.GetBytes (message);
	}
}
