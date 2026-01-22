using UnityEngine;


public class SkillAreaMaker : PrefabObjectMaker
{
    private Sprite _sprite;
    private Color _color;
    public SkillAreaMaker(PrefabPool<PrefabControllerBase> prefabPool,Sprite sprite, Color color) : base(prefabPool) 
    {
        _sprite = sprite;
        _color= color;
    }

    public override void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        SkillAreaPrefab skillAreaPrefabController = baseController as SkillAreaPrefab;
        skillAreaPrefabController.SkillAreaComponent.SetData(_sprite,_color);

        base.SetPrefabData(baseController, objectPos);
    }
}
