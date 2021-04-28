using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    PhotonView pv;
    public float speed = 5;
    PlayerManager playerManager;

    public TextMeshProUGUI name_text;

    public void Start()
    {
        pv = gameObject.GetComponent<PhotonView>();
        //Manager產生player時，順便告訴player產生它的manager是誰
        //用尋找該view ID的PlayerManager
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();

        if (!pv.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>());
        }

        name_text.text = PhotonNetwork.NickName;
    }



    public void Update()
    {
        if (!pv.IsMine)
            return;
        transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed * Time.deltaTime;
    }
}
