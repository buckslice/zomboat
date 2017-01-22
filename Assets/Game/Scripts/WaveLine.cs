using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveLine : MonoBehaviour {

    public Vector3 moveVel;
    public Vector3 pushVel;
    public float lifeTime = 5.0f;

    // Use this for initialization
    void Start() {
        StartCoroutine(SlideAndDestroy());
    }

    void Update() {
        transform.Translate(Time.deltaTime * moveVel);
    }

    IEnumerator SlideAndDestroy() {
        yield return new WaitForSeconds(lifeTime);

        // slide away before deleting so we get OnTriggerExit2D events
        moveVel = new Vector3(0.0f, -200.0f, 0.0f);
        yield return new WaitForSeconds(5.0f);

        Destroy(gameObject);

    }

    //void OnTriggerStay2D(Collider2D other) {
    //    if (other.CompareTag("Movable") && other.attachedRigidbody) {
    //        other.attachedRigidbody.drag = 0.0f;
    //        other.attachedRigidbody.velocity = direction * 2.0f;
    //    }else if (other.CompareTag("Player")) {
    //        other.GetComponent<PlayerController>().velocityChange = direction * 5.0f;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D other) {
    //    if (other.CompareTag("Movable") && other.attachedRigidbody) {
    //        other.attachedRigidbody.drag = 10000.0f;
    //    }
    //}

}
