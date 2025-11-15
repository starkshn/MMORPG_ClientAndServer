using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public MyPlayerController MyPlayer { get; set; }
    Dictionary<int, GameObject> _objs = new Dictionary<int, GameObject>();
    
    public void Add(PlayerInfo info, bool myPlayer = false)
    {
        if (myPlayer)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
            go.name = info.Name;
            _objs.Add(info.PlayerId, go);

            MyPlayer = go.GetComponent<MyPlayerController>();
            MyPlayer.Id = info.PlayerId;
            MyPlayer.PosInfo = info.PosInfo;
            MyPlayer.SyncPos();
        }
        else
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Player");
            go.name = info.Name;
            _objs.Add(info.PlayerId, go);

            PlayerController pc = go.GetComponent<PlayerController>();
            pc.Id = info.PlayerId;
            pc.PosInfo = info.PosInfo;
            pc.SyncPos();
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objs.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void RemoveMyPlayer()
    {
        if (MyPlayer == null)
            return;

        Remove(MyPlayer.Id);
        MyPlayer = null;
    }

    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objs.TryGetValue(id, out go);
        return go;
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objs.Values)
        {
            BaseController cc = obj.GetComponent<BaseController>();
            if (cc == null)
                continue;

            if (cc.CellPos == cellPos)
                return obj;
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject obj in _objs.Values)
        {
            if (condition.Invoke(obj))
                return obj;
        }

        return null;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objs.Values)
            Managers.Resource.Destroy(obj);
        _objs.Clear();
    }
}
