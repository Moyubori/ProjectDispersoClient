using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Weapon : MonoBehaviour {

	public Rigidbody bullet;
	public float shootForce;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			bullet.isKinematic = false;
			bullet.AddForce (transform.forward * shootForce);
		}
	}
}
