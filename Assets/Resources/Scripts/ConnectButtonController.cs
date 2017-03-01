using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ConnectButtonController : MonoBehaviour {

	public InputField nameInputField;
	public InputField ipInputField;
	public InputField portInputField;

	private static Regex ipPattern = new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");

	void Awake(){
		if (nameInputField == null) {
			Debug.LogError ("NameInputField not connected to ConnectButtonConteoller.");
		}
		if (ipInputField == null) {
			Debug.LogError ("IPInputField not connected to ConnectButtonConteoller.");
		}
		if (portInputField == null) {
			Debug.LogError ("PortInputField not connected to ConnectButtonConteoller.");
		}
	}

	public void ButtonOnClick(){
		string ip = ipInputField.text;
		if (ipPattern.IsMatch (ip)) {
			Server.SetIP (ip);
		} else {
			Debug.LogError ("Incorrect IP.");
			return;
		}
		try{
			int port = Int32.Parse(portInputField.text);
			Server.SetPort (port);
		} catch (FormatException e){
			Debug.LogError ("Entered Port is incorrect.");
		}
		string name = nameInputField.text;
		Server.SetClientName (name);
		Server.ConnectToServer ();
	}

}
