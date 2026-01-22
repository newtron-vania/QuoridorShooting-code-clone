
using UnityEngine;

public class WallMaker : PrefabObjectMaker
{
    private Sprite _sprite;
    public WallMaker(PrefabPool<PrefabControllerBase> prefabPool,Sprite sprite) : base(prefabPool) 
    {
        _sprite= sprite;
    }

    public override void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        SimpleWallPrefab wallPrefab = baseController as SimpleWallPrefab;
        wallPrefab.SpriteRenderer.sprite = _sprite;


        base.SetPrefabData(baseController, objectPos);
    }

}
