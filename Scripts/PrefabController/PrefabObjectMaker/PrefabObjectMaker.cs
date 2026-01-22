using UnityEngine;

public class PrefabObjectMaker
{
    protected readonly PrefabPool<PrefabControllerBase> _prefabPool;

    public PrefabObjectMaker(PrefabPool<PrefabControllerBase> prefabPool)
    {
        _prefabPool = prefabPool;
    }
    public virtual void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        baseController.transform.position = GameManager.ToWorldPosition((Vector2)objectPos);
    }

    //초기화에 필요한 다른 데이터가 필요하면 오버로딩으로 확장
    public PrefabControllerBase GetController(Vector2Int objectPos)
    {
        PrefabControllerBase prefabBaseController = _prefabPool.GetPrefab();
        SetPrefabData(prefabBaseController, objectPos);
        prefabBaseController.gameObject.SetActive(true);

        return prefabBaseController;
    }

    public void ReturnController(PrefabControllerBase obj)
    {
        _prefabPool.ReturnPrefab(obj);
    }
}