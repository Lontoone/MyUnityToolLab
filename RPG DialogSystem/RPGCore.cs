using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
public class RPGCore
{
    public static Lang lang = Lang.EN;//語言 
    public const string DEFULT_SCENE_NAME = "GameScene";
    //應在遊戲關閉前儲存的主要進度
    public static Dictionary<string, string> StoryRecord = new Dictionary<string, string>(); //需要init

    //暫時的變數(生命週期應只有1個故事腳本，應在故事腳本讀完後呼叫 Clear_Temp_StoryRecord 清除)
    public static Dictionary<string, string> temp_StoryRecord = new Dictionary<string, string>(); //需要init
    public static readonly Dictionary<string, string> ESC_CHAR = new Dictionary<string, string>() //跳脫字元
    {
        {"\\<", "[@lt]"},{"\\>", "[@gt]"}
    };


    #region //=============REGEX=================
    //初始拆解格式
    public const string REGEX_SPLIT_LINES = @"
                                        (?:
                                            (?:
                                                <(?<tag>/?\w*)(?<arg>[^>]*)>
                                                (?<text>[^<>]*)?
                                            )|
                                            (?<-tag> </\k<tag>>)+
                                        )";
    //拆解參數屬性
    //動畫 (物件名稱,動畫名稱)
    public const string REGEX_ARGS_ANIMATION = @"animation\s*\(\s*
                                                ['"".]*
                                                (?<objectName>[\w\s]+)['"".]*,
                                                ['"".]*
                                                (?<clipName>[\w\s]+)['"".]*";

    //對話框立繪 (左中右邊?,圖片路徑)
    /*EXAMPLE
        img(left,"img1")
        img(middle,'img2')
        img(right,/imgfolder/img1)
    */
    public const string REGEX_ARGS_IMG = @"img\s*\(\s*
                                                ['"".]*
                                                (?<side>[\w\s]+)['"".]*,
                                                ['"".]*
                                                (?<imgPath>[^'""]+)['"".]*";

    //對話框名字欄位
    /*EXAMPLE
        title= "Lontoone, The first"
        title= Lontoone The first
    */
    public const string REGEX_ARGS_TITLE = @"title\s*=\s*['""]?(?<title>[^'""]+)['""]?";

    //用來尋找場景中的物件名稱，將UI放置在該物件旁
    /*EXAMPLE
        by="objname"
    */
    public const string REGEX_ARGS_BY = @"by\s*=\s*['""]?(?<by>[^'""]+)['""]?";

    //設定變數  ※注 key為temp_開頭的用temp dictionary處理
    /* EXAMPLE
        set(" temp_a"=1)
        set("temp_a"= "temp_b")
        set("temp_a"="temp_b" + "temp_a")
    ※用""包起變數
    */
    public const string REGEX_ARGS_SET = @"
                                        set\s*\(\s*
                                        ['"".]*
                                        (?<key>['""]+[\w\s]+['""]+)['""\s]*=\s*
                                        (?<value>['"".]*[\w\s]+['"".]*)\s*
                                        (?:
                                        \s*(?<operator>[-*+=]+)\s*
                                        (?<value2>['"".]*\w+['"".]*)
                                        )?";

    //音樂 audio(objectName, [play|pause|stop])
    public const string REGEX_ARGS_AUDIO = @"audio\s*\(\s*
                                                ['"".]*
                                                (?<objectName>[\w\s]+)['"".]*,
                                                ['"".]*
                                                (?<operation>[\w\s]+)['"".]*";

    //IF 處理 
    /*EXAMPLE:
    <if(temp_a ==1)(temp_a ==1)></if>
    <if(temp_a ==1)||(temp_a ==1)></if>
    <if(temp_a ==1)||(temp_a >="1") &&(temp=="bb")></if>
    <if(temp_a ==1)></if>
    */
    public const string REGEX_ARGS_IF = @"
                                        \([\s'""]*
                                        (?<key>[\w]*)[\s'""]*
                                        (?<operator>[<>!=]*)\s*
                                        (?<value>.*?)
                                        \s*\)
                                        \s*(?<conjunction>[|&]*)
                                        ";

    //取得自定義變數
    /*EXAMPLE: $[key ] */
    public const string REGEX_CUSTOM_VARIABLE = @"\$\[\s*(?<key>\w+)\s*\]";

    //EXTRA (自定義擴充): 方法名稱-參數(split by',')
    public const string REGEX_FUNC_PARA = @"(?<func>\w+)\s*
                                            \(
                                            (?<para>[^\)]+)
                                            \s*\)";

    #endregion

    //讀取文本
    public static List<Line> ReadLines(string data)
    {
        List<Line> _lines = new List<Line>();
        Regex regex = new Regex(REGEX_SPLIT_LINES,
                                            RegexOptions.IgnoreCase
                                           | RegexOptions.Compiled
                                           | RegexOptions.Singleline
                                           | RegexOptions.IgnorePatternWhitespace);


        //string data = Resources.Load<TextAsset>(path).text;
        Debug.Log("讀取文本: " + data);

        //取代跳脫字元
        for (int i = 0; i < ESC_CHAR.Count; i++)
        {
            data = data.Replace(ESC_CHAR.ElementAt(i).Key, ESC_CHAR.ElementAt(i).Value);

        }

        //拆解巢狀
        Match match = regex.Match(data);
        while (match.Success)
        {
            GroupCollection groups = match.Groups;
            Line _tempLine = new Line(groups["tag"].Value,
                                      groups["arg"].Value,
                                      groups["text"].Value);


            _lines.Add(_tempLine);
            match = match.NextMatch();
        }
        //紀錄各括號終點
        for (int i = 0; i < _lines.Count - 1; i++)
        {
            FindEndLine(_lines, _lines[i].tag, ref i);

        }
        return _lines;
    }

    //紀錄括號終點
    static void FindEndLine(List<Line> lines, string tag, ref int index)
    {
        for (int j = index + 1; j < lines.Count;)
        {
            if (!lines[j].tag.StartsWith("/"))
            {
                lines[j].startLine = index; //槽狀開始
                FindEndLine(lines, lines[j].tag, ref j);
                //底層搜尋完=>從底層最後一個的index+1開始
            }
            if (lines[j].tag == "/" + tag)
            {
                Debug.Log(index + lines[index].tag + " 配對 " + j + lines[j].tag);
                lines[index].endLine = j;
                index = j + 1;
                break;
            }
            //else{}
        }
    }


    //==============DICTIONARY==================
    //清空暫存變數
    public static void Clear_Temp_StoryRecord()
    {
        temp_StoryRecord.Clear();
    }
    //初始化字典
    public static void Dict_Init()
    {
        if (StoryRecord == null)
            StoryRecord = new Dictionary<string, string>();
        if (temp_StoryRecord == null)
            temp_StoryRecord = new Dictionary<string, string>();
    }
    //Set值
    public static void SetDictionaryValue(string key, string value)
    {

        //不存在=>建立
        //存在=>修改
        //  判斷temp或永久字典
        if (key.StartsWith("temp"))
        {
            if (temp_StoryRecord.ContainsKey(key))
                temp_StoryRecord[key] = value;
            else
                temp_StoryRecord.Add(key, value);
        }
        else
        {
            if (StoryRecord.ContainsKey(key))
                StoryRecord[key] = value;
            else
                StoryRecord.Add(key, value);
        }
        Debug.Log("設定 " + key + " " + value);
    }
    public static string GetDictValue(string key, out bool isExist)
    {
        string result = "";
        isExist = false;
        key = Regex.Replace(key, @"['""]", "");

        //判斷字典
        if (key.StartsWith("temp") && temp_StoryRecord.ContainsKey(key))
        {
            isExist = true;
            result = temp_StoryRecord[key];
        }

        else if (StoryRecord.ContainsKey(key))
        {
            isExist = true;
            result = StoryRecord[key];
        }

        Debug.Log("讀取 " + key + " " + result);

        return result;
    }
    public static string GetDictValue(string key)
    {
        bool _temp;
        return GetDictValue(key, out _temp);
    }

    //讀取自定義變數
    public static string ReadCustomVariables(string input)
    {
        string outPut = input;
        Regex regex = new Regex(REGEX_CUSTOM_VARIABLE,
                                           RegexOptions.IgnoreCase
                                          | RegexOptions.Compiled
                                          | RegexOptions.Singleline
                                          | RegexOptions.IgnorePatternWhitespace);
        Match match = regex.Match(outPut);
        while (match.Success)
        {
            GroupCollection groups = match.Groups;
            string _dict_key = groups["key"].Value;
            string _pattern = @"\$\[\s*" + _dict_key + @"\s*\]"; //臨時pattern
            Debug.Log("取代" + _dict_key + " with " + _pattern);
            outPut = Regex.Replace(outPut, _pattern, GetDictValue(_dict_key));
            match = match.NextMatch();
        }
        Debug.Log(outPut);
        return outPut;
    }

    public static bool if_compare(string arg)
    {
        bool result = false;
        Regex regex = new Regex(REGEX_ARGS_IF,
                                           RegexOptions.IgnoreCase
                                          | RegexOptions.Compiled
                                          | RegexOptions.Singleline
                                          | RegexOptions.IgnorePatternWhitespace);

        Match match = regex.Match(arg);
        bool _previousResult = false; //前一個比較結果
        while (match.Success)
        {
            GroupCollection groups = match.Groups;

            string _key = groups["key"].Value;
            string _operator = groups["operator"].Value;
            string _value = groups["value"].Value;
            string _conjunction = groups["conjunction"].Value;

            bool _current_result;

            //key變數
            bool _key_isExist;
            string _key_dict_value = GetDictValue(_key, out _key_isExist);

            //不存在的變數=>回傳false，跳過
            if (!_key_isExist)
            {
                _current_result = false;
                Debug.Log("<color=red> <if> 不存在的key" + _key + " </color>");
                match = match.NextMatch();
                continue;
            }

            //_value是變數還是純值?
            if (Regex.IsMatch(_value, @"['""][a-zA-Z]*['""]"))
            {
                //_value是變數
                bool _value_isExist;
                _value = GetDictValue(_value, out _value_isExist);

                int _v_number;
                bool is_value_num = int.TryParse(_value, out _v_number);

                //_value是文字變數 or 數字?
                if (is_value_num)
                    _current_result = Compare<int>(_operator, int.Parse(_key_dict_value), _v_number);
                else
                    _current_result = Compare<String>(_operator, _key_dict_value, GetDictValue(_value));

            }
            else
            {
                //_value是純值
                int _v_number;
                bool is_value_num = int.TryParse(_value, out _v_number);
                if (!is_value_num)
                {
                    //_value是文字 值
                    _current_result = Compare<String>(_operator, _key_dict_value, _value);
                }
                else
                {
                    //_value是數字 值
                    _current_result = Compare<int>(_operator, int.Parse(GetDictValue(_key)), _v_number);

                }
            }

            //直接跳出
            if (result == false && _current_result == false && (_conjunction == "" || _conjunction == "&&"))
            {
                result = false;
                break;
            }
            // true || current
            else if (_current_result == true && _conjunction == "||")
            {
                _previousResult = true;
                result = true;
                match = match.NextMatch();
            }
            //else if (_conjunction == "||")
            // false || current
            // true && current
            else
            {
                result = _current_result;
                _previousResult = _current_result;
            }
            match = match.NextMatch();
        }
        return result;
    }
    static bool Compare<T>(string op, T left, T right) where T : IComparable<T>
    {
        //ref:https://stackoverflow.com/questions/7086058/convert-string-value-to-operator-in-c-sharp
        switch (op)
        {
            case "<": return left.CompareTo(right) < 0;
            case ">": return left.CompareTo(right) > 0;
            case "<=": return left.CompareTo(right) <= 0;
            case ">=": return left.CompareTo(right) >= 0;
            case "==": return left.Equals(right);
            case "!=": return !left.Equals(right);
            default: throw new ArgumentException("Invalid comparison operator: {0}", op);
        }
    }

    [System.Serializable]
    public class Line
    {
        public Line(string _tag, string _args, string _text)
        {
            tag = _tag;
            args = _args;
            text = _text;
        }
        public string tag;
        public string args;
        public string text;
        public int endLine;
        public int startLine;
    }

    public enum Lang
    {
        EN, ZH
    };

    //放回跳脫字元
    public static string Put_Back_EscChar(string input)
    {
        string output = input;
        for (int i = 0; i < ESC_CHAR.Count; i++)
        {
            output = output.Replace(ESC_CHAR.ElementAt(i).Value, ESC_CHAR.ElementAt(i).Key.Substring(1));
        }
        return output;
    }


    //***********EXTRA**********  you can delete it if you want.//
    #region Scene
    public static IEnumerator LoadScene(string sceneName, GameObject playerToMove, string spawnPointName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!async.isDone)
            {
                Debug.Log("scene loading :" + async.progress);
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.1f);
        //傳送至指定物件位置
        Debug.Log(spawnPointName + " " + playerToMove);
        if (spawnPointName != "" && playerToMove != null)
        {
            Vector3 _targetPos = GameObject.Find(spawnPointName).transform.position;
            playerToMove.transform.position = _targetPos;
        }

    }

    public static IEnumerator UnloadScene(string unloadName, string exceptSceneName)
    {
        if (unloadName == "all")
        {
            //紀錄要刪除的scene
            Queue<Scene> scenesToUnload = new Queue<Scene>();
            int max_scene = SceneManager.sceneCount;
            for (int i = 0; i < max_scene; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                //不要移除的scene
                if (scene.name != exceptSceneName)
                    scenesToUnload.Enqueue(scene);
            }

            for (int i = 0; i < scenesToUnload.Count; i++)
            {
                string _deque_name = scenesToUnload.Dequeue().name;
                Debug.Log("deqeue " + _deque_name);
                SceneManager.UnloadSceneAsync(_deque_name);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            SceneManager.UnloadSceneAsync(unloadName);
        }
    }
    #endregion
}
