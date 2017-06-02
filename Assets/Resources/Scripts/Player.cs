using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour {

	public string name { get; protected set; }
	public int health { get; protected set; }

	public void SetName(string name){
		this.name = name;
	}

	public void SetHealth(int health){
		this.health = health;
	}

	protected void Awake(){
		name = null;
	}

	public abstract void Kill ();

}
