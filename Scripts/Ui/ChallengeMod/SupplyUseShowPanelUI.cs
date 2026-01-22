using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupplyUseShowPanelUI : BaseUI
{
    enum Buttons
    {
        ExitButton // 팝업창 닫기
    }
    enum Texts
    {
        SupplyUseDesText // 보급품 사용 설명
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SupplyUseShowPopUp;

    public int SupplyID;
    public string TargetName;
    public string SupplyName;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickSupplyExitButton);
        GetText((int)Texts.SupplyUseDesText).text = "<color=#20B2AA>" + SupplyName + "</color>을 <color=#DC143C>" + TargetName + "</color>에게 사용했습니다.";
    }

    public void OnClickSupplyExitButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
