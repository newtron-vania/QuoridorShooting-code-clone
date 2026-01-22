using UnityEngine;

public class SimplePreviewMaker : PrefabObjectMaker
{
    private string _tag;
    private Color _color;
    private Sprite _sprite;
    public SimplePreviewMaker(PrefabPool<PrefabControllerBase> prefabPool, Sprite objectImg, Color color = default(Color), string tag = "Untagged") : base(prefabPool)
    {
        _tag = tag;
        _sprite= objectImg;
        _color = color;
    }

    public override void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        SimplePrefabController simplePrefabController = baseController as SimplePrefabController;
        simplePrefabController.SpriteRenderer.sprite = _sprite;
        simplePrefabController.gameObject.layer = LayerMask.NameToLayer("Preview");
        //태그 오류 막는 법 필요?
        simplePrefabController.tag = _tag;
        simplePrefabController.SpriteRenderer.color = _color;
        base.SetPrefabData(baseController, objectPos);
    }
}



public class BlockedBoxPreviewMaker:PrefabObjectMaker
{
    private Sprite _sprite;
    public BlockedBoxPreviewMaker(PrefabPool<PrefabControllerBase> prefabPool,Sprite objectImg):base(prefabPool) 
    {
        _sprite = objectImg;
    }

    public override void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        SimpleBoxPrefab blockedPreview = baseController as SimpleBoxPrefab;
        blockedPreview.SpriteRenderer.sprite = _sprite;
        blockedPreview.SpriteRenderer.color = new Color(blockedPreview.SpriteRenderer.color.r, blockedPreview.SpriteRenderer.color.g, blockedPreview.SpriteRenderer.color.b, 0.4f);
        blockedPreview.gameObject.layer = LayerMask.NameToLayer("Preview");
        blockedPreview.BoxCollider.enabled = false;

        base.SetPrefabData(baseController, objectPos);
    }
}


public class GlowPreviewMaker : PrefabObjectMaker
{
    private string _tag;
    private Color _color;
    private Color _colorFrame;
    private Sprite _sprite;
    private Material _material;
    public GlowPreviewMaker(PrefabPool<PrefabControllerBase> prefabPool, Sprite objectImg, Material material, Color color = default(Color), string tag = "Untagged", Color colorFrame = default(Color)) : base(prefabPool)
    {
        _tag = tag;
        _sprite = objectImg;
        _color = color;
        _material = material;
        _colorFrame = colorFrame;
    }

    public override void SetPrefabData(PrefabControllerBase baseController, Vector2Int objectPos)
    {
        SimplePrefabController simplePrefabController = baseController as SimplePrefabController;
        simplePrefabController.SpriteRenderer.sprite = _sprite;
        simplePrefabController.gameObject.layer = LayerMask.NameToLayer("Preview");
        simplePrefabController.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().color = _colorFrame;
        //태그 오류 막는 법 필요?
        simplePrefabController.tag = _tag;
        simplePrefabController.SpriteRenderer.color = _color;
        simplePrefabController.SpriteRenderer.material = _material;
        base.SetPrefabData(baseController, objectPos);
    }
}