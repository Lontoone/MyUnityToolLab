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
public class GCManager : MonoBehaviour
{
    static Dictionary<string, LinkedList<object>> dicts = new Dictionary<string, LinkedList<object>>();

    public static void RegisterObject(string _key, Object _obj)
    {
        LinkedList<object> _out;
        if (!dicts.TryGetValue(_key, out _out))
        {
            dicts.Add(_key, new LinkedList<object>());
            dicts[_key].AddFirst(_obj);            

            (_obj as GameObject).SetActive(false);
            Debug.Log("current dict count " + dicts.Count);
        }
        else
        {
            Debug.Log("Already exist");
        }
    }


    //取得有空閒的物品
    public static GameObject Instantiate(string _key)
    {
        //檢查該key是否存在
        LinkedList<object> _out;
        if (dicts.TryGetValue(_key, out _out))
        {
            //存在=>檢查有空閒的(active= false)
            if (!(_out.First.Value as GameObject).activeSelf)
            {
                LinkedListNode<object> first_obj = _out.First;
                (first_obj.Value as GameObject).SetActive(true);

                //移至最後:
                dicts[_key].RemoveFirst();
                dicts[_key].AddLast(first_obj);

                return (first_obj.Value as GameObject);
            }

            //找不到可使用的=>創建新的
            GameObject _newobj = UnityEngine.GameObject.Instantiate((GameObject)dicts[_key].First.Value);
            _newobj.SetActive(true);
            dicts[_key].AddLast(_newobj);
            return _newobj;

        }

        //不存在=> 返回null
        Debug.Log("Instanciate Failed, obj not found");
        return null;

    }

    public static void Destory(string _key, GameObject obj)
    {
        obj.SetActive(false);
        dicts[_key].Remove(obj);
        dicts[_key].AddFirst(obj);

    }

    public static void Remove(string _key)
    {
        if (dicts.ContainsKey(_key))
        {
            dicts.Remove(_key);
        }
    }

}
