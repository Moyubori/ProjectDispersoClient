using UnityEngine;
using System.Collections;

public class ObjectPool : MonoBehaviour {

	public GameObject[] objectPrefabs;

	void Start(){
		objectPrefabs = Resources.LoadAll<GameObject> ("Prefabs");
		if (objectPrefabs == null) {
			Debug.LogError ("Prefabs not loaded into ObjectPool.");
		}
	}

	private GameObject FindPrefabOfType<T>(){
		foreach(GameObject prefab in objectPrefabs){
			if (prefab.GetComponent<T>() != null) {
				return prefab;
			}
		}
		return null;
	}

	//seeks for existing inactive projectile instance or creates new one
	public GameObject GetInstance<T>() {
		foreach (GameObject child in transform) {
			if (!child.activeSelf && child.GetComponent<T>() != null) {
				child.SetActive (true);
				return child;
			}
		}
		try{
			GameObject prefab = FindPrefabOfType<T>();
			GameObject newObject = Instantiate (prefab, transform);
			newObject.name = prefab.name + "_" + InstancesOfTypeActive<T> () + 1;
			return newObject;
		} catch (System.NullReferenceException e){
			throw new UnityException ("Prefab of type " + typeof(T) + " not found.");
		}
	}


	// returns the instance of the prefab and sets it to given (global) position and rotation
	public GameObject GetInstance<T>(Vector3 position, Quaternion rotation){
		GameObject result = GetInstance<T> ();
		result.transform.position = position;
		result.transform.rotation = rotation;
		return result;
	}

	public GameObject GetInstance<T>(Vector3 position) {
		GameObject result = GetInstance<T> ();
		result.transform.position = position;
		return result;
	}

	// returns the instance of the prefab and sets it to given local position (relative to the center of the camera)
	public GameObject GetInstanceRelative<T>(Vector3 position, Quaternion rotation){
		GameObject result = GetInstance<T> ();
		result.transform.localPosition = position;
		result.transform.rotation = rotation;
		return result;
	}

	public GameObject GetInstanceRelative<T>(Vector3 position) {
		GameObject result = GetInstance<T> ();
		result.transform.localPosition = position;
		return result;
	}

	// returns number of currently active objects in the pool
	public int InstancesActive(){
		int counter = 0;
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).gameObject.activeSelf) {
				counter++;
			}
		}
		return counter;
	}

	public int InstancesOfTypeActive<T>(){
		int counter = 0;
		foreach (Transform child in transform) {
			if (child.GetComponent<T> () != null && child.gameObject.activeSelf) {
				counter++;
			}
		}
		return counter;
	}
}
