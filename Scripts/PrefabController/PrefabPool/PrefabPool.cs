using System.Collections.Generic;
using UnityEngine;
public interface IPoolable
{
    public void OnReturn();
}

public class PrefabPool<T> where T : MonoBehaviour,IPoolable
{

    private GameObject _original;

    private Transform _prefabStorage;


    public List<T> ActiveCellObjectList { get; set; } = new List<T>();

    //Active의 반대는?
    public List<T> InactiveCellObjectList { get; set; } = new List<T>();


    public PrefabPool(GameObject cellObjectPrefab, Transform storageTransform)
    {
        _original = cellObjectPrefab;
        _prefabStorage = storageTransform;
    }

    public T GetPrefab()
    {
        T poolableBase = default(T);
        if (InactiveCellObjectList.Count == 0)
        {
            CreatePrefabs(1);
        }
        poolableBase = InactiveCellObjectList[InactiveCellObjectList.Count - 1];
        InactiveCellObjectList.RemoveAt(InactiveCellObjectList.Count - 1);
        ActiveCellObjectList.Add(poolableBase);

        return poolableBase;
    }


    public void CreatePrefabs(int createNum)
    {
        for (int i = 0; i < createNum; i++)
        {
            GameObject prefabObject = Object.Instantiate(_original, _prefabStorage);
            prefabObject.SetActive(false);
            T poolableBase = prefabObject.GetComponent<T>();
            // poolableBase.InitObject();
            InactiveCellObjectList.Add(poolableBase);
        }
    }

    public void ReturnPrefab(T poolableBase)
    {
        poolableBase.OnReturn();
        poolableBase.gameObject.SetActive(false);

        if (ActiveCellObjectList.Contains(poolableBase))
        {
            ActiveCellObjectList.Remove(poolableBase);
            InactiveCellObjectList.Add(poolableBase);
        }
        else
        {
            Debug.LogError("[Error]CellObjectPool::ReturnCellObject Trying to return a CellObject that is not in the active list.");
        }
    }
}