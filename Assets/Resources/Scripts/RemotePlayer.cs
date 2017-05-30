using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : Player {

	private Vector3 position;
	private Vector3 lastPosition;
	private Vector3 velocity;

	private bool isInitiated = false;
	private float lastUpdated;

	public const float timeout = 0.5f;
	public bool isDead = false;

	void Awake(){
		base.Awake ();

		velocity = Vector3.zero;
		position = transform.position;

		lastUpdated = 0;
	}

	void Update(){
		if (!isDead) {
			if (lastUpdated >= timeout) {
				velocity = Vector3.zero;

			} else {
				lastUpdated += Time.deltaTime;

				float interpolation = lastUpdated / (1f / Server.tickrate);
				transform.position = Vector3.Lerp (transform.position, position + velocity * interpolation, 0.1f);
			}
		}
	}

	void OnEnable(){
		isDead = false;
	}

	public void SetPosition(Vector3 position){
		if (!isInitiated) {
			this.lastPosition = position;
			this.isInitiated = true;
		} else {
			this.lastPosition = transform.position;
		}

		this.position = position;
		this.velocity = this.position - this.lastPosition;

		lastUpdated = 0;
	}

	public void SetRotation(float rotation){
		transform.eulerAngles = new Vector3 (0, rotation, 0);
	}

	public override void Kill(){
		isDead = true;
		gameObject.SetActive (false);
	}

}
