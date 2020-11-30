using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//每個UI代表的場上的口罩
public class GamePlay_UI_btn : MonoBehaviour
{
    //public float max_sp, sp;
    //public Mask mask;
    //public Image sp_fill;
    public int ui_sort_order; //UI欄位排序得順位
    public int anwserCode;//答案代碼

    //public GameObject thisMask_obj;
    //public Image ava_cg;

    //public Sprite die_pic;

    public GamePlay_UI_btn(string id)
    {
        //mask = GM.PlayerData.masks.Find(x => x.ID == id);
    }
    public void SetMask(Mask _mask)
    {
        //mask = _mask;
        Start();
    }

    private void Start()
    {
        /*
        if (mask != null)
        {
            Debug.Log(mask.current_sp);
            sp_fill.fillAmount = mask.current_sp / mask.max_sp;
            thisMask_obj = GameObject.Find(mask.ID);

            //設定頭像
            Sprite cg = Resources.Load<Sprite>("CG/" + mask.character_name.ToString());
            ava_cg.sprite = cg;
        }
        Mask_PlayControl.GetDamage_event += UpDataSP_fill_UI;*/
    }
    public void OnDestroy()
    {
        //Mask_PlayControl.GetDamage_event -= UpDataSP_fill_UI;
    }
    public void FixedUpdate()
    {
        //TODO:暫時放這裡
       
    }


    public void Die()
    {
        GamePlay_UIDragPanelControl gamePlay_UI=GameObject.FindObjectOfType<GamePlay_UIDragPanelControl>();
        gamePlay_UI.DiscardAtList(0); //刪掉第一個
        gamePlay_UI.ResetOrder();

    }

}
