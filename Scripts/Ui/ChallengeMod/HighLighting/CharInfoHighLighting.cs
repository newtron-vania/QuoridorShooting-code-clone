using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CharInfoHighLighting : BaseUI
{
    protected override bool IsSorting => false;
    public override UIName ID => UIName.CharInfoHighLighting;

    // 0 : 캐릭터 선택 / 1 : 캐릭터 행동력 충만
    public int HighlightingID = 1;
    public TMP_Text PopUpText = null;

    public override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        Init();
        ChangeLightColor();
    }

    private void ChangeLightColor()
    {
        switch(HighlightingID)
        {
            case 0:
                gameObject.GetComponent<Image>().color = new Color(0.227451f, 0.5411765f, 0.6784314f, 1); // #3A8AAD
                gameObject.GetComponent<Image>().DOFade(0.0f, 1).SetLoops(-1, LoopType.Yoyo);
                break;
            case 1:
                gameObject.GetComponent<Image>().color = new Color(1, 1, 0 , 0); // #FFFF00
                PopUpText.text = "Ready!"; // 팝업 문자 설정
                StartCoroutine(FullActionHighlighting());
                break;
            default:
                Debug.LogWarning("[WARN]CharInfoSelectHighLighting(ChangeLightColor) - 올바르지않은 ID값이 할당 되었습니다.");
                break;
        }
    }

    private IEnumerator FullActionHighlighting()
    {
        yield return new WaitForSeconds(2.1f);
        gameObject.GetComponent<Image>().DOFade(1f, 0.3f).SetEase(Ease.Linear);
        PopUpText.DOFade(1f, 0.3f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponent<Image>().DOFade(0.0f, 0.3f).SetEase(Ease.Linear);
        PopUpText.DOFade(0.0f, 0.3f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.3f);
        PopUpText.text = ""; // 팝업 문자 설정 초기화
        UIManager.Instance.CloseUI(this);
    }
}
