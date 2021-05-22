using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using UnityEngine.SceneManagement;
using TMPro;
//對話框操作
public class StoryReader : MonoBehaviour
{
    //==================對話框物件====================
    public Image left_portrait, right_portrait, middle_portrait;//立繪位置
    //public Text dialogText, title; //對話框、標題欄
    public TextMeshProUGUI dialogText, title; //對話框、標題欄
    public GameObject selectPanel, option_btn_prefab;//選單 , 選項
    public GameObject Wrapper; //子物件
    public KeyCode next_line_key = KeyCode.Space;
    //public string path = "Story/test1";
    Coroutine isReading;
    public List<RPGCore.Line> Lines = new List<RPGCore.Line>();

    Coroutine isReadingTags;//讀取tag
    static string input = ""; // 輸入，每行重製
    int line_index = 1; //略開頭<story>
    bool forceEnd = false; //強制換行?


    public static List<GameObject> temp_preparedList = new List<GameObject>(); //會用到的gameObject清單

    void Start()
    {
        RPGCore.Dict_Init();
    }

    //準備物件清單 for enable /disable (obj1,obj2,obj3......)
    public void PrePareGameObject(string text)
    {
        //檢查暫存內容
        temp_preparedList.Remove(temp_preparedList.Find(x => x.Equals(null)));


        foreach (Match match in GetMatch(text, RPGCore.REGEX_FUNC_PARA))
        {
            //方法名稱
            string functionName = match.Groups["func"].Value;
            Debug.Log(functionName);
            string[] paras = match.Groups["para"].Value.Split(',');

            if (functionName.Equals("enable") || functionName.Equals("disable"))
            {
                foreach (string objName in paras)
                {
                    GameObject _findTarget = GameObject.Find(objName);
                    if (_findTarget != null && !temp_preparedList.Exists(x => x.name.Equals(objName)))
                        temp_preparedList.Add(_findTarget);
                }
            }

        }
    }

    public void StartConversation(string data, Action finish)
    {
        PrePareGameObject(data);
        //string path_with_lang = RPGCore.lang + "/" + text_file_path;

        //更新:改直接接收文檔內容
        Lines = RPGCore.ReadLines(data);
        if (isReading == null)
            isReading = StartCoroutine(ReadEachLine_coro(finish));
    }

    //讀取每行
    IEnumerator ReadEachLine_coro(Action finish)
    {
        yield return new WaitForEndOfFrame();
        ReadArgs(ref Lines[0].args); //story's setting
        line_index = 1;
        while (line_index < Lines.Count - 1)//略結尾</story>
        {
            yield return new WaitForFixedUpdate();

            //對話框處理
            if (isReadingTags == null)
                UpdateDialog();

            yield return new WaitUntil(() => { return Input.GetKeyDown(next_line_key) || forceEnd; });
            forceEnd = false;
        }

        Debug.Log("對話結束");
        //清空暫時紀錄
        RPGCore.Clear_Temp_StoryRecord();


        isReading = null;
        finish();

        Destroy(gameObject);

    }

    //下句台詞更新對話框面板
    public void UpdateDialog()
    {
        #region  關閉視窗
        if (dialogText != null)
            dialogText.gameObject.SetActive(false);

        if (selectPanel != null)
            selectPanel.gameObject.SetActive(false);

        if (left_portrait != null)
            left_portrait.gameObject.SetActive(false);

        if (right_portrait != null)
            right_portrait.gameObject.SetActive(false);

        if (middle_portrait != null)
            middle_portrait.gameObject.SetActive(false);
        #endregion

        //重置----
        input = "";

        //特效?

        //=============使用文本設定:==================
        isReadingTags = StartCoroutine(ReadTags(Lines[line_index].tag));


    }

    public void GoNextLine(int goalLine)
    {
        //檢查要走的line是否比startLine小
        if (Lines[goalLine].endLine < Lines[line_index].startLine)
        {
            //  是=>跳脫上一層的巢狀
            Debug.Log("跳脫巢狀" + line_index + " to " + (Lines[line_index].endLine + 1));
            line_index = Lines[Lines[line_index].startLine].endLine + 1;
        }
        else
        {
            //  否=>設成goalLine
            line_index = goalLine;
        }
    }
    //tag設定
    public IEnumerator ReadTags(string tag)
    {
        Debug.Log(line_index + " " + "TAG " + tag + Lines[line_index].args + " " + Lines[line_index].text);

        //l=line 基本單行對話
        if (tag == "l")
        {
            //開啟對話框
            if (dialogText != null)
                dialogText.gameObject.SetActive(true);
            ReadArgs(ref Lines[line_index].args);
            ReadText(Lines[line_index].text);

            //line_index = Lines[line_index].endLine + 1; //下一句
            GoNextLine(Lines[line_index].endLine + 1);
        }

        //select 選單 內含opt數個
        else if (tag == "select")
        {
            //開啟選單
            if (selectPanel != null)
            {
                selectPanel.gameObject.SetActive(true);
                //清除舊選項
                foreach (Transform child in selectPanel.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            ReadArgs(ref Lines[line_index].args);
            ReadText(Lines[line_index].text);

            //select開始的index
            int select_start_index = line_index;
            line_index++;

            //在選單創建選項 (tag=opt)
            Debug.Log("select end line " + Lines[select_start_index].endLine);
            List<RPGCore.Line> _opts = new List<RPGCore.Line>();
            for (int i = line_index; i < Lines[select_start_index].endLine;)
            {
                if (option_btn_prefab != null)
                {
                    GameObject opt = Instantiate(option_btn_prefab, selectPanel.transform.position, Quaternion.identity, selectPanel.transform);
                    
                    //Read Text:
                    string _opt_text = RPGCore.Put_Back_EscChar(Lines[i].text);
                    _opt_text = RPGCore.ReadCustomVariables(_opt_text);                    

                    opt.name = _opts.Count.ToString();
                    //設定text
                    //opt.GetComponentInChildren<Text>().text = Lines[i].text;
                    //opt.GetComponentInChildren<TextMeshProUGUI>().text = Lines[i].text;
                    opt.GetComponentInChildren<TextMeshProUGUI>().text = _opt_text;
                    
                    Debug.Log(line_index + " " + "TAG " + tag + Lines[i].args + " " + Lines[i].text);

                }

                _opts.Add(Lines[i]);

                i = Lines[i].endLine + 1;
            }

            //等待選擇:
            while (input == "")
            {
                yield return new WaitForFixedUpdate();
            }
            Debug.Log("<color=green> 選擇: " + input + " " + _opts[int.Parse(input)].args + "</color>");

            //使用選擇的結果
            ReadArgs(ref _opts[int.Parse(input)].args);

            //從select結尾+1開始
            GoNextLine(Lines[select_start_index].endLine + 1);
            UpdateDialog();
        }

        //if 條件式
        //  true=> 跳至下一行
        //  false=> 跳至endline+1
        else if (tag == "if")
        {
            ReadArgs(ref Lines[line_index].args);
            if (RPGCore.if_compare(Lines[line_index].args))
            {
                GoNextLine(line_index + 1);
            }
            else
            {
                GoNextLine(Lines[line_index].endLine + 1);
            }
            UpdateDialog();
        }
        else if (tag.StartsWith("/")) //FOR:[bug] if、select結尾後沒有其他段落會直接跳到story tag後解讀失敗
        {

            forceEnd = true;
        }
        else
        {
            Debug.LogError("未知tag " + tag);
        }

        yield return new WaitForFixedUpdate();
        isReadingTags = null;
    }

    //參數設定
    public void ReadArgs(ref string args)
    {
        if (args == "") { return; }

        //跳脫字元
        args = RPGCore.Put_Back_EscChar(args);
        //自定義變數
        args = RPGCore.ReadCustomVariables(args);

        //title
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_TITLE))
        {
            title.text = match.Groups["title"].Value;
        }

        //by
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_BY))
        {
            //找到說話的物件，計算位置，將UI放置在該物件旁 (UI座標)
            GameObject talk_by = GameObject.Find(match.Groups["by"].Value);
            if (talk_by != null)
            {
                Vector3 target_top_pos = talk_by.transform.position;

                //在物件上方
                Renderer rend = talk_by.GetComponent<Renderer>();
                if (rend != null)
                    target_top_pos.y += rend.bounds.size.y * 1.5f;
                else
                    target_top_pos.y += 2.5f;

                Vector2 ui_pos = Camera.main.WorldToScreenPoint(target_top_pos);

                //ui_pos.y+=100;

                Wrapper.transform.position = ui_pos;
            }
            else
            {
                Debug.LogError("talk by not found.");
            }
        }

        #region 

        //img
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_IMG))
        {
            SetPortraitImg(match.Groups["side"].Value, match.Groups["imgPath"].Value);
        }

        //animation
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_ANIMATION))
        {
            GameObject target = GameObject.Find((match.Groups["objectName"].Value));
            if (target != null)
            {
                Animator animator = target.GetComponent<Animator>();
                if (animator != null)
                    animator.Play((match.Groups["clipName"].Value));
                else
                    Debug.LogError("<animation arg error> animator not found");
            }
        }

        //audio
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_AUDIO))
        {
            string operation = (match.Groups["operation"].Value);
            GameObject target = GameObject.Find((match.Groups["objectName"].Value));
            if (target != null)
            {
                AudioSource audioSource = target.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    if (operation == "play")
                        audioSource.Play();
                    else if (operation == "pause")
                        audioSource.Pause();
                    else if (operation == "stop")
                        audioSource.Stop();
                }
                else
                    Debug.LogError("<audio arg error> audioSource not found");
            }
        }

        //set
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_SET))
        {
            Debug.Log("SET");
            string _key = Regex.Replace(match.Groups["key"].Value, @"['""]", ""); ;
            string _value = match.Groups["value"].Value;
            string _operator = match.Groups["operator"].Value; //optional
            string _value2 = match.Groups["value2"].Value; //optional

            //判斷value是字典key或值
            bool v1_is_key = Regex.IsMatch(_value, @"['""].*?['""]");
            bool v2_is_key = Regex.IsMatch(_value2, @"['""].*?['""]");

            string v1 = _value, v2 = _value2;
            if (v1_is_key)
                v1 = RPGCore.GetDictValue(Regex.Replace(_value, @"['""]", ""));
            if (v2_is_key)
                v2 = RPGCore.GetDictValue(Regex.Replace(_value2, @"['""]", ""));
            //先做運算
            if (_operator != "")
            {
                int v1_num = int.Parse(v1);
                int v2_num = int.Parse(v2);
                if (_operator == "+")
                    RPGCore.SetDictionaryValue(_key, (v1_num + v2_num).ToString());
                if (_operator == "-")
                    RPGCore.SetDictionaryValue(_key, (v1_num - v2_num).ToString());
                if (_operator == "*")
                    RPGCore.SetDictionaryValue(_key, (v1_num * v2_num).ToString());
                if (_operator == "/")
                    RPGCore.SetDictionaryValue(_key, (v1_num / v2_num).ToString());
            }
            else
            {
                //不做運算直接給值
                RPGCore.SetDictionaryValue(_key, v1);
            }

        }


        //其他 用括號的參數改用統一regex拆解 other
        foreach (Match match in GetMatch(args, RPGCore.REGEX_FUNC_PARA))
        {
            //方法名稱
            string functionName = match.Groups["func"].Value;
            Debug.Log(functionName);
            string[] paras = match.Groups["para"].Value.Split(',');
            //enable /disable
            if (functionName == "enable")
            {
                foreach (string objName in paras)
                {
                    GameObject target = temp_preparedList.Find(x => x.name.Equals(objName));
                    if (target != null)
                        target.SetActive(true);
                }
            }
            if (functionName == "disable")
            {
                foreach (string objName in paras)
                {
                    GameObject target = temp_preparedList.Find(x => x.name.Equals(objName));
                    if (target != null)
                        target.SetActive(false);
                }
            }

            //**************** YOUR OWN SCRIPT FUNCTION *****************************
            //changeScene(scenetoLoad ) 換場景
            if (functionName == "changeScene")
            {
                //load場景
                string scene_to_load = paras[0];
                SceneManager.LoadScene(scene_to_load);
            }

        }
        #endregion
    }

    void SetPortraitImg(string[] paras)
    {
        string side = paras[0];
        string path = paras[1];
        string effect = (paras.Length > 2) ? paras[2] : "";
        SetPortraitImg(side, path);
    }
    //設定大頭照 dialog image
    void SetPortraitImg(string side, string imgPath)
    {
        Sprite pic = Resources.Load<Sprite>(imgPath);
        Debug.Log(imgPath);
        if (side == "right" && right_portrait != null)
        {
            right_portrait.gameObject.SetActive(true);
            right_portrait.sprite = pic;
        }

        else if (side == "middle" && right_portrait != null)
        {
            middle_portrait.gameObject.SetActive(true);
            middle_portrait.sprite = pic;
        }

        else if (side == "left" && right_portrait != null)
        {
            left_portrait.gameObject.SetActive(true);
            left_portrait.sprite = pic;
        }

    }
    MatchCollection GetMatch(string arg, string pat)
    {
        Regex regex = new Regex(pat,
                                RegexOptions.IgnoreCase
                                | RegexOptions.Compiled
                                | RegexOptions.Singleline
                                | RegexOptions.IgnorePatternWhitespace);
        MatchCollection match = regex.Matches(arg);
        return match;
    }


    public void ReadText(string text)
    {
        if (dialogText != null)
        { 
            //放回跳脫字元 put back Escape character
            text = RPGCore.Put_Back_EscChar(text);
            //自定義變數 custom variables
            text = RPGCore.ReadCustomVariables(text);

            dialogText.text = text;
        }

    }

    //按鈕回傳 select button return
    public static void ReturnInput(string _input)
    {
        input = _input;
    }

  
}
