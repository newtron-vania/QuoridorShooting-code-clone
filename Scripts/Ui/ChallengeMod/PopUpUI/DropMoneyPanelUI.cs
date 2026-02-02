using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMoneyPanelUI : BaseUI
{
    enum Texts
    {
        MoenyText,
        MoenyTimerText,
    }

    enum Images
    {
        MoneyImage,
    }

    enum Buttons
    {
        BackGround
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SupplyItemUI;

    private int _value;
    private float _timer;
    private RewardPool _rewardPool;
    private bool _isCountDown = false;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.BackGround).gameObject.BindEvent(OnClickSkipButton);

        // _rewardPool = GameObject.Find("GameManager").GetComponent<RewardPool>();
        _rewardPool = UIManager.Instance.SceneUI.GetComponent<RewardPool>();

        _rewardPool.GetDropMoneyInstance(ref _value, ref _timer);

        GetText((int)Texts.MoenyText).text = _value.ToString();

        StartCoroutine(CountDown());
    }

    public void OnClickSkipButton(PointerEventData data)
    {
        End();
    }

    private void End()
    {
        if (_isCountDown) StopCoroutine(CountDown());
        Debug.Log($"[INFO] DropMoneyPanelUI::OnClickSkipButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
        UIManager.Instance.ClosePopupUI(this);
        UIManager.Instance.ShowPopupUI<RewardPopUpUI>();
    }

    IEnumerator CountDown()
    {
        _isCountDown = true;
        while (_timer > 0)
        {
            GetText((int)Texts.MoenyTimerText).text = _timer.ToString();
            yield return new WaitForSeconds(1f);

            _timer -= 1;
        }
        _isCountDown = false;
        End();
    }
}
