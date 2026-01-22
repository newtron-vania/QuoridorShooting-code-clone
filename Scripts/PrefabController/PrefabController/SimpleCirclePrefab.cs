using UnityEngine;


//프리팹의 원형콜라이더까지 관여해야되는 경우
public class SimpleCirclePrefab : SimplePrefabController
{
    public CircleCollider2D CircleCollider;

    public override void InitComponent()
    {
        base.InitComponent();
        CircleCollider = GetComponent<CircleCollider2D>();
    }


}