﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HappyFunTimes;

public class PlayerHandle {
    public NetPlayer netPlayer;
    public PlayerController controller;

    public PlayerHandle(NetPlayer netPlayer, PlayerController controller) {
        this.netPlayer = netPlayer;
        this.controller = controller;
    }
}

public struct WaveObjectEntry {
    public WaveObjectEntry(int _waveIndex, CollisionObject _waveObject)
    {
        waveIndex = _waveIndex;
        waveObject = _waveObject;
    }

    public int waveIndex;
    public CollisionObject waveObject;
}

public class GameManager : MonoBehaviour {
    public float[] WAVE_TIMES = { 15, 30 };

    public int currentWave = 0;
    public int playersToStart = 3;
    public float spawnRadius = 2.0f;
    public GameObject playerPrefab;
    bool gameStarted = false;
    public float gameTime;

    public List<WaveObjectEntry> waveObjects = new List<WaveObjectEntry>();

    private List<PlayerHandle> players = new List<PlayerHandle>();
    

    // crappy singleton
    public static GameManager instance = null;
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void RegisterNetPlayer(NetPlayer np) {
        players.Add(new PlayerHandle(np, null));
        np.OnDisconnect += OnPlayerDisconnected;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(players.Count);

        if(!gameStarted && players.Count >= playersToStart) {
            for(int i = 0; i < players.Count; ++i) {
                // initialize prefabs
                Vector2 pos = Random.insideUnitCircle.normalized * spawnRadius;

                GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);

                PlayerHandle ph = players[i];
                ph.controller = go.GetComponent<PlayerController>();
                // this is a little stupid but ok
                ph.controller.gamepad.InitializeNetPlayer(ph.netPlayer);
                // give each player a random role
                ph.controller.role = (Role)Random.Range(0, System.Enum.GetValues(typeof(Role)).Length - 1);

            }

            players[Random.Range(0, players.Count)].controller.BeginZombification();

            gameStarted = true;
        }

        if(gameStarted) {
            gameTime += Time.deltaTime;

            if(currentWave < WAVE_TIMES.Length && gameTime > WAVE_TIMES[currentWave]) {
                List<WaveObjectEntry> remainingEntries = new List<WaveObjectEntry>();
                foreach (WaveObjectEntry entry in waveObjects) {
                    if (entry.waveIndex == currentWave) {
                        entry.waveObject.HandleWave();
                    } else {
                        remainingEntries.Add(entry);
                    }
                }

                waveObjects = remainingEntries;
                currentWave = currentWave + 1;
            }
        }
	}

    void OnPlayerDisconnected(object sender, System.EventArgs e) {
        NetPlayer np = (NetPlayer)sender;
        for(int i = 0; i < players.Count; ++i) {
            if(players[i].netPlayer == np) {
                players.RemoveAt(i);
                break;
            }
        }
    }

}
