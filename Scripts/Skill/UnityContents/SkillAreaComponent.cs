using System.Collections;
using UnityEngine;

public class SkillAreaComponent : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Transform _hitEffect;

    private void Update()
    {
        //데미지를 가하는 장소를 일시적으로 보여주는 임시 함수
    }

    public IEnumerator ShowZoneHit()
    {
        gameObject.SetActive(true);
        _hitEffect.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        _hitEffect.gameObject.SetActive(false);
        //gameObject.SetActive(false);  //안꺼도 문제 없는데 꺼야되면 예외처리 필요
    }


    public void InitData(SpriteRenderer spriteRenderer, Transform hitEffectTransform)
    {
        gameObject.SetActive(false);

        _spriteRenderer = spriteRenderer;

        _hitEffect = hitEffectTransform;
        _hitEffect.gameObject.SetActive(false);

    }

    public void SetData(Sprite sprite, Color color)
    {
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.color = color;

        gameObject.SetActive(true);
    }

    //ToDo: 타일에 중복되는 상태가 생기면 추가 처리
    public void Reset()
    {
        _spriteRenderer.sprite = null;
        _spriteRenderer.color = Color.white;
        gameObject.SetActive(false);
        _hitEffect.gameObject.SetActive(false);
    }

}