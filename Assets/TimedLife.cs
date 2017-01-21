using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedLife : MonoBehaviour {
    public float lifeTime = 5.0f;
    public float spawnTime;
    public GameManager gameManager;

    // Use this for initialization
    void Start() {
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

        spawnTime = gameManager.gameTime;
    }
	
	// Update is called once per frame
	void Update () {
		if (gameManager.gameTime - spawnTime > lifeTime) {
            GameObject.Destroy(gameObject);
        }
    }
}
