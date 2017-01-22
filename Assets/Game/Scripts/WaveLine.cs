﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveLine : MonoBehaviour {

    public float speed = 2.0f; // movespeed
    Vector3 moveDir;
    bool oscillating = true;
    public float lifeTime = 5.0f;
    float range;

    // Use this for initialization
    void Start() {
        StartCoroutine(SlideAndDestroy());
        moveDir = transform.right;
        range = Random.Range(3.0f, 6.0f);
    }

    void Update() {
        float t = oscillating ? Mathf.Sin(Time.time * 2.0f) * range : 1.0f;
        transform.Translate(Time.deltaTime * moveDir * speed * t);
    }

    IEnumerator SlideAndDestroy() {
        yield return new WaitForSeconds(lifeTime);

        // slide away before deleting so we get OnTriggerExit2D events
        speed = 200.0f;
        moveDir = Vector3.down;
        oscillating = false;

        yield return new WaitForSeconds(5.0f);

        Destroy(gameObject);

    }
}
