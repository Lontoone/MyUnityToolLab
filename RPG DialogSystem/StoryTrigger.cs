using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
public class StoryTrigger : MonoBehaviour
{
    public bool lock_player_control = true;
    public StoryReader reader_prefab;
    public TextAsset text;
    public string rootFolider = "Story/"; //resources資料夾下的目錄 //語言包之上
    public GameObject hint;
    public bool onlyOnce;//只觸發一次?
    public bool autoTrigger; //自動觸發?
    public Collider2D conversation_trigger_range;
    bool isTalking = false;
    public KeyCode start_key = KeyCode.E;
    GameObject player;
    StoryReader reader;


    private void Start()
    {
        player = FindObjectOfType<PlayerControl>().gameObject;
    }

    private void Update()
    {
        //玩家靠近出現提示

        Collider2D[] colliders = new Collider2D[10];
        ContactFilter2D contactFilter = new ContactFilter2D();
        
        contactFilter.SetLayerMask(LayerMask.GetMask("Player"));

        contactFilter.useTriggers = true;
        int num = conversation_trigger_range.OverlapCollider(contactFilter, colliders);
        if (num > 0 && !isTalking)
        {
            Debug.Log(colliders[0].gameObject.name);
            if (hint != null)
                hint.SetActive(true);
            //開始對話
            if (Input.GetKeyDown(start_key) || autoTrigger)
            {
                isTalking = true;
				/* How you deal with player control while dialog start: EXAMPLE
                if (lock_player_control)
                {
                    player.GetComponent<PlayerControl>().enabled = false;
                }
				*/

                //創建UI
                reader = Instantiate(reader_prefab, Vector3.zero, Quaternion.identity);

                //在資料夾中尋找//TODO:該語言包無此文檔的處理
                var textAssets = Resources.LoadAll<TextAsset>(rootFolider + RPGCore.lang.ToString());
                reader.StartConversation(textAssets.Single(s => s.name.Equals(text.name)).text, FinishCallBack);
                //reader.StartConversation(text_data_path, FinishCallBack);
            }
        }
        else
        {
            if (hint != null)
                hint.SetActive(false);
        }
    }

    //完成對話後觸發:
    void FinishCallBack()
    {
        isTalking = false;
        if (onlyOnce)
        {
            Destroy(this);
        }
		
		/*
        if (lock_player_control)
        {
            player.GetComponent<PlayerControl>().enabled = true;
        }
		*/
    }
}
