using UnityEngine;

public abstract class PrefabControllerBase : MonoBehaviour, IPoolable
{
    public abstract void ResetComponent();
    public abstract void InitComponent();

    private void Awake()
    {
        InitComponent();
    }

    public void OnReturn() => ResetComponent();

}

//프리팹의 스프라이트만 관여하면 되는 경우
public class SimplePrefabController : PrefabControllerBase
{
    public SpriteRenderer SpriteRenderer;

    public override void InitComponent()
    {
        SpriteRenderer = transform.GetComponent<SpriteRenderer>();
    }
    public override void ResetComponent()
    {
        //gameObject.SetActive(false);
        SpriteRenderer.color = Color.white;
        tag = "Untagged";
        gameObject.layer = LayerMask.NameToLayer("Default");
    }


}

