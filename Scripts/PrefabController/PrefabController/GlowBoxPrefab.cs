using UnityEngine;

public class GlowBoxPrefab : SimplePrefabController
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
