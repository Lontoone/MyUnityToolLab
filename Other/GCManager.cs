using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 How to use:
 **Register your object**
    CGManager.RegisterObject("myKey", gameObject );

 **Get your object**
    GameObject _obj = CGManager.Instanciate<GameObject>("myKey");

**Destory**
    CGManager.Destory("myKey",gameObject);

 No need to Instance this in Scene
 */
public class GCManager : MonoBehaviour
{
    static Dictionary<string, LinkedList<object>> s_m_dicts = new Dictionary<string, LinkedList<object>>();
    static Dictionary<object, LinkedListNode<object>> s_m_nodes = new Dictionary<object, LinkedListNode<object>>();
    static Dictionary<string, Vector3> s_m_registerScale = new Dictionary<string, Vector3>();

    static int _db_c = 0;
    public static void RegisterObject(string _key, object _obj)
    {
        LinkedList<object> _out;
        if (!s_m_dicts.TryGetValue(_key, out _out))
        {
            //create new and add to first
            s_m_dicts.Add(_key, new LinkedList<object>());
            s_m_dicts[_key].AddFirst(_obj);
            s_m_nodes.Add(_obj, s_m_dicts[_key].First);

            s_m_registerScale.Add(_key, (_obj as GameObject).transform.localScale);

            (_obj as GameObject).SetActive(false);
            Debug.Log("current dict count " + s_m_dicts.Count);
        }
        else
        {
            //already registered.
            Debug.Log("Already exist");
        }
    }

    public static T Instantiate<T>(string _key, Transform parent = null, Vector3 position = default, GameObject prefab = null) where T:class
    {
        T _res;
        LinkedListNode<object> _node = InstantiateNode(_key, parent, position, prefab);
        LinkedListNode<object> _oldNode;
        if (s_m_nodes.TryGetValue(_node.Value, out _oldNode))
        {
            s_m_nodes[_node.Value] = _node;
            _res =( _node.Value as GameObject).GetComponent<T>();
            return _res;
        }
        else
        {
            s_m_nodes.Add(_node.Value, _node);
            //_res = (T)_node.Value;
            _res = (_node.Value as GameObject).GetComponent<T>();
            return _res;
        }
    }

    //取得有空閒的物品
    private static LinkedListNode<object> InstantiateNode(string _key, Transform parent = null, Vector2 position = default, GameObject prefab = null)
    {
        //檢查該key是否存在
        LinkedList<object> _out;
        if (s_m_dicts.TryGetValue(_key, out _out))
        {
            //存在=>檢查有空閒的(active= false)
            if (!(_out.First.Value as GameObject).activeSelf)
            {
                LinkedListNode<object> first_obj = _out.First;
                (first_obj.Value as GameObject).transform.position = position;
                (first_obj.Value as GameObject).SetActive(true);

                //移至最後:
                s_m_dicts[_key].RemoveFirst();
                s_m_dicts[_key].AddLast(first_obj);
                (first_obj.Value as GameObject).transform.SetParent(parent);
                Debug.Log("GC Get First value " + _db_c.ToString());

                (first_obj.Value as GameObject).name = _db_c.ToString();
                _db_c++;

                (first_obj.Value as GameObject).transform.localScale = s_m_registerScale[_key];

                //return (first_obj.Value as GameObject);
                return first_obj;
            }

            //找不到可使用的=>創建新的
            GameObject _newobj = UnityEngine.GameObject.Instantiate((GameObject)s_m_dicts[_key].First.Value, parent);
            LinkedListNode<object> newNode = s_m_dicts[_key].AddLast(_newobj);
            return newNode;      
        }
        else if (prefab != null)
        {
            //生出初始prefab:
            GameObject _newobj = UnityEngine.GameObject.Instantiate((GameObject)s_m_dicts[_key].First.Value);
            s_m_dicts.Add(_key, new LinkedList<object>());
            LinkedListNode<object> newNode = s_m_dicts[_key].AddLast(_newobj);
            _newobj.SetActive(true);
            Debug.Log("GC Create prefab");
            return newNode;
        }

        //不存在=> 返回null
        Debug.Log("GC Instanciate Failed, obj not found");
        return null;
    }

    public static void Destory(string _key, object obj)
    {
        LinkedListNode<object> _node = s_m_nodes[obj];
        Destory(_key, _node);   
    }

    private static void Destory(string _key, LinkedListNode<object> node)
    {
        object obj = node.Value;
        (obj as GameObject).SetActive(false);
        //LinkedListNode<object> to_remove_obj = dicts[_key].Find(obj); 
        //上面這樣.Find()又要尋找整個Linklist, 
        //時間依舊O(N), 使用Linkedlist時正確應該是把Node給使用者自行管理
        s_m_dicts[_key].Remove(obj);
        s_m_dicts[_key].AddFirst(obj);
        Debug.Log("GC回收 " + (obj as GameObject).name);

    }

    public static void Remove(string _key)
    {
        if (s_m_dicts.ContainsKey(_key))
        {
            s_m_dicts.Remove(_key);            
        }
    }

    public static void Clear()
    {
        s_m_dicts.Clear();
        s_m_registerScale.Clear();
        s_m_nodes.Clear();
    }
}
