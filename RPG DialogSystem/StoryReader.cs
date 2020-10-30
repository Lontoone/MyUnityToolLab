using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

//對話框操作
public class StoryReader : MonoBehaviour
{
    //==================對話框物件====================
    public Image left_portrait, right_portrait, middle_portrait;//立繪位置
    public Text dialogText, title; //對話框、標題欄
    public GameObject selectPanel, option_btn_prefab;//選單 , 選項

    public KeyCode next_line_key = KeyCode.Space;
    //public string path = "Story/test1";
    Coroutine isReading;
    public List<RPGCore.Line> Lines = new List<RPGCore.Line>();

    Coroutine isReadingTags;//讀取tag
    static string input = ""; // 輸入，每行重製
    int line_index = 1; //略開頭<story>
    void Start()
    {
        RPGCore.Dict_Init();

        //TEST
        StartConversation("Story/test1");
    }

    public void StartConversation(string text_file_path)
    {
        Lines = RPGCore.ReadLines(text_file_path);
        if (isReading == null)
            isReading = StartCoroutine(ReadEachLine_coro());
    }

    //讀取每行
    IEnumerator ReadEachLine_coro()
    {
        line_index = 1;
        while (line_index < Lines.Count - 1)//略結尾</story>
        {
            yield return new WaitForFixedUpdate();

            //對話框處理
            if (isReadingTags == null)
                UpdateDialog();

            yield return new WaitUntil(() => { return Input.GetKeyDown(next_line_key); });
        }

        Debug.Log("對話結束");
        //清空暫時紀錄
        RPGCore.Clear_Temp_StoryRecord();

        isReading = null;
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
            Debug.Log("跳脫巢狀" + line_index + " to " + Lines[line_index].endLine + 1);
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
            ReadArgs(Lines[line_index].args);
            ReadText(Lines[line_index].text);

            //line_index = Lines[line_index].endLine + 1; //下一句
            GoNextLine(Lines[line_index].endLine + 1);
        }

        //select 選單 內含opt數個
        else if (tag == "select")
        {
            //開啟選單
            if (selectPanel != null)
                selectPanel.gameObject.SetActive(true);

            ReadArgs(Lines[line_index].args);
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
                    //opt.name = (i - line_index).ToString();
                    opt.name = _opts.Count.ToString();
                    //設定text
                    opt.GetComponentInChildren<Text>().text = Lines[i].text;
                    Debug.Log(line_index + " " + "TAG " + tag + Lines[i].args + " " + Lines[i].text);

                }

                //ReadArgs(Lines[i].args);
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
            ReadArgs(_opts[int.Parse(input)].args);

            //從select結尾+1開始
            //line_index = Lines[select_start_index].endLine + 1;
            GoNextLine(Lines[select_start_index].endLine + 1);
            UpdateDialog();
        }


        //if 條件式
        //  true=> 跳至下一行
        //  false=> 跳至endline+1
        else if (tag == "if")
        {
            if (RPGCore.if_compare(Lines[line_index].args))
            {
                //line_index++;
                GoNextLine(line_index + 1);
            }
            else
            {
                //line_index = Lines[line_index].endLine + 1;
                GoNextLine(Lines[line_index].endLine + 1);
            }
            UpdateDialog();
        }


        else
        {
            Debug.LogError("未知tag " + tag);
        }

        yield return new WaitForFixedUpdate();
        isReadingTags = null;
    }

    //參數設定
    public void ReadArgs(string args)
    {
        if (args == "") { return; }

        //title
        foreach (Match match in GetMatch(args, RPGCore.REGEX_ARGS_TITLE))
        {
            title.text = match.Groups["title"].Value;
        }

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

    }

    //設定大頭照
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
            dialogText.text = text;

    }

    public static void ReturnInput(string _input)
    {
        input = _input;
    }
}
