using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI.Define;
using UnityEngine;
using UnityEngine.UI;

public class ToastItemUI : BaseUI
{
    enum Texts
    {
        EventDescriptionText    // 이벤트 발생 설명 텍스트
    }
    enum Images
    {
        Icon                    // 이벤트 발생 시킨 이미지
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.ToastItemUI;

    public ToastData ToastData;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        if (ToastData != null)
        {
            GetImage((int)Images.Icon).sprite = ToastData.Icon;
            GetText((int)Texts.EventDescriptionText).text = ToastData.Description;
        }
        else
        {
            Debug.LogWarning("[WARN]ToastItemUI - ToastPopUp data missing");
        }

        StartCoroutine(ShowToastPopUp());
    }

    // 토스트 팝업 사이즈가 맥스에 도달했을 때 자동 호출
    public void DeleteToast()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    // 토스트 팝업 연출 코루틴
    private IEnumerator ShowToastPopUp()
    {
        gameObject.transform.DOScaleX(1, 0.3f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponent<Image>().DOFade(0, 2.5f).SetEase(Ease.Linear);
        GetText((int)Texts.EventDescriptionText).DOFade(0, 2.5f).SetEase(Ease.Linear);
        GetImage((int)Images.Icon).DOFade(0, 2.5f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(2.5f);
        DestroyImmediate(gameObject);
    }
}
