using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerTurnStartPanelUI : BaseUI
{
    enum Texts
    {
        BattleStartText, // 배틀 시작 텍스트
    }
    enum GameObjects
    {
        TextBackGround, // 배틀 시작 뒷 배경
        BackGround,     // 검은색 뒷배경
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.PlayerTurnStartUI;
    private Sequence _startSequence;
    private Sequence _endSequence;
    

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
    }

    public void Start()
    {
        Init();
        StartCoroutine(StartPlayerTurn());
    }

    IEnumerator StartPlayerTurn()
    {
        // 시작 시퀀스
        _startSequence = DOTween.Sequence()
        .OnStart(() =>
        {
            GetObject((int)GameObjects.BackGround).GetComponent<Image>().color = new Color(0, 0, 0, 0);
            GetText((int)Texts.BattleStartText).color = new Color(1, 1, 1, 0);
            GetObject((int)GameObjects.TextBackGround).transform.localScale = new Vector3(0, 1, 1);
        })
        .Append(GetObject((int)GameObjects.BackGround).GetComponent<Image>().DOFade(0.7f, 0.4f).SetEase(Ease.Linear))
        .Append(GetObject((int)GameObjects.TextBackGround).transform.DOScaleX(1, 0.3f).SetEase(Ease.Linear))
        .Append(GetText((int)Texts.BattleStartText).DOFade(1, 0.3f).SetEase(Ease.Linear));
        yield return new WaitForSeconds(1.5f);
        // 종료 시퀀스
        _endSequence = DOTween.Sequence()
        .OnStart(() =>
        {
            Debug.Log("[INFO] PlayerTurnStartPanelUI(StartPlayerTurn) - PlayerTurnPanelUI 사라짐");
        })
        .Append(GetObject((int)GameObjects.TextBackGround).transform.DOScaleX(0, 0.3f).SetEase(Ease.Linear))
        .Join(GetText((int)Texts.BattleStartText).DOFade(0, 0.3f).SetEase(Ease.Linear))
        .Append(GetObject((int)GameObjects.BackGround).GetComponent<Image>().DOFade(0, 0.3f).SetEase(Ease.Linear));
        yield return new WaitForSeconds(0.6f);
        UIManager.Instance.ClosePopupUI(this);
    }
}
