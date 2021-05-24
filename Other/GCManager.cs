using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 How to use:
 **Register your object**
    CGManager.RegisterObject("myKey", Instantiate(my_prefab));

 **Get your object**
    GameObject _obj = CGManager.Instanciate("myKey");

**Destory**
    CGManager.Destory("myKey",my_gameObj);

 No need to Instance this in Scene
 */
public class GCManager : MonoBehaviour
{
    static Dictionary<string, LinkedList<object>> dicts = new Dictionary<string, LinkedList<object>>();
    static Dictionary<string, Vector2> registerScale = new Dictionary<string, Vector2>();

    static int _db_c = 0;
    public static void RegisterObject(string _key, Object _obj)
    {
        LinkedList<object> _out;
        if (!dicts.TryGetValue(_key, out _out))
        {
            //create new and add to first
            dicts.Add(_key, new LinkedList<object>());
            dicts[_key].AddFirst(_obj);

            registerScale.Add(_key, (_obj as GameObject).transform.localScale);

            (_obj as GameObject).SetActive(false);
            Debug.Log("current dict count " + dicts.Count);
        }
        else
        {
            //already registered.
          
            Debug.Log("Already exist");
        }
    }


    //取得有空閒的物品
    public static GameObject Instantiate(string _key, Transform parent = null, Vector2 position = default, GameObject prefab = null)
    {
        //檢查該key是否存在
        LinkedList<object> _out;
        if (dicts.TryGetValue(_key, out _out))
        {
            //存在=>檢查有空閒的(active= false)
            if (!(_out.First.Value as GameObject).activeSelf)
            {
                LinkedListNode<object> first_obj = _out.First;
                (first_obj.Value as GameObject).transform.position = position;
                (first_obj.Value as GameObject).SetActive(true);

                //移至最後:
                dicts[_key].RemoveFirst();
                dicts[_key].AddLast(first_obj);
                (first_obj.Value as GameObject).transform.SetParent(parent);
                Debug.Log("GC Get First value " + _db_c.ToString());

                (first_obj.Value as GameObject).name = _db_c.ToString();
                _db_c++;

                (first_obj.Value as GameObject).transform.localScale = registerScale[_key];

                return (first_obj.Value as GameObject);
            }

            //找不到可使用的=>創建新的
            GameObject _newobj = UnityEngine.GameObject.Instantiate((GameObject)dicts[_key].First.Value, parent);
            _newobj.SetActive(true);
            dicts[_key].AddLast(_newobj);
          
            Debug.Log("GC Create new one");

            _newobj.transform.localScale = registerScale[_key];

            return _newobj;

        }
        else if (prefab != null)
        {
            //生出初始prefab:
            GameObject _newobj = UnityEngine.GameObject.Instantiate((GameObject)dicts[_key].First.Value);
            //RegisterObject(_key, _newobj);
            dicts.Add(_key, new LinkedList<object>());
            dicts[_key].AddLast(_newobj);
            _newobj.SetActive(true);
            Debug.Log("GC Create prefab");
            return _newobj;
        }

        //不存在=> 返回null
        Debug.Log("GC Instanciate Failed, obj not found");
        return null;
    }

    public static void Destory(string _key, GameObject obj)
    {

        obj.SetActive(false);
        //dicts[_key].Remove(obj);
        LinkedListNode<object> to_remove_obj = dicts[_key].Find(obj);
        if (to_remove_obj == null)
        {
            Debug.Log("GC find null " + obj.name);
            return;
        }
        dicts[_key].Remove(to_remove_obj);
        dicts[_key].AddFirst(obj);
        Debug.Log("GC回收 " + obj.name);

    }

    public static void Remove(string _key)
    {
        if (dicts.ContainsKey(_key))
        {
            dicts.Remove(_key);
        }
    }

    public static void Clear() {
        dicts.Clear();
        registerScale.Clear();
    }
}
