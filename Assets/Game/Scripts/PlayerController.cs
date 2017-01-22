using System.Collections;
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
    public float abilityCooldown = 5.0f;

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
    public GameObject medPack;

    public Vector2 velocityChange = Vector2.zero;

    Rigidbody2D rb;
    SpriteRenderer sr;
    SoundManager soundManager;

    // Use this for initialization
    void Awake() {
        health = maxHealth;
        moveSpeed = humanSpeed;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        gamepad.OnDisconnect += Remove;
        gamepad.OnColorChanged += ColorChanged;
        gamepad.OnTap += PerformAction;
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
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
            case Role.ZOMBIE:
                PerformAction();    // just auto attack boxes as zombie
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
    }

    void FixedUpdate() {
        if (canMove && gamepad.touching) {
            rb.velocity = gamepad.dir * moveSpeed + velocityChange;
        } else {
            rb.velocity = Vector2.zero + velocityChange;
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
            gamepad.ChangeLives(0, 5);
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
        float oldHealth = prevHealth;
        if (alive) {
            if (prevHealth - health >= 20) {
                prevHealth = health;
                if (health <= 0) {
                    gamepad.ChangeLives(0, (int) oldHealth);
                } else if (health <= 20) {
                    gamepad.ChangeLives(20, (int) oldHealth);
                } else if (health <= 40) {
                    gamepad.ChangeLives(40, (int)oldHealth);
                } else if (health <= 60) {
                    gamepad.ChangeLives(60, (int) oldHealth);
                } else if (health <= 80) {
                    gamepad.ChangeLives(80, (int) oldHealth);
                } else if (health > 80) {
                    gamepad.ChangeLives(100, (int) oldHealth);
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
        if (alive && other.CompareTag("MedPack")) {
            AddHealth(50.0f);
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Wave")) {
            velocityChange = other.transform.up * -2.0f;
        }
    }

    private void OnTriggerExit2D(Collider2D col) {
        if (col.CompareTag("Wave")) {
            velocityChange = Vector2.zero;
        }
    }

    RaycastHit2D[] hitRes = new RaycastHit2D[8];
    void PerformAction() {
        //Debug.Log("Perform Action");
        if (canMove) {
            switch (role) {
                case Role.RUNNER:
                    if (abilityTimer < 0.0f) {
                        dashing = true;
                        dashTimer = maxDashTime;
                        abilityTimer = abilityCooldown;
                    }
                    break;
                case Role.POLICE:
                    if (abilityTimer < 0.0f) {
                        GameObject p = Instantiate(projectile, transform.position + transform.right.normalized * 2.0f + new Vector3(0, 0, 10), Quaternion.identity);
                        p.GetComponent<Projectile>().direction = transform.right;
                        abilityTimer = abilityCooldown;
                    }
                    break;
                case Role.MEDIC:
                    if (abilityTimer < 0.0f) {
                        GameObject p = Instantiate(medPack, transform.position, Quaternion.identity);
                        abilityTimer = 10.0f;
                    }
                    break;
                case Role.ZOMBIE:
                    if (abilityTimer > 0.0f) {
                        return;
                    }
                    abilityTimer = 0.1f;
                    int rets = Physics2D.RaycastNonAlloc(transform.position, gamepad.dir, hitRes, 2.0f);
                    for (int i = 0; i < rets; ++i) {
                        if (hitRes[i].collider.CompareTag("Movable")) {
                            if (Random.value < 0.25f) {
                                // random chance to spawn food
                                GameManager.instance.foodPrefab.GetComponentInChildren<SpriteRenderer>().sprite = GameManager.instance.foodSprites[Random.Range(0, GameManager.instance.foodSprites.Length)];
                                Instantiate(GameManager.instance.foodPrefab, hitRes[i].transform.position, Quaternion.identity);
                            }
                            // add box destruction
                            GameObject go = Instantiate(GameManager.instance.boxBreakPrefab, hitRes[i].transform.position, Quaternion.identity);
                            Destroy(go, 3.0f);
                            abilityTimer = 3.0f;
                            Destroy(hitRes[i].collider.gameObject);
                            break;
                        }
                    }
                    break;
            }
        }
    }

    public void DisableControls(float time) {
        Debug.Log("disabled");
        canMove = false;
        StartCoroutine("EnableMove", time);
    }

    IEnumerator EnableMove(float time) {
        yield return new WaitForSeconds(time);
        canMove = true;
    }

}
