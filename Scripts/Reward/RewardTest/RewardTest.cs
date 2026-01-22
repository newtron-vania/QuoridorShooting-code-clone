using UnityEngine;

public class RewardTest : MonoBehaviour
{
    [SerializeField] private DropMoneyUI _dropMoneyUI;
    [SerializeField] private RewardUI    _rewardUI;

    public void OpenDropMoneyUI()
    {
        _dropMoneyUI.OpenUI();
    }

    public void OpenRewardUI()
    {
        _rewardUI.ShowRewardPanel();
    }
}
