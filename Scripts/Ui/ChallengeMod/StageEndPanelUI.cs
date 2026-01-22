using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using HM;

public class StageEndPanelUI : BaseUI
{
    enum Texts
    {
        TitleText,      // Lose Win
        DescriptionText // 부연설명
    }
    enum Buttons
    {
        CheckButton     // 확인버튼
    }
    protected override bool IsSorting => true;
    public override UIName ID => UIName.StageEndPopUpUI;

    public bool IsPlayerWin;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<TMP_Text>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.CheckButton).gameObject.BindEvent(OnClickCheckButton);

        if (IsPlayerWin)
        {
            GetText((int)Texts.TitleText).text = "WIN";
            GetText((int)Texts.DescriptionText).text = "전투에서 승리하셨습니다.";
        }
        else
        {
            GetText((int)Texts.TitleText).text = "LOSE";
            GetText((int)Texts.DescriptionText).text = "전투에서 패배하셨습니다.";
        }
    }

    public void OnClickCheckButton(PointerEventData data)
    {
        Debug.Log("[INFO]StageEndPaenelUI(OnClickCheckButton) - 해당 스테이지를 완료했습니다.");
        UIManager.Instance.ShowPopupUI<DropMoneyPanelUI>();
        //string CurrentSceneName = SceneManager.GetActiveScene().name;
        //SupplyManager.Reset();
        //EventManager.Reset();
        //UIManager.Reset();
        //TriggerManager.Reset();
        //LogManager.Reset();
        //GameManager.Reset();
        //SceneManager.LoadScene(CurrentSceneName);
    }
}
