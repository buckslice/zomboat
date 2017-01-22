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

public class GameManager : MonoBehaviour {
    public int currentWave = 0;
    public float waveCrash = 0.0f;
    public int playersToStart = 3;
    public float spawnRadius = 1.0f;
    public GameObject playerPrefab;
    public GameObject foodPrefab;
    public Sprite[] foodSprites;
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
    public Sprite intro1;
    public Sprite intro2;
    public Sprite intro3;
    int introSequence = 1;
    Vector3[] humanSpawnPoints;
    public AudioClip lobbyClip;
    public AudioClip introClip;
    public AudioClip gameClip;
    public AudioClip shotgunClip;
    public AudioClip humanWinClip;
    AudioSource source;
    AudioSource ambience;
    WaveSpawner waves;
    SoundManager soundManager;
    public float baseAmbienceFrequency = 10.0f;
    public float chompFrequency;
    float ambienceFrequency;
    public float ambienceSpeedUp = 5.0f;
    float timeUntilAmbience = 15.0f;

    List<PlayerHandle> players = new List<PlayerHandle>();
    bool introGoing = false;

    // crappy singleton
    public static GameManager instance = null;
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
        ResetVariables();

        UpdateGameTimerText();
        DontDestroyOnLoad(transform.gameObject);    // keep the manager alive
    }

    public void RegisterNetPlayer(NetPlayer np) {
        PlayerHandle ph = new PlayerHandle(np, null);
        np.OnDisconnect += OnPlayerDisconnected;
        np.SendCmd("wait");
        players.Add(ph);
    }

    private void OnLevelWasLoaded(int level) {  // not sure what the non deprecated version of this is
        ResetVariables();
    }

    void ResetVariables() {
        source = GetComponent<AudioSource>();
        source.pitch = 1.0f;
        ambience = GameObject.Find("AmbienceSource").GetComponent<AudioSource>();
        timerText = GameObject.Find("TimerText").GetComponent<Text>();
        centerText = GameObject.Find("CenterText").GetComponent<Text>();
        playerCountText = GameObject.Find("PlayerCountText").GetComponent<Text>();
        splash = GameObject.Find("SplashScreen").GetComponent<Image>();
        waves = FindObjectOfType<WaveSpawner>();
        waves.enabled = false;
        gameStarted = false;
        curTime = 0.0f;
        introSequence = 1;
        introGoing = true;
        splash.enabled = true;
        splash.sprite = intro1;
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        ambienceFrequency = baseAmbienceFrequency;
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
                Vector3 actualPos = spawnPointObjects[i].transform.position;
                humanSpawnPoints[i] = actualPos;
            }
        }
    }

    void SpawnPlayer(PlayerHandle ph) {
        // initialize prefabs
        Vector2 pos = humanSpawnPoints[Random.Range(0, humanSpawnPoints.Length)];
        pos.x += Random.Range(-spawnRadius, spawnRadius);
        pos.y += Random.Range(-spawnRadius, spawnRadius);
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

    Coroutine countDownRoutine = null;
    void StartCountDown() {
        if (countDownRoutine == null) {
            countDownRoutine = StartCoroutine(CountDownRoutine());
        }
    }
    WaitForSeconds waitOne = new WaitForSeconds(1.0f);
    IEnumerator CountDownRoutine() {
        source.pitch = 1.0f;
        centerText.enabled = true;
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
        waves.enabled = true;

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
        playerCountText.text = "players: " + players.Count;  // shows number of connected players

        if (introGoing) {
            ambience.Stop();
            if (Input.GetKeyDown(KeyCode.Space)) {
                introSequence++;
                if (introSequence == 2) {
                    splash.sprite = intro2;
                } else if (introSequence == 3) {
                    splash.sprite = intro3;
                    source.clip = lobbyClip;
                    source.loop = true;
                    source.Play();
                } else {
                    splash.enabled = false;
                    introGoing = false;
                    ambience.Play();
                    Debug.Log("Playing ambience");
                }
            }
            return;
        }

        if (!gameStarted) {
            for (int i = 0; i < players.Count; ++i) {
                if (players[i].controller == null) {
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
                    source.clip = gameClip;
                    source.loop = true;
                    source.Play();
                }
            }
        } else if(!reseting){
            timeUntilAmbience -= Random.Range(0, Time.deltaTime * 2.0f);
            if (timeUntilAmbience <= 0.0f) {
                ambienceFrequency = baseAmbienceFrequency - (curTime / winTimeSeconds) * ambienceSpeedUp;
                Debug.Log(ambienceFrequency);
                timeUntilAmbience += ambienceFrequency;
                soundManager.PlayAmbience(Random.Range(0, soundManager.numAmbience));
            }

            curTime += Time.deltaTime;
            UpdateGameTimerText();

            source.pitch = 1.0f + Mathf.Lerp(0.5f, 0.0f, (winTimeSeconds - curTime) / 30.0f);

            if (curTime >= winTimeSeconds - 1.0f) {
                splash.sprite = winSplash;
                splash.enabled = true;
                reseting = true;
                StartCoroutine(ResetRoutine(true));
            } else if (OnlyZombiesLeft()) {
                splash.sprite = loseSplash;
                splash.enabled = true;
                reseting = true;
                StartCoroutine(ResetRoutine(false));
            }
        }
    }
    bool reseting = false;

    IEnumerator ResetRoutine(bool humansWin) {
        source.pitch = 1.0f;
        if (humansWin) {
            source.clip = humanWinClip;
            source.Play();
        }
        float t = 0.0f;
        while (t < 10.0f) {
            t += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space)) {
                break;
            }
            yield return null;
        }
        for (int i = 0; i < players.Count; ++i) {
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

    void OnPlayerDisconnected(object sender, System.EventArgs e) {
        NetPlayer np = (NetPlayer)sender;
        for (int i = 0; i < players.Count; ++i) {
            if (players[i].netPlayer == np) {
                players.RemoveAt(i);
                break;
            }
        }
    }

}
