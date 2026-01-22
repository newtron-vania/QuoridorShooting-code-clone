using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausePanelUI : BaseUI
{
    enum Buttons
    {
        PauseNOButton,      // 돌아가기 버튼
        PauseYESButton      // 게임 종료 버튼
    }
    protected override bool IsSorting => true;
    public override UIName ID => UIName.PausePanelUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.PauseNOButton).gameObject.BindEvent(OnClickPauseNoButton);
        GetButton((int)Buttons.PauseYESButton).gameObject.BindEvent(OnClickPauseYesButton);
    }

    // 다시 게임으로 돌아가기
    public void OnClickPauseNoButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
    }

    // 게임 종료하기
    public void OnClickPauseYesButton(PointerEventData data)
    {
        Application.Quit();
    }
}
