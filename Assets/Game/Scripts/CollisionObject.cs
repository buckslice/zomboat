using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionObject : MonoBehaviour {
    public int waveIndex = 1;

	// Use this for initialization
	void Start () {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManagerObject.GetComponent<GameManager>().waveObjects.Add(new WaveObjectEntry(waveIndex, this));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void HandleWave()
    {
        GameObject.Destroy(gameObject);
    }
}
