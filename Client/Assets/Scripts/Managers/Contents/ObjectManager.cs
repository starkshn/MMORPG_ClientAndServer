using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    // Dictionary<int, GameObject> _objs = 

    List<GameObject> _objs = new List<GameObject>();
    
    public void Add(GameObject go)
    {
        _objs.Add(go);
    }

    public void Remove(GameObject go)
    {
        _objs.Remove(go);
    }

    public GameObject Find(Vector3Int cellPos)
    {
        foreach (GameObject obj in _objs)
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
        foreach (GameObject obj in _objs)
        {
            if (condition.Invoke(obj)) 
                return obj;
        }
        return null;
    }

    public void Clear()
    {
        _objs.Clear();
    }
}
