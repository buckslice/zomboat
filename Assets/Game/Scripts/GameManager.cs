using System.Collections;
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

public class GameManager : MonoBehaviour {
    public int playersToStart = 3;
    public float spawnRadius = 2.0f;
    public GameObject playerPrefab;
    bool gameStarted = false;

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

            }

            players[Random.Range(0, players.Count)].controller.BeginZombification();

            gameStarted = true;
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
