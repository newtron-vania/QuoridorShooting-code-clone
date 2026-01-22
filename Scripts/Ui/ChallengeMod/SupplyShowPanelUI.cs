using HM;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupplyShowPanelUI : BaseUI
{
    enum Buttons
    {
        SkipButton,
        SelectButton
    }

    enum GameObjects
    {
        SupplyShowItemsBoundary  // 보급품이 소환되어야 하는 곳
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SupplyItemUI;
    public bool IsSelect
    {
        get
        {
            return SupplyManager.Instance.SelectedSupplyItem == 0 ? false : true;
        }
    }

    // 랜덤으로 뽑은 보급품 저장
    List<SupplyItemUI> supplyItems = new List<SupplyItemUI>();

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

        // 버튼 이벤트 연결
        GetButton((int)Buttons.SkipButton).gameObject.BindEvent(OnClickSupplySkipButton);
        GetButton((int)Buttons.SelectButton).gameObject.BindEvent(OnClickSupplySelectButton);

        SpawnSupplyItem();
        SupplyManager.Instance.SupplyShowPanelUI = this;
    }

    public void OnClickSupplySkipButton(PointerEventData data)
    {
        Debug.Log($"[INFO] SupplyShowPanelUI::OnClickSupplySkipButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
        UIManager.Instance.ClosePopupUI(this);
    }

    public void OnClickSupplySelectButton(PointerEventData data)
    {
        if (IsSelect)
        {
            // 한도 초과 시 실행될 부분
            if (SupplyManager.Instance.SupplyInventoryValueCount + 1 > 99)
            {
                Debug.Log("[INFO]SupplyShowPanelUI(OnClickSupplySelectButton) - 인벤토리 안 보급품 한도가 초과했습니다.");
            }
            // 딕셔너리 아이템 등록
            else
            {
                if (SupplyManager.Instance._supplyInventory.ContainsKey(SupplyManager.Instance.SelectedSupplyItem))
                {
                    SupplyManager.Instance._supplyInventory[SupplyManager.Instance.SelectedSupplyItem] += 1;
                }
                else
                {
                    SupplyManager.Instance._supplyInventory.Add(SupplyManager.Instance.SelectedSupplyItem, 1);
                }
                // 보급품 획득 시 이벤트 발생
                EventManager.Instance.InvokeEvent(HM.EventType.OnSupplyGet, this);
            }
            Debug.Log($"[INFO] SupplyShowPanelUI::OnClickSupplySelectButton - {gameObject.GetComponent<Canvas>().sortingOrder} : 닫힘");
            UIManager.Instance.ClosePopupUI(this);
        }
    }

    public void SpawnSupplyItem()
    {
        GameObject parent = GetObject((int)GameObjects.SupplyShowItemsBoundary);
        // 이전 : 1~2개 보급품 / 이후 : 고정 3개 중 1개 택
        //int showSupplyCount = Random.Range(0, 101) < 31 ? 2 : 1; // 30퍼 확률로 보급품 2개 얻기

        // 보급품 3개 생성
        for (int i = 0; i < 3; i++)
        {
            int grade = Random.Range(0, 101);
            SupplymentData.SupplyGrade selectRank = SupplymentData.SupplyGrade.None;
            // 확률변경 일반 70%, 희귀 20%, 영웅 10%
            if (grade < 71) selectRank = SupplymentData.SupplyGrade.Normal;
            else if (grade < 91) selectRank = SupplymentData.SupplyGrade.Rare;
            else selectRank = SupplymentData.SupplyGrade.Unique;

            // 게임 오브젝트 태그에 따라 생성될 보급품 결정
            SupplymentData supplyData;
            do
            {
                supplyData = DataManager.Instance.GetSupplyData(Random.Range(0, DataManager.Instance.SupplyDatasCount));
            } while (supplyData.Grade == selectRank && Random.Range(0, 101) >= ((gameObject.tag == "BoxSupply" ? supplyData.BoxDropRate : supplyData.EnemyDropRate) * 100));

            // SupplyItemUI 리스트 저장
            supplyItems.Add(UIManager.Instance.MakeSubItem<SupplyItemUI>(parent.transform));
            supplyItems[i].SupplyId = supplyData.Id;
        }
    }

    public void SupplyItemsReset()
    {
        // 모든 아이템 UI 리셋 함수 실행
        foreach (SupplyItemUI child in supplyItems)
        {
            child.Reset();
        }
    }
}
