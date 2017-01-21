using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float humanSpeed = 3.5f;
    public float zombieSpeed = 5.0f;
    float moveSpeed;
    public bool alive = true;   // zombies are dead
    public float health = 100.0f;
    public float dps = 10.0f; // damage that the zombies do to humans per second

    public Sprite playerSprite;
    public Sprite zombieSprite;
    public ParticleSystem zombParticles;
    public TopDownGamePad gamepad;

    Rigidbody2D rb;
    SpriteRenderer sr;

    // Use this for initialization
    void Awake() {
        moveSpeed = humanSpeed;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        gamepad.OnDisconnect += Remove;
        gamepad.OnColorChanged += ColorChanged;
    }

    // Update is called once per frame
    void Update() {
        if (health < 0) {
            BeginZombification();
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

    // 
    bool zombifying = false;
    public void BeginZombification() {
        if (zombifying) {
            return;
        }
        zombifying = true;

        StartCoroutine(ZombificationRoutine());
    }

    WaitForSeconds wait = new WaitForSeconds(3.0f);
    IEnumerator ZombificationRoutine() {
        // turn on gross particle effect
        // disable controls?
        zombParticles.Play();

        yield return wait;

        zombParticles.Stop();
        SetZombie(true);
    }

    void SetZombie(bool isZombie) {
        if(isZombie && !alive) {
            return;
        }

        if (isZombie) {
            moveSpeed = zombieSpeed;
            health = 0.0f;
            alive = false;
            sr.sprite = zombieSprite;
        } else {
            moveSpeed = humanSpeed;
            health = 100.0f;
            alive = true;
            sr.sprite = playerSprite;
        }
    }

    void ColorChanged(Color c) {
        sr.color = c;
    }

    void Remove() {
        Destroy(gameObject);
    }
    void AddHealth(float amount) {
        if (alive) {
            health += amount;
        }
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
