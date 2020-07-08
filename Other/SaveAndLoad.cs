using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveAndLoad : MonoBehaviour
{
    [ContextMenu("Save")]
    public static void Save<T>(T data, string _path)
    {

        //檢查位置
        if (!Directory.Exists(Path.GetDirectoryName(_path)))
        {
            //位置不存在，=>產生位置
            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            Debug.Log(Path.GetDirectoryName(_path));
        }


        StreamWriter stream = new StreamWriter(_path);
        string json = JsonUtility.ToJson(data);
        stream.Write(json);
        stream.Close();
    }

    public static T Load<T>(string _path)
    {
        T Data;
        //檢查路徑:
        if (File.Exists(_path))
        {
            StreamReader stream = new StreamReader(_path);

            string json = stream.ReadToEnd();
            //解析
            Data = JsonUtility.FromJson<T>(json);

            Debug.Log("Load:" + Data);

            stream.Close();
            return Data;
        }

        else
        {
            Debug.LogWarning("<color=red>找不到檔案</color>");
            return default;
        }

    }


    public static void Save_bin<T>(T data, string _path)
    {

        //檢查位置
        if (!Directory.Exists(Path.GetDirectoryName(_path)))
        {
            //位置不存在，=>產生位置
            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            Debug.Log(Path.GetDirectoryName(_path));
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(_path, FileMode.OpenOrCreate,
                                       FileAccess.ReadWrite,
                                       FileShare.None);
        formatter.Serialize(stream, data);
        stream.Dispose();
        stream.Close();
    }

    public static T Load_bin<T>(string _path)
    //public static Dictionary<string, int> Load_PlayerCoinsData(string _path)
    {
        //檢查路徑:
        if (File.Exists(_path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.None);
            T data = (T)formatter.Deserialize(stream);
            stream.Dispose();
            stream.Close();
            return data;
        }

        else
        {
            Debug.LogWarning("<color=red>找不到檔案</color>");

            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            return default;
        }

    }

}
