using System;
using UnityEngine;

public enum GradeType
{
    None   = -1,
    Normal = 0,
    Rare   = 1,
    Unique = 2,
}

public enum RewardType
{
    None       = -1,
    Artifact   = 0,
    Supplyment = 1,
    Money      = 2,
}


public class RewardPool : MonoBehaviour
{
    [Header("----- Reward Table ------")]
    [SerializeField] private BattleRewardTable[] _battleRewardTables;
    [SerializeField] private SupplymentRewardTable[] _supplymentRewardTables;
    [SerializeField] private ArtifactRewardTable[] _artifactRewardTables;
    [SerializeField] private MoneyRewardTable[] _selectMoneyRewardTables;
    [SerializeField] private DefaultMoneyRewardTable[] _defaultMoneyRewardTables;

    [Header("----- Value ------")]
    [SerializeField] private BattleType _battleType;
    [SerializeField] private Sprite _sprite;

    private SupplymentPool _supplymentPool;

    public RewardCardData? SelectItem = null;
    public int SelectItemIdx = -1;

    private void Start()
    {
        _supplymentPool = new();
    }

    /// <summary>
    /// 확정 드랍 재화 관련 변수 반환
    /// </summary>
    /// <param name="Money"></param>
    /// <param name="Duration"></param>
    public void GetDropMoneyInstance(ref int Money, ref float Duration)
    {
        int index = (int)_battleType;

        Money = _defaultMoneyRewardTables[index].GetMoney();
        Duration = _defaultMoneyRewardTables[index].Duration;
    }

    /// <summary>
    /// 리워드 풀 초기화
    /// </summary>
    public void InitRewardPool()
    {
        _supplymentPool.ListShuffle();
    }

    /// <summary>
    /// 보상 카드 데이터 반환
    /// </summary>
    /// <returns></returns>
    public RewardCardData GetRewardCardData()
    {
        int battleType = (int)_battleType;

        int        index       = 0;
        int        amount      = 1;
        RewardType category    = _battleRewardTables[battleType].GetRewardType();
        GradeType  grade       = GradeType.Normal;
        string     name        = "";
        Sprite     image       = _sprite;
        string     description = "";

        switch (category)
        {
            case RewardType.Supplyment:
                grade = _supplymentRewardTables[battleType].GetRewardGrade();
                SupplymentData supplyment = _supplymentRewardTables[battleType].GetSupplyment(_supplymentPool, grade);
                index                        = supplyment.Id;
                name                      = supplyment.Name;
                description               = supplyment.Description;
                break;
            case RewardType.Artifact:
                grade = _artifactRewardTables[battleType].GetRewardGrade();
                name = "유물";
                description = "유물이다";
                break;
            case RewardType.Money:
                grade = GradeType.Normal;
                name = "돈";
                amount = _selectMoneyRewardTables[battleType].GetMoney();
                description = amount.ToString();
                break;
        }

        RewardCardData data = new RewardCardData(index, amount, grade, category, name, image, description);
        return data;
    }

}
