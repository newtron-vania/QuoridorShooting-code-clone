using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardItemUI : BaseUI
{
    enum Texts
    {
        ItemNameText,  // 이름
        DescriptionText, // 설명
        TagText,        // 태그 설명
    }

    enum Images
    {
        ItemImage,     // 이미지
        IconImage,     // 아이콘 이미지
    }

    enum GameObjects
    {
        BackEffect    // 뒷배경 이펙트
    }

    enum Buttons
    {
        BackGround, // 아이템 선택
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.RewardItemUI;
    public RewardCardData CardData;
    public int ItemIndex;
    public RewardPool ParentRewardPool;
    public RewardPopUpUI ParentRewardPopUpUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));
        // 이벤트 연결
        GetButton((int)Buttons.BackGround).gameObject.BindEvent(OnClickSupplySelect);

        // 아이템 설명 연결
        GetText((int)Texts.ItemNameText).text = CardData.Name;
        GetText((int)Texts.DescriptionText).text = CardData.Description;

        GetObject((int)GameObjects.BackEffect).SetActive(false);
    }

    public void Reset()
    {
        if (ParentRewardPool.SelectItemIdx != ItemIndex)
        {
            GetObject((int)GameObjects.BackEffect).SetActive(false);
        }
    }

    public void OnClickSupplySelect(PointerEventData data)
    {
        ParentRewardPool.SelectItem = CardData;
        ParentRewardPool.SelectItemIdx = ItemIndex;
        GetObject((int)GameObjects.BackEffect).SetActive(true);
        ParentRewardPopUpUI.ResetItems();
    }
}
