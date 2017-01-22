using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public AudioClip[] sounds;
    public AudioClip[] ambience;
    public AudioSource source;
    public int numAmbience;
    public int numSounds;
    public static SoundManager instance = null;

    // Use this for initialization
    void Awake () {
        source = GetComponent<AudioSource>();
        numAmbience = ambience.Length;
        numSounds = sounds.Length;
        instance = this;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlaySound(int soundIndex) {
        source.PlayOneShot(sounds[soundIndex]);
    }

    public void PlayAmbience(int ambienceIndex) {
        source.PlayOneShot(ambience[ambienceIndex]);
    }
}
