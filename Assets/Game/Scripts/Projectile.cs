using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public Vector3 direction;
    public float speed = 5.0f;
    Rigidbody2D rb;
    // Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        rb.velocity = direction.normalized * speed;
	}

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (!player.alive) {
                player.DisableControls(3.0f);
            }
        }
        Destroy(gameObject);
    }
}
