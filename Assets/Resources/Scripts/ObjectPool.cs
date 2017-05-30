using UnityEngine;
using System.Collections;

public class ObjectPool : MonoBehaviour {

	public static ObjectPool instance;

	public GameObject[] objectPrefabs;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);    
		}
		//DontDestroyOnLoad (gameObject);
	}

	void Start(){
		objectPrefabs = Resources.LoadAll<GameObject> ("Prefabs");
		if (objectPrefabs == null) {
			Debug.LogError ("Prefabs not loaded into ObjectPool.");
		}
	}
		
	private GameObject FindPrefab<T>(string tag = null){
		foreach(GameObject prefab in objectPrefabs){
			if (prefab.GetComponent<T>() != null) {
				if (tag == null || prefab.tag == tag) {
					return prefab;
				}
			}
		}
		return null;
	}
		
	// seeks for existing inactive instance or creates a new one
	public GameObject GetInstance<T>(string tag = null) {
		foreach (Transform child in transform) {
			if (!child.gameObject.activeSelf && child.GetComponent<T>() != null) {
				child.gameObject.SetActive (true);
				return child.gameObject;
			}
		}
		try{
			GameObject prefab = FindPrefab<T>(tag);
			GameObject newObject = Instantiate (prefab, transform);
			newObject.name = prefab.name + "_" + InstancesOfTypeCreated<T> () + 1;
			return newObject;
		} catch (System.NullReferenceException e){
			throw new UnityException ("Prefab of type " + typeof(T) + " not found.");
		}
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

	// returns number of currently active objects of type T in the pool
	public int InstancesOfTypeActive<T>(){
		int counter = 0;
		foreach (Transform child in transform) {
			if (child.GetComponent<T> () != null && child.gameObject.activeSelf) {
				counter++;
			}
		}
		return counter;
	}

	// returns number of all objects in pool
	public int InstancesCreated(){
		return transform.childCount;
	}

	// returns number of all objects of type T in pool
	public int InstancesOfTypeCreated<T>(){
		int counter = 0;
		foreach (Transform child in transform) {
			if (child.GetComponent<T> () != null) {
				counter++;
			}
		}
		return counter;
	}
}
