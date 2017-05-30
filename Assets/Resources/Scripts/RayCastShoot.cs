using UnityEngine;
using System.Collections;

public class RaycastShoot : MonoBehaviour {

	public int gunDamage = 1;											
	public float fireRate = 0.25f;										
	public float weaponRange = 50f;										
	public Transform gunEnd;	
	public Camera camera;

	private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);
	private LineRenderer laserLine;
	private float nextFire;

	void Start () {
		laserLine = GetComponent<LineRenderer>();
	}
		
	void Update () {
		if (Input.GetButtonDown("Fire1") && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			StartCoroutine (ShotEffect());
			Vector3 rayOrigin = camera.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
			RaycastHit hit;
			laserLine.SetPosition (0, gunEnd.position);
			if (Physics.Raycast (rayOrigin, camera.transform.forward, out hit, weaponRange)) {
				laserLine.SetPosition (1, hit.point);
			}
			else {
				laserLine.SetPosition (1, rayOrigin + (camera.transform.forward * weaponRange));
			}
		}
	}
		
	private IEnumerator ShotEffect() {
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}
}