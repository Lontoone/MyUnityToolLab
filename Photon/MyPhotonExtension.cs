
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

public static class MyPhotonExtension
{
    // Convert an object to a byte array
    public static byte[] ObjectToByteArray(Object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    // Convert a byte array to an Object
    public static Object ByteArrayToObject(byte[] arrBytes)
    {
        using (var memStream = new MemoryStream())
        {
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = binForm.Deserialize(memStream);
            return obj;
        }
    }  

    public static ExitGames.Client.Photon.Hashtable WrapToHash(Object[] key_value_array)
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < key_value_array.Length; i += 2)
        {
            hash.Add(key_value_array[i], key_value_array[i + 1]);
        }
        return hash;

    }

    public static Photon.Realtime.Player GetPlayerByName(this string _nickName)
    {
        foreach (Photon.Realtime.Player p in Photon.Pun.PhotonNetwork.PlayerList)
        {
            if (p.NickName.Trim() == _nickName.Trim())
            {
                return p;
            }
        }
        return null;
    }
}
