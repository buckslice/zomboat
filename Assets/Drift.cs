using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drift : MonoBehaviour {
    public float driftSpeed;
    public Vector3 direction;
    public float turnaroundTime = 8.0f;
    private float driftingTime = 0.0f;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        driftingTime += Time.deltaTime;
        //Debug.Log(driftSpeed * Time.deltaTime * direction);
        transform.Translate(driftSpeed * Time.deltaTime * direction);
        
        if (driftingTime > turnaroundTime) {
            direction = -direction;
            driftingTime = driftingTime -= turnaroundTime;
        }
    }
}
