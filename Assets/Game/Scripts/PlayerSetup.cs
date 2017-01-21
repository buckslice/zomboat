using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HappyFunTimes;

public class PlayerSetup : MonoBehaviour {

    void InitializeNetPlayer(SpawnInfo spawnInfo) {
        GameManager.instance.RegisterNetPlayer(spawnInfo.netPlayer);
        Destroy(gameObject);
    }

}
