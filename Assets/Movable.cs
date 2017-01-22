using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour {
    public float stubborness = 1.0f;
    public float weight = 1.0f;
    public float destructability = 1.0f;
    public float destruction = 1.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D collision) {
        Movable otherMovable = collision.collider.GetComponent<Movable>();
        if (otherMovable && destructability < otherMovable.destruction) {
            OnDestruction();
        }
    }

    void OnDestruction() {
        float r = Random.Range(0.0f, 1.0f);
        if (r < 0.25f) {
            Instantiate(GameManager.instance.foodPrefab, transform.position, Quaternion.identity);
        }
        GameObject.Destroy(gameObject);
    }
}
