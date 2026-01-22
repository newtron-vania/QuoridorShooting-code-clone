using UnityEngine;

//프리팹의 박스콜라이더까지 관여해야되는 경우
public class SimpleBoxPrefab : SimplePrefabController
{
    public BoxCollider2D BoxCollider;

    public override void InitComponent()
    {
        base.InitComponent();
        BoxCollider = GetComponent<BoxCollider2D>();
    }

    public override void ResetComponent()
    {
        base.ResetComponent();
        BoxCollider.enabled = true;
    }
}

