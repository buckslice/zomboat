using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float moveSpeed = 2.0f;
    public bool alive = true;   // zombies are dead
    public float health = 100.0f;
    public float dps = 10.0f; // damage that the zombies do to humans per second

    public Sprite playerSprite;
    public Sprite zombieSprite;

    private TopDownGamePad gamepad;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // Use this for initialization
    void Start() {
        gamepad = GetComponent<TopDownGamePad>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        gamepad.OnDisconnect += Remove;
        gamepad.OnColorChanged += ColorChanged;
    }

    // Update is called once per frame
    void Update() {
        if (health <= 0) {
            health = 0.0f;
            alive = false;
            sr.sprite = zombieSprite;
        }
    }

    void FixedUpdate() {
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

    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player") && !alive) {
            PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (otherPlayer.alive) {
                otherPlayer.health -= dps * Time.deltaTime;
                //Debug.Log(otherPlayer.health);
            }
        }
    }

}
