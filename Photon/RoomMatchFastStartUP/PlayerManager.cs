using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
public class PlayerManager : MonoBehaviour
{


    PhotonView pv;
    GameObject controller;

    public void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (pv.IsMine)
        {
            CreateController();
        }
    }

    void CreateController()
    {
        Debug.Log("Instantiated Player Controller");
        //Transform spawnPoint = SpawnManager.instance.GetSpawnpoint();
        //controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation, 0, new object[] { pv.ViewID });
        controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), Vector2.zero, Quaternion.identity, 0, new object[] { pv.ViewID });
    }

    public void Die()
    {
        PhotonNetwork.Destroy(controller);
        CreateController();
    }
}
