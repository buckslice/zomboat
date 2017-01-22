using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {
    SpriteRenderer sr;
    public float t;
	// Use this for initialization
	void Start () {
        sr = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime;
        sr.color = new Color(1.0f, 1.0f, 1.0f, 0.5f + 0.3f * (Mathf.Sin(t)));
	}
}
