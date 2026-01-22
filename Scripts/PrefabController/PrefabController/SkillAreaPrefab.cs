using UnityEngine;



public class SkillAreaPrefab : PrefabControllerBase
{
    public SkillAreaComponent SkillAreaComponent;
    private SpriteRenderer _spriteRenderer;
    private Transform _hitEffect;
    public override void InitComponent()
    {
        SkillAreaComponent = GetComponent<SkillAreaComponent>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _hitEffect = transform.Find("HitEffect");
        SkillAreaComponent.InitData(_spriteRenderer, _hitEffect);
    }

    public override void ResetComponent()
    {
        SkillAreaComponent.Reset();
    }
  
}
