using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour {

    public int destructability = 1;

    Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    //void OnCollisionEnter2D(Collision2D col) {
    //    if (col.collider.CompareTag("Movable")){
    //        Movable other = col.collider.GetComponent<Movable>();
    //        if (other && destructability < other.destructability) {
    //            if (Random.value < 0.25f) { // random chance to spawn food
    //                GameManager.instance.foodPrefab.GetComponentInChildren<SpriteRenderer>().sprite = GameManager.instance.foodSprites[Random.Range(0, GameManager.instance.foodSprites.Length)];
    //                Instantiate(GameManager.instance.foodPrefab, transform.position, Quaternion.identity);
    //            }
    //            Destroy(gameObject);
    //        }
    //    }
    //}

    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Wave")) {
            rb.drag = 0.0f;
            rb.velocity = other.transform.up * -2.0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Wave")) {
            rb.drag = 10000.0f;
        }
    }
}
