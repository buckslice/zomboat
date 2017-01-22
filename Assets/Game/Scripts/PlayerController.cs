﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Role {
    MEDIC, // can heal other humans
    RUNNER, // can run faster and have a dash
    POLICE, // can shoot zombies with a taser to stun them temporarily
    SECRETZOMBIE, // a zombie that appears as human but can still infect others
    ZOMBIE,
    COUNT
}

public class PlayerController : MonoBehaviour {

    public float ZOMBIE_SCALE = 1.0f;
    public float HUMAN_SCALE = 0.75f;

    public float humanSpeed = 3.5f;
    public float zombieSpeed = 5.0f;
    public float dashSpeed = 10.0f;

    public float dashTimer = 0.0f;
    public float maxDashTime = 1.0f;
    public float abilityTimer = 0.0f;
    public float dashCooldown = 5.0f;

    public bool disabled = false;
    public float disabledTimer = 0.0f;
    float moveSpeed;
    bool dashing = false; // for runners
    public bool alive = true;   // zombies are dead
    public float health = 100.0f;
    public float maxHealth = 100.0f;
    private float prevHealth = 100.0f;
    public float dps; // damage that the zombies do to humans per second
    public float hps; // healing that the medics do to humans per second 
    public Role role;
    //Role previousRole; // in case we ever want to convert zombies back into humans
    bool canMove = true;

    public Sprite playerSprite;
    public Sprite zombieSprite;
    public ParticleSystem zombParticles;
    public ParticleSystem bloodParticles;
    public TopDownGamePad gamepad;
    public GameObject projectile;

    public Vector2 velocityChange = Vector2.zero;

    Rigidbody2D rb;
    SpriteRenderer sr;

    // Use this for initialization
    void Awake() {
        health = maxHealth;
        moveSpeed = humanSpeed;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        gamepad.OnDisconnect += Remove;
        gamepad.OnColorChanged += ColorChanged;
        gamepad.OnTap += PerformAction;

    }

    // Update is called once per frame
    void Update() {

        if (health < 0) {
            BeginZombification();
        }
        switch (role) {
            case Role.RUNNER:
                moveSpeed = zombieSpeed; // runners can run as fast as zombies
                break;
            case Role.SECRETZOMBIE:
                SetZombie(true);
                break;
            default:
                break;
        }

        abilityTimer -= Time.deltaTime;
        if (dashing) {
            dashTimer -= Time.deltaTime;
            if (dashTimer > 0.0f) {
                moveSpeed = dashSpeed;
            } else {
                moveSpeed = zombieSpeed;
                dashing = false;
            }
        }
        disabledTimer -= Time.deltaTime;
        if (disabledTimer < 0.0f) {
            disabled = false;
        }
    }

    void FixedUpdate() {
        if (!disabled) {
            if (canMove && gamepad.touching) {
                rb.velocity = gamepad.dir * moveSpeed + velocityChange;
            } else {
                rb.velocity = Vector2.zero + velocityChange;
            }
            //Debug.Log(gamepad.dir);  
        }
    }

    bool zombifying = false;
    public void BeginZombification() {
        if (zombifying) {
            return;
        }
        zombifying = true;

        StartCoroutine(ZombificationRoutine());
    }

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    IEnumerator ZombificationRoutine() {
        // turn on gross particle effect
        // disable controls?
        zombParticles.Play();

        yield return new WaitForSeconds(3.0f); ;

        zombParticles.Stop();
        SetZombie(true);
    }

    public void SetZombie(bool isZombie) {
        if (isZombie && !alive) {
            return;
        }

        if (isZombie) {
            bloodParticles.Stop();
            moveSpeed = zombieSpeed;
            health = 0.0f;
            gamepad.ChangeLives(0);
            gamepad.SendZombification();
            alive = false;
            if (role != Role.SECRETZOMBIE) {
                sr.sprite = zombieSprite;
                //previousRole = role;
                role = Role.ZOMBIE;
            }
            transform.localScale = new Vector3(ZOMBIE_SCALE, ZOMBIE_SCALE, 1);
        } else {
            moveSpeed = humanSpeed;
            health = 100.0f;
            alive = true;
            sr.sprite = playerSprite;
            //role = previousRole;
            transform.localScale = new Vector3(HUMAN_SCALE, HUMAN_SCALE, 1);
        }
    }

    void ColorChanged(Color c) {
        sr.color = c;
    }

    void Remove() {
        Destroy(gameObject);
    }
    public void AddHealth(float amount) {
        // adds the amount of health to the player health and clamps it at max health

        float oldHealth = health;
        if (alive) {
            if (prevHealth - health >= 20) {
                prevHealth = health;
                if (health <= 0) {
                    gamepad.ChangeLives(0);
                } else if (health <= 20) {
                    gamepad.ChangeLives(20);
                } else if (health <= 40) {
                    gamepad.ChangeLives(40);
                } else if (health <= 60) {
                    gamepad.ChangeLives(60);
                } else if (health <= 80) {
                    gamepad.ChangeLives(80);
                } else if (health > 80) {
                    gamepad.ChangeLives(100);
                }
            }
            health += amount;
            if (health >= maxHealth) {
                health = maxHealth;
                bloodParticles.Stop();
            }
        }
    }
    void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            PlayerController otherPlayer = collision.gameObject.GetComponent<PlayerController>();
            if (otherPlayer.alive) {
                if (!alive) {
                    otherPlayer.AddHealth(-dps * Time.deltaTime);
                    if (!otherPlayer.bloodParticles.isPlaying) {
                        otherPlayer.bloodParticles.Play();
                    }

                    Debug.Log("Blood");

                    //Debug.Log(otherPlayer.health);
                } else if (alive && role == Role.MEDIC) {
                    otherPlayer.AddHealth(hps * Time.deltaTime);
                    //Debug.Log(otherPlayer.health);
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D other) {
        if (alive && other.CompareTag("Food")) {
            AddHealth(10.0f);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Wave")) {
            velocityChange = other.GetComponent<WaveLine>().pushVel * 1.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D col) {
        if (col.CompareTag("Wave")) {
            velocityChange = Vector2.zero;
        }
    }

    void PerformAction() {
        //Debug.Log("Perform Action");
        switch (role) {
            case Role.RUNNER:
                if (abilityTimer < 0.0f) {
                    dashing = true;
                    dashTimer = maxDashTime;
                    abilityTimer = dashCooldown;
                }
                break;
            case Role.POLICE:
                if (abilityTimer < 0.0f) {
                    GameObject p = Instantiate(projectile, transform.position + transform.right.normalized * 2.0f + new Vector3(0, 0, 10), Quaternion.identity);
                    p.GetComponent<Projectile>().direction = transform.right;
                    abilityTimer = dashCooldown;
                }
                break;

        }
    }

    public void DisableControls(float time) {
        disabled = true;
        disabledTimer = time;
    }

}
