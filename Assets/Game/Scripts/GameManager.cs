using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HappyFunTimes;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHandle {
    public NetPlayer netPlayer;
    public PlayerController controller;

    public PlayerHandle(NetPlayer netPlayer, PlayerController controller) {
        this.netPlayer = netPlayer;
        this.controller = controller;
    }
}

public struct WaveObjectEntry {
    public WaveObjectEntry(int _waveIndex, CollisionObject _waveObject) {
        waveIndex = _waveIndex;
        waveObject = _waveObject;
    }

    public int waveIndex;
    public CollisionObject waveObject;
}

public class GameManager : MonoBehaviour {
    public float[] WAVE_TIMES = { 15, 30 };
    public float[] WAVE_INTENSITY = { 1.0f, 1.0f, 1.0f };

    public int currentWave = 0;
    public float waveCrash = 0.0f;
    public int playersToStart = 3;
    public float spawnRadius = 2.0f;
    public GameObject playerPrefab;
    bool gameStarted = false;
    public float curTime;
    public float winTimeSeconds = 180.0f;
    public float countDownTime = 10.0f;
    Text timerText;
    Text centerText;
    Text playerCountText;
    Image splash;
    public Sprite winSplash;
    public Sprite loseSplash;
    Vector3[] humanSpawnPoints;
    public AudioClip lobbyClip;
    public AudioClip introClip;
    public AudioClip gameClip;
    public AudioClip shotgunClip;
    AudioSource source;

    public List<WaveObjectEntry> waveObjects = new List<WaveObjectEntry>();
    private List<PlayerHandle> players = new List<PlayerHandle>();

    // crappy singleton
    public static GameManager instance = null;
    void Awake() {
        source = GetComponent<AudioSource>();
        ResetVariables();
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        UpdateGameTimerText();

        DontDestroyOnLoad(transform.gameObject);    // keep the manager alive
    }

    private void OnLevelWasLoaded(int level) {
        ResetVariables();
    }

    void ResetVariables() {
        source.clip = lobbyClip;
        source.loop = true;
        source.Play();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        centerText = GameObject.Find("CenterText").GetComponent<Text>();
        playerCountText = GameObject.Find("PlayerCountText").GetComponent<Text>();
        playerCountText.text = "players " + players.Count;  // shows number of connected players
        splash = GameObject.Find("SplashScreen").GetComponent<Image>();
        gameStarted = false;
        curTime = 0.0f;
    }

    public void RegisterNetPlayer(NetPlayer np) {
        PlayerHandle ph = new PlayerHandle(np, null);
        np.OnDisconnect += OnPlayerDisconnected;
        players.Add(ph);
        playerCountText.text = "players " + players.Count;  // shows number of connected players
    }

    // Use this for initialization
    void Start() {
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

    void SpawnPlayer(PlayerHandle ph) {
        // initialize prefabs
        Vector2 pos = humanSpawnPoints[Random.Range(0, humanSpawnPoints.Length)];

        GameObject go = Instantiate(playerPrefab, pos, Quaternion.identity);

        ph.controller = go.GetComponent<PlayerController>();
        // this is a little stupid but ok
        ph.controller.gamepad.InitializeNetPlayer(ph.netPlayer);
        // give each player a random role
        ph.controller.role = (Role)Random.Range(0, (int)Role.COUNT - 2);
        ph.controller.gamepad.SendRole(ph.controller.role);
        ph.controller.SetCanMove(false);
        ph.controller.SetZombie(false);
    }

    void SetPlayersCanMove(bool canMove) {    // set all players movement
        for (int i = 0; i < players.Count; ++i) {
            players[i].controller.SetCanMove(canMove);
        }
    }

    void CheckWaves() {
        if (currentWave < WAVE_TIMES.Length && curTime > WAVE_TIMES[currentWave]) {
            /*
            List<WaveObjectEntry> remainingEntries = new List<WaveObjectEntry>();
            foreach (WaveObjectEntry entry in waveObjects) {
                if (entry.waveIndex == currentWave) {
                    entry.waveObject.HandleWave();
                } else {
                    remainingEntries.Add(entry);
                }
            }

            waveObjects = remainingEntries;
            */
            currentWave = currentWave + 1;
        }

        if (currentWave < WAVE_INTENSITY.Length && waveCrash >= WAVE_INTENSITY[currentWave]) {
            foreach (WaveObjectEntry entry in waveObjects) {
                if (entry.waveIndex == currentWave) {
                    entry.waveObject.HandleWave();
                    waveObjects.Remove(entry);
                    break;
                }
            }
        }
    }

    Coroutine countDownRoutine = null;
    void StartCountDown() {
        if (countDownRoutine == null) {
            countDownRoutine = StartCoroutine(CountDownRoutine());
        }
    }
    WaitForSeconds waitOne = new WaitForSeconds(1.0f);
    IEnumerator CountDownRoutine() {
        centerText.enabled = true;
        source.Stop();
        source.clip = introClip;
        source.loop = false;
        source.Play();
        for (int i = 0; i < countDownTime; ++i) {
            centerText.text = "" + (int)(countDownTime - i);
            yield return waitOne;
        }
        centerText.text = "GO";
        StartGame();
        centerText.enabled = true;
        source.clip = shotgunClip;
        source.Play();
        yield return waitOne;
        centerText.enabled = false;
        source.clip = gameClip;
        source.loop = true;
        source.Play();
    }

    void StartGame() {
        centerText.enabled = false;
        countDownRoutine = null;
        gameStarted = true;
        SetPlayersCanMove(true);

        Invoke("ZombifySomeone", 3.0f);
    }

    // checks to see which players are actually in game (with controllers)
    void ZombifySomeone() {
        List<PlayerController> activePlayers = new List<PlayerController>();
        for (int i = 0; i < players.Count; ++i) {
            if (players[i].controller) {
                activePlayers.Add(players[i].controller);
            }
        }
        // make one player a zombie
        activePlayers[Random.Range(0, activePlayers.Count)].BeginZombification();
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log(players.Count);
        if (!gameStarted) {
            for(int i = 0; i < players.Count; ++i) {
                if(players[i].controller == null) {
                    SpawnPlayer(players[i]);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                if (countDownRoutine == null) {
                    StartCountDown();
                } else {
                    if (countDownRoutine != null) { // stop countdown
                        StopCoroutine(countDownRoutine);
                    }
                    StartGame();
                }
            }
        } else {
            curTime += Time.deltaTime;
            UpdateGameTimerText();
            CheckWaves();

            if (curTime >= winTimeSeconds) {
                splash.sprite = winSplash;
                splash.enabled = true;
                ResetGame();
            } else if (OnlyZombiesLeft()) {
                splash.sprite = loseSplash;
                splash.enabled = true;
                ResetGame();
            }
        }
    }

    bool reseting = false;
    void ResetGame() {
        if (reseting) {
            return;
        }
        reseting = true;
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine() {
        float t = 0.0f;
        while(t < 10.0f) {
            t += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space)) {
                break;
            }
            yield return null;
        }
        for(int i = 0; i < players.Count; ++i) {
            players[i].controller = null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);   // reload scene
        reseting = false;
    }

    bool OnlyZombiesLeft() {
        for (int i = 0; i < players.Count; ++i) {
            if (players[i].controller.alive) {
                return false;
            }
        }
        return true;
    }

    void UpdateGameTimerText() {
        int t = (int)(winTimeSeconds - curTime);
        string color = "<color=#FFFFFFFF>";
        if (t <= 10) {
            if ((int)((winTimeSeconds - curTime) * 2.0f) % 2 == 0) {
                color = "<color=#FF0000FF>";
            }
        } else if (t <= 30) {
            color = "<color=#FF0000FF>";
        } else if (t <= 60) {
            color = "<color=#FFFF00FF>";
        }
        string txt = string.Format("{0}:{1:00}", t / 60, t % 60);
        timerText.text = color + txt + "</color>";
    }

    void EndGame() {
        // not sure how this will work yet
        // need to delete all player objects
        // send game over message to players
        // 
    }

    void OnPlayerDisconnected(object sender, System.EventArgs e) {
        NetPlayer np = (NetPlayer)sender;
        for (int i = 0; i < players.Count; ++i) {
            if (players[i].netPlayer == np) {
                players.RemoveAt(i);
                break;
            }
        }
        playerCountText.text = "players " + players.Count;
    }

}
