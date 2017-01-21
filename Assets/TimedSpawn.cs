using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedSpawn : MonoBehaviour {
    public float[] spawnTimes;
    public Vector3[] spawnPositions;
    public GameObject[] spawnObjects;
    public GameManager gameManager;
    public int spawnedObjectIndex = 0;

	// Use this for initialization
	void Start () {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null) {
            GameObject.Destroy(this);
            return;
        }

        gameManager = gameManagerObject.GetComponent<GameManager>();
        if (gameManager == null) {
            GameObject.Destroy(this);
            return;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (spawnedObjectIndex >= spawnTimes.Length) {
            GameObject.Destroy(this);
            return;
        }

		while (gameManager.gameTime > spawnTimes[spawnedObjectIndex]) {
            GameObject spawnedObject = GameObject.Instantiate(spawnObjects[spawnedObjectIndex]);
            spawnedObject.transform.position = spawnPositions[spawnedObjectIndex];
            spawnedObjectIndex = spawnedObjectIndex + 1;
        }
	}
}
