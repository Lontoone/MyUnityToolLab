using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LocalPlayerProperty
{
    public static event Action<string, object, Player> OnDataUpdate;
    public Dictionary<string, object> playerProperty = new Dictionary<string, object>();
    public void SetProperty(string _key, object _data, bool _useUpdate = true)
    {
        Player _playerData = GetValue<Player>("Player");

        if (playerProperty.ContainsKey(_key))
        {
            playerProperty[_key] = _data;
            Debug.Log("set " + _key + " " + _data);
        }
        else
        {
            playerProperty.Add(_key, _data);
            Debug.Log("add " + _key + " " + _data);
        }
        if (_useUpdate)
        {
            OnDataUpdate?.Invoke(_key, _data, _playerData);
        }
    }

    public T GetValue<T>(string _key, bool _useNetwork = false)
    {
        object data;
        if (_useNetwork && PhotonNetwork.IsConnected)
        {
            return GetOnlineValue<T>(_key);
        }
        //local
        if (playerProperty.TryGetValue(_key, out data))
        {
            return (T)data;
        }
        else
        {
            return default;
        }
    }
    public T GetOnlineValue<T>(string _key)
    {
        Player _playerData = GetValue<Player>(CustomPropertyCode.PLAYER);
        return (T)_playerData.CustomProperties[_key];
    }


}
