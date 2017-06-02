using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameLabel : MonoBehaviour {

	[SerializeField]
	private TextMesh text;
	[SerializeField]
	private SpriteRenderer bgSprite;
	private RemotePlayer player;

	void Start(){
		player = transform.parent.GetComponent<RemotePlayer> ();
	}

	void Update(){
		text.text = player.name;
		transform.LookAt (Camera.main.transform.position);
		bgSprite.color = new Color (player.health / 100f, 0, 0);
	}

}
