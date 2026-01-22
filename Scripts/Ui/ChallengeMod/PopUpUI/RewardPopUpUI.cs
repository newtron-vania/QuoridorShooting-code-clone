using HM;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardPopUpUI : BaseUI
{
    enum Buttons
    {
        SkipButton,
        SelectButton
    }

    enum GameObjects
    {
        ShowItemsBoundary  // 이미지 소환 경계
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.RewardUI;
    public bool IsSelect
    {
        get
        {
            return _rewardPool.SelectItem.HasValue ? false : true;
        }
    }

    private RewardPool _rewardPool;
    private List<RewardItemUI> _items = new List<RewardItemUI>();

    private void Start()
    {
        Init();
    }

    public void Update()
    {
        GetButton((int)Buttons.SelectButton).GetComponent<Image>().color = IsSelect ? new Color(1, 1, 1) : new Color(0.5f, 0.5f, 0.5f);
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        _rewardPool = GameObject.Find("GameManager").GetComponent<RewardPool>();

        // 버튼 이벤트 연결
        GetButton((int)Buttons.SkipButton).gameObject.BindEvent(OnClickSkipButton);
        GetButton((int)Buttons.SelectButton).gameObject.BindEvent(OnClickSelectButton);
        
        // 보상 설정
        SpawnItem();
    }

    public void OnClickSkipButton(PointerEventData data)
    {
        Debug.Log($"[INFO] RewardShowPanelUI::OnClickSupplySkipButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
        UIManager.Instance.ClosePopupUI(this);
    }

    public void OnClickSelectButton(PointerEventData data)
    {
        if (IsSelect)
        {
            Debug.Log($"[INFO] SupplyShowPanelUI::OnClickSupplySelectButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
            UIManager.Instance.ClosePopupUI(this);
        }
    }

    private void SpawnItem()
    {
        GameObject parent = GetObject((int)GameObjects.ShowItemsBoundary);
        for(int i = 0; i < 3; i++)
        {
            RewardCardData newItemData = _rewardPool.GetRewardCardData();
            _items.Add(UIManager.Instance.MakeSubItem<RewardItemUI>(parent.transform));
            _items[i].CardData = newItemData;
            _items[i].ParentRewardPool = _rewardPool;
        }
    }
}