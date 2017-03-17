using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : Player {

	protected Vector3 position;
	protected Vector3 lastPosition;
	protected Vector3 velocity;

	protected bool isInitiated = false;
	protected float lastUpdated;

	void Awake(){
		base.Awake ();

		velocity = Vector3.zero;
		position = transform.position;

		lastUpdated = 0;
	}

	void Update(){
		lastUpdated += Time.deltaTime;

		float interpolation = lastUpdated / (1f / Server.tickrate);
		transform.position = Vector3.Lerp(transform.position, position + velocity * interpolation, 0.1f);
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

}
