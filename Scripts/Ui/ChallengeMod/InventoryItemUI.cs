using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mono.Cecil.Cil;

public class InventoryItemUI : BaseUI
{
    enum Images
    {
        ItemType    // 아이템 타입
    }
    enum Texts
    {
        ItemCount   // 아이템 갯수
    }
    enum Buttons
    {
        ItemButton
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.InventoryItemUI;

    public TMP_Text ItemCount;
    public Image ItemType;
    public Image ItemImage;
    public Button ItemButton;
    public int ItemID;
    public bool IsSupplyItem;

    private List<EffectData> _effects;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        // 컴포넌트 저장
        ItemCount = GetText((int)Texts.ItemCount);
        ItemButton = GetButton((int)Buttons.ItemButton);
        ItemImage = ItemButton.gameObject.GetComponent<Image>();
        ItemType = GetImage((int)Images.ItemType);

        // 버튼 이벤트 연결
        ItemButton.gameObject.BindEvent(OnClickInvenItem);
    }

    public void OnClickInvenItem(PointerEventData data)
    {
        if (IsSupplyItem) _effects = DataManager.Instance.GetSupplyData(ItemID).EffectDataList;
        else return;
        // 인게임 상에서 기물 선택이 필수로 요구하는 보급품일 경우 검사
        if (_effects[0].Target == TargetType.Self)
        {
            GameObject token = GameManager.Instance.BattleSystem.CurrentSelectCharacter;
            IsSupplyItem = (token && token.tag == "Player") ? true : false;
        }

        if (IsSupplyItem)
        {
            SupplyUsePanelUI item = UIManager.Instance.ShowPopupUI<SupplyUsePanelUI>();
            item.ItemID = ItemID;
        }
        else
        {
            AlertPopUpUI alert = UIManager.Instance.ShowPopupUI<AlertPopUpUI>();
            alert.ShowText = "올바른 기물을 선택해주세요.";
            IsSupplyItem = true;
        }
    }
}
