using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedPack : MonoBehaviour {
    BoxCollider2D bc;
    public GameObject glowCircle;
    public float delay = 3.0f;
	// Use this for initialization
	void Start () {
        bc = GetComponent<BoxCollider2D>();
        StartCoroutine("Spawn");
	}
	
    IEnumerator Spawn() {
        yield return new WaitForSeconds(delay);
        bc.enabled = true;
        glowCircle.SetActive(true);
    }
}
