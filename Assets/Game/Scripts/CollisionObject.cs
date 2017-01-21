using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionObject : MonoBehaviour {
    public int waveIndex = 1;

	// Use this for initialization
	void Start () {
        GameManager.instance.waveObjects.Add(new WaveObjectEntry(waveIndex, this));
    }

    public void HandleWave()
    {
        Destroy(gameObject);
    }
}
