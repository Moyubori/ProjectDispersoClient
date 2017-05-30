using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour {

	public string name { get; protected set; }

	public void SetName(string name){
		//if (this.name != null) {
		//	Debug.LogError ("Name was already set.");
		//	return;
		//}
		this.name = name;
	}



	protected void Awake(){
		name = null;
	}

	public abstract void Kill ();

}
