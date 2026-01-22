using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupplyReUsePopUpUI : BaseUI
{
    enum Buttons
    {
        CloseButton, // 닫기 버튼
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SupplyReUsePopUpUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
    }

    public void OnClickCloseButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI();
    }
}
