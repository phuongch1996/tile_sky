using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public PoolParam0 poolParamTiles;
    public PoolParam0 poolParamGameTexts;
    public PoolParam0 poolParamFxs;
    public PoolParam0 poolParamBreaks;
    public PoolParam0 poolParamUI;
    readonly List<PoolParam> poolParams = new();
    public Dictionary<Transform, List<PoolObject>> poolObjectsDict;
    public static PoolManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
        poolParams.Clear();
        for (int i = 0; i < poolParamTiles.poolObjects.Count; i++)
        {
            var tfm = new GameObject("itemtiles_" + i).transform;
            tfm.SetParent(poolParamTiles.poolTfm);
            tfm.localPosition = Vector3.zero;
            poolParams.Add(new(poolParamTiles.poolObjects[i], tfm));
        }
        for (int i = 0; i < poolParamGameTexts.poolObjects.Count; i++)
        {
            //var tfm = Instantiate(new GameObject("gametexts_" + i), poolParamGameTexts.poolTfm).transform;
            var tfm = new GameObject("gametexts_" + i).transform;
            tfm.SetParent(poolParamGameTexts.poolTfm);
            tfm.localPosition = Vector3.zero;
            poolParams.Add(new(poolParamGameTexts.poolObjects[i], tfm));
        }
        for (int i = 0; i < poolParamFxs.poolObjects.Count; i++)
        {
            //var tfm = Instantiate(new GameObject("fxs_" + i), poolParamFxs.poolTfm).transform;
            var tfm = new GameObject("fxs_" + i).transform;
            tfm.SetParent(poolParamFxs.poolTfm);
            tfm.localPosition = Vector3.zero;
            poolParams.Add(new(poolParamFxs.poolObjects[i], tfm));
        }
        for (int i = 0; i < poolParamBreaks.poolObjects.Count; i++)
        {
            //var tfm = Instantiate(new GameObject("breaks_" + i), poolParamBreaks.poolTfm).transform;
            var tfm = new GameObject("breaks_" + i).transform;
            tfm.SetParent(poolParamBreaks.poolTfm);
            tfm.localPosition = Vector3.zero;
            poolParams.Add(new(poolParamBreaks.poolObjects[i], tfm));
        }
        for (int i = 0; i < poolParamUI.poolObjects.Count; i++)
        {
            //var tfm = Instantiate(new GameObject("breaks_" + i), poolParamBreaks.poolTfm).transform;
            var tfm = new GameObject("ui_" + i).transform;
            tfm.SetParent(poolParamUI.poolTfm);
            tfm.localPosition = Vector3.zero;
            poolParams.Add(new(poolParamUI.poolObjects[i], tfm));
        }
        //poolParams.AddRange(poolParamTiles);
        //poolParams.AddRange(poolParamGameTexts);
        //poolParams.AddRange(poolParamFxs);
        //poolParams.AddRange(poolParamBreaks);
        poolObjectsDict = new();
        for (int i = 0; i < poolParams.Count; i++)
        {
            List<PoolObject> poolObjects = new();
            //for (int j = 0; j < 10; j++)
            //{
            //var poolObj = Instantiate(poolParams[i].poolObject, poolParams[i].poolTfm, false);
            //poolObj.poolTfm = poolParams[i].poolTfm;
            //poolObj.Inactive();
            //poolObjects.Add(poolObj);
            //}
            poolObjectsDict.Add(poolParams[i].poolTfm, poolObjects);
        }
    }
    public T GetObject<T>(PoolObject prefab, Transform parent, float? timeActive = null) where T : PoolObject
    {
        if (poolParams.Exists(x => x.poolObject == prefab))
        {
            PoolParam poolParam = poolParams.Find(x => x.poolObject == prefab);
            if (poolObjectsDict.TryGetValue(poolParam.poolTfm, out var value))
            {
                // Unity retains managed references to objects destroyed with a scene/FX parent.
                value.RemoveAll(obj => obj == null);
                var reusable = value.Find(obj => obj.PoolerStatus == ObjPoolerStatus.Inactive);
                if (reusable == null)
                {
                    var poolObj = Instantiate(poolParam.poolObject, parent != null ? parent : poolParam.poolTfm, true);
                    poolObj.poolTfm = poolParam.poolTfm;
                    value.Add(poolObj);
                    poolObj.Active(timeActive);
                    return poolObj.GetComponent<T>();
                }
                else
                {
                    var obj = reusable;
                    obj.transform.SetParent(parent != null ? parent : poolParam.poolTfm);
                    obj.Active(timeActive);
                    return obj.GetComponent<T>();
                }
            }
        }
        return null;
    }
    public void ReleaseObject(PoolObject poolObj)
    {
        if (poolObj == null)
            return;
        poolObj.transform.SetParent(poolObj.poolTfm != null ? poolObj.poolTfm : transform);
        poolObj.Inactive();
    }
    public void ReleaseAllObjects()
    {
        for (int i = 0; i < poolParams.Count; i++)
        {
            if (poolObjectsDict.TryGetValue(poolParams[i].poolTfm, out var value))
            {
                for (int index = value.Count - 1; index >= 0; index--)
                {
                    var obj = value[index];
                    if (obj == null)
                    {
                        value.RemoveAt(index);
                        continue;
                    }
                    if (obj.PoolerStatus != ObjPoolerStatus.Inactive || obj.transform.parent != obj.poolTfm)
                        ReleaseObject(obj);
                }
            }
        }
    }
}
[Serializable]
public class PoolParam
{
    public PoolObject poolObject;
    public Transform poolTfm;
    public PoolParam(PoolObject poolObject, Transform poolTfm)
    {
        this.poolObject = poolObject;
        this.poolTfm = poolTfm;
    }
}
[Serializable]
public class PoolParam0
{
    public List<PoolObject> poolObjects;
    public Transform poolTfm;
}
