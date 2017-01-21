using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HappyFunTimes;
using UnityEngine.UI;

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
    public float spawnRadius = 0.5f;
    public GameObject playerPrefab;
    bool gameStarted = false;
    public float gameTime;
    public float countDownTime = 10.0f;
    float countTimer = 10.0f;
    public Text countDownText;
    public Text playerCountText;
    public Vector3[] humanSpawnPoints;

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
        playerCountText.text = "players " + players.Count;
    }

    // Use this for initialization
    void Start () {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("HumanSpawn");
        if (spawnPointObjects.Length == 0) {
            humanSpawnPoints = new Vector3[1];
            humanSpawnPoints[0] = new Vector3(0, 0, 0);
        } else {
            humanSpawnPoints = new Vector3[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; ++i) {
                Vector2 variation = Random.insideUnitCircle.normalized * spawnRadius;
                Vector3 actualPos = spawnPointObjects[i].transform.position;
                actualPos.x += variation.x;
                actualPos.y += variation.y;
                humanSpawnPoints[i] = actualPos;
            }
        }
    }

    void SpawnPlayers() {
        if(!gameStarted && players.Count >= playersToStart) {
            for(int i = 0; i < players.Count; ++i) {
                // initialize prefabs
                Vector2 pos = humanSpawnPoints[Random.Range(0, humanSpawnPoints.Length)];

            GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);

            PlayerHandle ph = players[i];
            ph.controller = go.GetComponent<PlayerController>();
            // this is a little stupid but ok
            ph.controller.gamepad.InitializeNetPlayer(ph.netPlayer);
            // give each player a random role
            ph.controller.role = (Role)Random.Range(0, System.Enum.GetValues(typeof(Role)).Length - 1);

        }

        // make one random player a zombie
        players[Random.Range(0, players.Count)].controller.BeginZombification();
    }

    void CheckWaves() {
        if (currentWave < WAVE_TIMES.Length && gameTime > WAVE_TIMES[currentWave]) {
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

    Coroutine countDownRoutine = null;
    void StartCountDown() {
        if (countDownRoutine == null) {
            countDownRoutine = StartCoroutine(CountDownRoutine());
        }
    }
    void StopCountDown() {
        if (countDownRoutine != null) {
            StopCoroutine(countDownRoutine);
            countDownText.enabled = false;
            countDownRoutine = null;
        }
    }
    void SkipCountDown() {
        StopCountDown();
    }
    WaitForSeconds waitOne = new WaitForSeconds(1.0f);
    IEnumerator CountDownRoutine() {
        countDownText.enabled = true;
        for (int i = 0; i < countDownTime; ++i) {
            countDownText.text = "" + (int)(countDownTime - i);
            yield return waitOne;
        }
        countDownText.enabled = false;
        countDownRoutine = null;

        SpawnPlayers();
        gameStarted = true;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(players.Count);
        if (!gameStarted) {
            if (Input.GetKeyDown(KeyCode.Space)) {  // start game instantly regardless of players
                StopCountDown();
                SpawnPlayers();
                gameStarted = true;
            }else if (players.Count >= playersToStart) {
                StartCountDown();
            } else {
                StopCountDown();
            }
        } else {
            gameTime += Time.deltaTime;

            CheckWaves();
        }

	}

    void EndGame() {
        // not sure how this will work yet
        // need to delete all player objects
        // send game over message to players
        // 
    }

    void OnPlayerDisconnected(object sender, System.EventArgs e) {
        NetPlayer np = (NetPlayer)sender;
        for(int i = 0; i < players.Count; ++i) {
            if(players[i].netPlayer == np) {
                players.RemoveAt(i);
                break;
            }
        }
        playerCountText.text = "players " + players.Count;
    }

}
