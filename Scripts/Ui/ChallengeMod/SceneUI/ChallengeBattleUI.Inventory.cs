using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CharacterDefinition;

// 오른쪽 작은 인벤토리 화면 관리 (Inventory)
public partial class ChallengeBattleUI : BaseUI
{
    // 인벤토리 value탐색
    private int _supplyItemCount = -1;
    private bool _invenState = true;
    private int _maxInvenCount = 24;
    private bool _isOpenShortInven = false;
    private float _inventoryPopSpeed = 0.2f;

    private void InventoryInit()
    {
        // 버튼 이벤트 연결
        GetButton((int)Buttons.RelicInventoryButton).gameObject.BindEvent(OnClickRelicSelectButton);
        GetButton((int)Buttons.SupplyInventoryButton).gameObject.BindEvent(OnClickSupplySelectButton);
        GetButton((int)Buttons.ShortInvenOpenButton).gameObject.BindEvent(OnClickShortInvenOpenButton);
        GetButton((int)Buttons.ShortInvenCloseButton).gameObject.BindEvent(OnClickShortInvenCloseButton);

        // 인벤토리안 아이템 추가
        for (int i = 0; i < _maxInvenCount; i++)
        {
            UIManager.Instance.MakeSubItem<InventoryItemUI>(GetObject((int)GameObjects.InventoryRelicContent).transform);
            UIManager.Instance.MakeSubItem<InventoryItemUI>(GetObject((int)GameObjects.InventorySupplyContent).transform);
        }
        // 인벤토리 유물 비활성화
        GetObject((int)GameObjects.RelicInvenBackGround).SetActive(false);
    }

    private void InventoryUpdate()
    {
        // 인벤토리 값 관리
        if (SupplyManager.Instance.SupplyInventoryValueCount != _supplyItemCount)
        {
            _supplyItemCount = SupplyManager.Instance.SupplyInventoryValueCount;
            ConvertInventoryPanel();
        }
    }

    // 보급품 인벤토리 화면으로 화면 전환
    public void OnClickSupplySelectButton(PointerEventData data)
    {
        _invenState = true;
        ConvertInventoryPanel();
    }
    // 유물 인벤토리 화면으로 화면 전환
    public void OnClickRelicSelectButton(PointerEventData data)
    {
        _invenState = false;
        ConvertInventoryPanel();
    }
    // 작은 인벤토리창 열기 버튼
    public void OnClickShortInvenOpenButton(PointerEventData data)
    {
        // 클릭한 버튼 비활성화
        GetButton((int)Buttons.ShortInvenOpenButton).gameObject.SetActive(false);
        // 인벤토리 창 열리기
        GetObject((int)GameObjects.InvenBox).GetComponent<RectTransform>().DOAnchorPosX(0, _inventoryPopSpeed).SetEase(Ease.Linear);
        _isOpenShortInven = true;
    }
    // 작은 인벤토리창 닫기 버튼
    public void OnClickShortInvenCloseButton(PointerEventData data)
    {
        // 중복클릭으로 코루틴 여러번 실행 움직임 제한
        if (_isOpenShortInven)
        {
            StartCoroutine(CloseShortInvenUI());
        }
    }

    #region OnClickInvenInfoButtonFunc
    private void ConvertInventoryPanel()
    {
        GameObject parent = GetObject((_invenState) ? (int)GameObjects.InventorySupplyContent : (int)GameObjects.InventoryRelicContent);
        int invenCount = 0;

        // 보급품 인벤토리
        if (_invenState)
        {
            foreach (KeyValuePair<int, int> pair in SupplyManager.Instance._supplyInventory)
            {
                InventoryItemUI item = parent.transform.GetChild(invenCount).GetComponent<InventoryItemUI>();
                item.ItemCount.text = pair.Value.ToString();
                // item.ItemImage.sprite = DataManager.Instance.GetSupplyData(pair.Key).Image;
                // TMEP : 색상은 타입에 따른 색상을 따로 말씀해주신게 없어서 임의로 했음
                item.ItemType.color = new Color(0, 0, 0, 255);
                // 아이템 아이디 넣어주기
                item.ItemID = DataManager.Instance.GetSupplyData(pair.Key).Id;
                item.IsSupplyItem = true;
                invenCount++;
            }

            for (int i = invenCount; i < _maxInvenCount; i++)
            {
                InventoryItemUI item = parent.transform.GetChild(i).GetComponent<InventoryItemUI>();
                item.ItemCount.text = "";
                item.ItemImage.sprite = null;
                // TMEP : 색상은 타입에 따른 색상을 따로 말씀해주신게 없어서 임의로 했음
                item.ItemType.color = new Color(0, 0, 0, 0);
                // 아이템 아이디 넣어주기
                item.ItemID = -1;
                item.IsSupplyItem = false;
            }
        }
        // 유물 인벤토리 생성부
        else
        {

        }
        // 인벤토리 비활 활성화 담당
        if (_invenState)
        {
            // 보급품 인벤토리 창 활성화
            GetObject((int)GameObjects.SupplyInvenBackGround).SetActive(true);
            GetObject((int)GameObjects.RelicInvenBackGround).SetActive(false);
            // 비활성화 버튼 색 회색으로 변경 활성화 버튼 하얀색으로 변경
            GetButton((int)Buttons.SupplyInventoryButton).gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            GetButton((int)Buttons.RelicInventoryButton).gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
        else
        {
            // 유물 인벤토리 창 활성화
            GetObject((int)GameObjects.SupplyInvenBackGround).SetActive(false);
            GetObject((int)GameObjects.RelicInvenBackGround).SetActive(true);
            // 비활성화 버튼 색 회색으로 변경 활성화 버튼 하얀색으로 변경
            GetButton((int)Buttons.SupplyInventoryButton).gameObject.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
            GetButton((int)Buttons.RelicInventoryButton).gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }
    #endregion

    // 작은 인벤토리창 닫기
    IEnumerator CloseShortInvenUI()
    {
        _isOpenShortInven = false;
        // 화면 밖으로 이동
        GetObject((int)GameObjects.InvenBox).GetComponent<RectTransform>().DOAnchorPosX(350f, _inventoryPopSpeed).SetEase(Ease.Linear);
        yield return new WaitForSeconds(_inventoryPopSpeed);
        // 작은 인벤토리창 열기 버튼 활성화
        GetButton((int)Buttons.ShortInvenOpenButton).gameObject.SetActive(true);
    }
}