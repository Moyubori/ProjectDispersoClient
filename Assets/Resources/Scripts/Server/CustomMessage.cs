using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class CustomMessage : IMessage {

	private string message;

	public CustomMessage(string message){
		this.message = message;
	}

	public int Size (){
		return System.Text.Encoding.Default.GetByteCount (message);
	}


	public byte[] ToByteArray (){
		return System.Text.Encoding.Default.GetBytes (message);
	}
}
