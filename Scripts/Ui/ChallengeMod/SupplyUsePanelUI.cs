using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Reflection;

public class SupplyUsePanelUI : BaseUI
{
    enum Buttons
    {
        ExitButton,     // 화면 밖 클릭시 이벤트 발생
        SupplyUseButton // 보급품 사용 버튼
    }

    enum Texts
    {
        SupplyNameText, // 보급품 이름
        SupplyDescriptionText // 보급품 설명
    }

    enum Images
    {
        SupplyImage,        // 보급품 이미지
        SupplyTypeImage     // 보급품 타입 이미지
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SupplyUsePanelUI;

    public int ItemID;
    private SupplymentData _supplyData;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.SupplyUseButton).gameObject.BindEvent(OnClickSupplyUseButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

        _supplyData = DataManager.Instance.GetSupplyData(ItemID);

        // 보급품 UI 세팅
        GetText((int)Texts.SupplyNameText).text = _supplyData.Name;
        GetText((int)Texts.SupplyDescriptionText).text = _supplyData.Description;
        // GetImage((int)Images.SupplyImage).sprite = DataManager.Instance._supplyData.Image;
        // GetImage((int)Images.SupplyTypeImage).sprite; // 타입 관련 스프라이트 따로 없음
    }

    public void OnClickSupplyUseButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
        SupplyManager.Instance.UseSupplyItem(ItemID);
    }

    public void OnClickExitButton(PointerEventData data)
    {
        Debug.Log($"[INFO] SupplyUsePanelUI::OnClickSupplyExitButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
        UIManager.Instance.ClosePopupUI(this);
    }
}
