using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateOnlineProperty : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        LocalPlayerProperty.OnDataUpdate += UpdateOnlineData;
        DontDestroyOnLoad(gameObject);
    }
    private void OnDestroy()
    {
        LocalPlayerProperty.OnDataUpdate -= UpdateOnlineData;
    }
    private void UpdateOnlineData(string _key, object _data, Player _player)
    {
        if (_player != null)
        {
            Debug.Log("Photon " + _player.NickName + " update " + _key);
            _player.SetCustomProperties(MyExtension.WrapToHash(new object[]
                                                               { _key, _data }));
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        //copy data to local
        int _playerIndx = (int)targetPlayer.CustomProperties[CustomPropertyCode.PLAYER_INDEX];

        object _data;
        if (changedProps.TryGetValue(GameResultManager.KILL, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(GameResultManager.KILL, _data, false);
        }
        else if (changedProps.TryGetValue(GameResultManager.DAMAGE, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(GameResultManager.DAMAGE, _data, false);
        }
        else if (changedProps.TryGetValue(GameResultManager.DAMAGETAKE, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(GameResultManager.DAMAGETAKE, _data, false);
        }
        else if (changedProps.TryGetValue(GameResultManager.DEATH, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(GameResultManager.DEATH, _data, false);
        }


        else if (changedProps.TryGetValue(CustomPropertyCode.TEAM_CODE, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.TEAM_CODE, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.TEAM_LAYER, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.TEAM_LAYER, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.BODY_CODE, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.BODY_CODE, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.HEAD_CDOE, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.HEAD_CDOE, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.ROOM_READY, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.ROOM_READY, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.PLAYER, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.PLAYER, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.PLAYER_INDEX, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.PLAYER_INDEX, _data, false);
        }
        else if (changedProps.TryGetValue(CustomPropertyCode.LIFESTOCK, out _data))
        {
            LocalRoomManager.instance.players[_playerIndx].SetProperty(CustomPropertyCode.LIFESTOCK, _data, false);
        }
    }
}
