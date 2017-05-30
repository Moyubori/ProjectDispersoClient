using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : Player {

	void Update(){
		
	}

	public void Respawn(Vector3 newPos){
		Transform cam = transform.Find ("Camera") as Transform;
		cam.position = new Vector3 (cam.position.x, cam.position.y + 0.5f, cam.position.z);
		transform.position = newPos;
		GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().enabled = true;
	}

	public override void Kill(){
		GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController> ().enabled = false;
		Transform cam = transform.Find ("Camera") as Transform;
		cam.position = new Vector3 (cam.position.x, cam.position.y - 0.5f, cam.position.z);
	}

}