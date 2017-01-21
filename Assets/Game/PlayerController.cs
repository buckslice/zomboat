using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 2.0f;

    private TopDownGamePad gamepad;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
        gamepad = GetComponent<TopDownGamePad>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        gamepad.OnDisconnect += Remove;
        gamepad.OnColorChanged += ColorChanged;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (gamepad.touching) {
            rb.velocity = gamepad.dir * moveSpeed;
        } else {
            rb.velocity = Vector2.zero;
        }
        //Debug.Log(gamepad.dir);    
	}

    void ColorChanged(Color c) {
        sr.color = c;
    }

    void Remove() {
        Destroy(gameObject);
    }
}
