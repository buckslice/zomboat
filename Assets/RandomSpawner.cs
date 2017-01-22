using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour {
    public GameObject spawnObject;
    public Vector3 startPositionMinimum;
    public Vector3 startPositionExtent;
    public float baseTime = 10.0f;
    public float variation = 5.0f;
    private float nextSpawn;


	// Use this for initialization
	void Start () {
        nextSpawn = baseTime + Random.Range(-variation, variation);
	}
	
	// Update is called once per frame
	void Update () {
        nextSpawn -= Time.deltaTime;
        if (nextSpawn <= 0.0f) {
            nextSpawn += baseTime + Random.Range(-variation, variation);
            Vector3 position = Vector3.Lerp(startPositionMinimum, startPositionExtent, Random.Range(0.0f, 1.0f));
            GameObject.Instantiate(spawnObject, position, Quaternion.identity);
        }
	}
}
