using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

	public GameObject wallPrefab;

	public class Wall{
		public Vector2 startPoint;
		public Vector2 endPoint;
		public float width;

		public Wall(Vector2 startPoint, Vector2 endPoint, float width){
			this.startPoint = startPoint;
			this.endPoint = endPoint;
			this.width = width;
		}
	}

	private List<Wall> levelData;
	private List<GameObject> levelObjects = new List<GameObject>();

	public void LoadLevel(List<Wall> levelData){
		this.levelData = levelData;
		CreateLevel ();
	}

	private void CreateLevel(){
		foreach (Wall wall in levelData) {
			GameObject wallObject = Instantiate (wallPrefab, transform);
			float wallLength = Vector2.Distance (wall.startPoint, wall.endPoint);
			float height = Mathf.Clamp (Random.value * 10f, 2, 10);
			Vector3 startPoint3D = new Vector3 (wall.startPoint.x, 0, wall.startPoint.y);
			Vector3 endPoint3D = new Vector3 (wall.endPoint.x, 0, wall.endPoint.y);

			wallObject.transform.localScale = new Vector3 (wall.width, height, wallLength);
			wallObject.transform.position = new Vector3 ((wall.startPoint.x + wall.endPoint.x)/2, height/2, (wall.startPoint.y + wall.endPoint.y)/2);
			wallObject.transform.rotation = Quaternion.LookRotation (endPoint3D - startPoint3D);
		}
	}

}
