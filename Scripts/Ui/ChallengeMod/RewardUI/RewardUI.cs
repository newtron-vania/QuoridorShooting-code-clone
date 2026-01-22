using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : MonoBehaviour
{
    [Header("---- Reward Panel -----")]
    [SerializeField] private GameObject _rewardPanel;

    [Header("---- Reward Cards -----")]
    [SerializeField] private RewardCard[] _rewardCards;
    [SerializeField] private int          _selectedIndex = -1;

    [Header("---- NextButton -----")]
    [SerializeField] private Button _nextButton;

    [Header("------ Model ------")]
    [SerializeField] private RewardPool    _rewardPool;
    [SerializeField] private TestInventory _testInventory;

    private void Awake()
    {
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            _rewardCards[i].SetIndex(i);
            _rewardCards[i].OnCardClicked += SelectCard;
        }

        _nextButton.onClick.AddListener(ClickNextButton);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            _rewardCards[i].OnCardClicked -= SelectCard;
        }

        _nextButton.onClick.RemoveAllListeners();
    }

    public void ShowRewardPanel()
    {
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            _rewardCards[i].ToggleDisablePanel(false);
        }

        ToggleNextButton(false);

        _selectedIndex = -1;

        _rewardPool.InitRewardPool();
        //열기 전 보상 세팅
        for (int i = 0; i < _rewardCards.Length; i++)
        {
            RewardCardData data = _rewardPool.GetRewardCardData();
            _rewardCards[i].SetRewardCard(data);
        }

        _rewardPanel.SetActive(true);
    }

    public void CloseRewardPanel()
    {
        _rewardPanel.SetActive(false);
    }

    /// <summary>
    /// 완료 버튼 클릭 시 함수
    /// </summary>
    public void ClickNextButton()
    {
        GetReward();

        CloseRewardPanel();
    }

    //보상 반환
    private void GetReward()
    {
        RewardCard card = _rewardCards[_selectedIndex];

        RewardType rewardType = card.Category;
        int        amount     = card.Amount;
        int        id         = card.Id;

        switch (rewardType)
        {
            case RewardType.Artifact:
                //유물은 아직 구현 X
                break;
            case RewardType.Supplyment:

                Dictionary<int, int> inventory = SupplyManager.Instance._supplyInventory;

                if (inventory.TryGetValue(id, out int currentVal))
                {
                    inventory[id] = currentVal + amount;
                }
                else
                {
                    inventory[id] = amount;
                }
                break;
            case RewardType.Money:
                _testInventory.UpdateMoney(amount);
                break;
        }
    }

    private void ToggleNextButton(bool isActive)
    {
        _nextButton.interactable = isActive;
    }

    private void SelectCard(int index)
    {
        //재선택 시
        if(index == _selectedIndex)
        {
            _selectedIndex = -1;
            for (int i = 0; i < _rewardCards.Length; i++)
            {
                _rewardCards[i].ToggleDisablePanel(false);
            }

            ToggleNextButton(false); 
        }
        else
        {
            for (int i = 0; i < _rewardCards.Length; i++)
            {
                _rewardCards[i].ToggleDisablePanel(true);
            }

            _rewardCards[index].ToggleDisablePanel(false);
            _selectedIndex = index;

            ToggleNextButton(true);
        }
    }
}
