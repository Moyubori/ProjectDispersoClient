using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

	public int gunDamage = 1;											
	public float fireRate = 0.25f;										
	public float weaponRange = 50f;										
	public Transform gunEnd;	
	public Camera camera;

	private WaitForSeconds shotDuration = new WaitForSeconds(0.07f);
	private LineRenderer laserLine;
	private float nextFire;
	private LocalPlayer player;

	void Start () {
		laserLine = GetComponent<LineRenderer>();
		player = transform.parent.GetComponent<LocalPlayer> ();
		if (player == null) {
			Debug.LogError ("player not found");
		}
	}

	void Update () {
		if (Input.GetButtonDown("Fire1") && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			StartCoroutine (ShotEffect());
			Vector3 rayOrigin = camera.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
			RaycastHit hit;
			laserLine.SetPosition (0, gunEnd.position);
			if (Physics.Raycast (rayOrigin, camera.transform.forward, out hit, weaponRange)) {
				if (hit.collider.GetComponent<Player> ()) {
					Server.instance.sender.EnqueueMessage (new ShootMessage (player.transform.position, player.transform.rotation.eulerAngles.y, hit.collider.GetComponent<Player> ().name));
				} else {
					Server.instance.sender.EnqueueMessage (new ShootMessage (player.transform.position, player.transform.rotation.eulerAngles.y));
				}
				laserLine.SetPosition (1, hit.point);
				StartCoroutine (HitEffect (hit.point));
			}
			else {
				laserLine.SetPosition (1, rayOrigin + (camera.transform.forward * weaponRange));
				Server.instance.sender.EnqueueMessage (new ShootMessage (player.transform.position, player.transform.rotation.eulerAngles.y));
			}
		}
	}

	private IEnumerator ShotEffect() {
		laserLine.enabled = true;
		yield return shotDuration;
		laserLine.enabled = false;
	}

	private IEnumerator HitEffect(Vector3 explosionPosition){
		ParticleSystem explosion = ObjectPool.instance.GetInstance<ParticleSystem> ("WeaponHitExplosion").GetComponent<ParticleSystem> ();
		Debug.Log (explosion);
		explosion.transform.position = explosionPosition;
		explosion.Play ();
		yield return new WaitForSeconds (explosion.duration - 0.05f);
		explosion.gameObject.SetActive (false);
	}
}