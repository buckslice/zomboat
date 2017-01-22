using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour {
    public float minWait = 2.0f;
    public float maxWait = 10.0f;
    public GameObject wavePrefab;
    private List<Transform> spawnPoints = new List<Transform>();
    float timeToSpawn = 10.0f;

    // Use this for initialization
    void Start() {
        foreach (Transform t in transform) {
            spawnPoints.Add(t);
        }
    }

    // Update is called once per frame
    void Update() {
        timeToSpawn -= Time.deltaTime;
        if (timeToSpawn < 0.0f) {
            // spawn wave
            Transform t = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Instantiate(wavePrefab, t.position, t.rotation);
            timeToSpawn = Random.Range(minWait, maxWait);
        }
    }
}
