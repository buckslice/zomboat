using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveLine : MonoBehaviour {
    public float strength;
    public float intensity = 1.0f;
    public Vector3 direction = new Vector3(0, 1, 0);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    	
	}

    void OnTriggerStay2D(Collider2D other) {
        Vector3 translation = direction;
        translation.y = Time.deltaTime * intensity;
        Movable otherMovable = other.GetComponent<Movable>();
        if (otherMovable && strength > otherMovable.stubborness) {
            other.transform.Translate(translation / otherMovable.weight, Space.World);
        }

        //other.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, intensity * Time.deltaTime));
    }
}
