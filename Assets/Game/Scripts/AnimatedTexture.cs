using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedTexture : MonoBehaviour {
    public Sprite[] textures;
    public float animationFrequency = 0.0f;
    private float animationTime = 0.0f;
    private int currentTexture = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (textures.Length == 0) { GameObject.Destroy(gameObject); return; }

        animationTime += Time.deltaTime;
        if (animationTime > animationFrequency) {
            animationTime -= animationFrequency;
            currentTexture = (currentTexture + 1) % textures.Length;
            gameObject.GetComponent<SpriteRenderer>().sprite = textures[currentTexture];
        }
	}
}
