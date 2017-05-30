using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteWeaponController : MonoBehaviour {


	public int gunDamage = 1;											
	public float fireRate = 0.25f;										
	public float weaponRange = 50f;										
	public Transform gunEnd;
	public ParticleSystem explosionPrefab;

	private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);
	private LineRenderer laserLine;
	private float nextFire;

	void Start () {
		laserLine = GetComponent<LineRenderer>();
	}

	public void Shoot() {
		nextFire = Time.time + fireRate;
		StartCoroutine (ShotEffect());
		Vector3 rayOrigin = gunEnd.position + new Vector3(0,0,1);
		RaycastHit hit;
		laserLine.SetPosition (0, gunEnd.position);
		if (Physics.Raycast (rayOrigin, gunEnd.transform.forward, out hit, weaponRange)) {
			laserLine.SetPosition (1, hit.point);
			StartCoroutine (HitEffect (hit.point));
		}
		else {
			laserLine.SetPosition (1, rayOrigin + (gunEnd.transform.forward * weaponRange));
		}
	}

	private IEnumerator ShotEffect() {
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}

	private IEnumerator HitEffect(Vector3 explosionPosition){
		ParticleSystem explosion = Instantiate (explosionPrefab).GetComponent<ParticleSystem> ();
		Debug.Log (explosion);
		explosion.transform.position = explosionPosition;
		explosion.Play ();
		yield return new WaitForSeconds (explosion.duration - 0.05f);
		explosion.gameObject.SetActive (false);
	}
}
