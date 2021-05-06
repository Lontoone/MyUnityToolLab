using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 How to use:
 **Register your object**
    CGManager.RegisterObject("myKey", Instantiate(my_prefab));

 **Get your object**
    GameObject _obj = CGManager.Instanciate("myKey");

 No need to Instance in Scene
 */
public class CGManager : MonoBehaviour
{
    static Dictionary<string, List<object>> dicts = new Dictionary<string, List<object>>();

    public static void RegisterObject(string _key, Object _obj)
    {
        if (!dicts.ContainsKey(_key))
        {
            dicts.Add(_key, new List<object>());
            dicts[_key].Add(_obj);

            (_obj as GameObject).SetActive(false);
            Debug.Log("current dict count " + dicts.Count);
        }
        else
        {
            Debug.Log("Already exist");
        }
    }


    //取得有空閒的物品
    public static GameObject Instanciate(string _key)
    {
        //檢查該key是否存在
        if (dicts.ContainsKey(_key))
        {
            //存在=>檢查有空閒的(active= false)
            for (int i = 0; i < dicts[_key].Count; i++)
            {
                if (!(dicts[_key][i] as GameObject).activeSelf)
                {
                    (dicts[_key][i] as GameObject).SetActive(true);
                    //inuse_objs.Add(dicts[_key][i], _key);
                    return (dicts[_key][i] as GameObject);
                }
            }
            //找不到可使用的=>創建新的
            GameObject _newobj = Instantiate((GameObject)dicts[_key][0]);
            _newobj.SetActive(true);
            dicts[_key].Add(_newobj);
            return _newobj;

        }

        //不存在=> 返回null
        Debug.Log("Instanciate Failed, obj not found");
        return null;

    }

    public static void Destory(GameObject obj)
    {
        obj.SetActive(false);
    }

    public static void Remove(string _key)
    {
        if (dicts.ContainsKey(_key))
        {
            dicts.Remove(_key);
        }
    }

}
