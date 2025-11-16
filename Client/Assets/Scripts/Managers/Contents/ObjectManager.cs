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

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0X7F;
        return (GameObjectType)type;
    }
    
    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objs.Add(info.ObjectId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
                MyPlayer.Stat = info.StatInfo;
                MyPlayer.SyncPos();
            }
            else
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objs.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PosInfo = info.PosInfo;
                pc.Stat = info.StatInfo;
                pc.SyncPos();
            }
        }
        else if (objectType == GameObjectType.Monster)
        {

        }
        else if (objectType == GameObjectType.Projectile)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
            go.name = "Arrow";
            _objs.Add(info.ObjectId, go);
            
            ArrowController ac = go.GetComponent<ArrowController>();
            ac.Dir = info.PosInfo.MoveDir;
            ac.CellPos = new Vector3Int(info.PosInfo.PosX, info.PosInfo.PosY, 0);
            ac.Stat = info.StatInfo;
            ac.SyncPos();
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
