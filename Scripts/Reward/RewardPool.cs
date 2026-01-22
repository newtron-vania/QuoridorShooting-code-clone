using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

enum BattleType
{
    None = -1,
    Normal = 0,
    Elite = 1,
    Boss = 2
}
public enum GradeType
{
    None = -1,
    Normal = 0,
    Rare = 1,
    Unique = 2,
}

public enum RewardType
{
    None = -1,
    Artifact = 0,
    Supplyment = 1,
    Money = 2,
}

[Serializable]
public class BattleRewardTable
{
    [SerializeField] private float _artifactProbability;
    [SerializeField] private float _supplymentProbability;
    [SerializeField] private float _moneyProbability;

    public BattleRewardTable(float artifactProbability = 0f, float supplymantProbability = 0f, float moneyProbability = 0f)
    {
        _artifactProbability = artifactProbability;
        _supplymentProbability = supplymantProbability;
        _moneyProbability = moneyProbability;
    }

    public RewardType GetRewardType()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        if (randomValue <= _artifactProbability)
        {
            return RewardType.Artifact;
        }
        randomValue -= _artifactProbability;

        if (randomValue <= _supplymentProbability)
        {
            return RewardType.Supplyment;
        }

        return RewardType.Money;
    }
}

[Serializable]
public class ArtifactRewardTable
{
    [SerializeField] private float _normalProbability;
    [SerializeField] private float _rareProbability;
    [SerializeField] private float _uniqueProbability;

    public ArtifactRewardTable(float normalProbability = 0f, float rareProbability = 0f, float uniqueProbability = 0f)
    {
        _normalProbability = normalProbability;
        _rareProbability   = rareProbability;
        _uniqueProbability = uniqueProbability;
    }

    public GradeType GetRewardGrade()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        if (randomValue <= _normalProbability)
        {
            return GradeType.Normal;
        }
        randomValue -= _normalProbability;

        if (randomValue <= _rareProbability)
        {
            return GradeType.Rare;
        }

        return GradeType.Unique;
    }
}

[Serializable]
public class SupplymentPool
{
    private List<SupplymentData> _normalShuffleList;
    private List<SupplymentData> _rareShuffleList;
    private List<SupplymentData> _uniqueShuffleList;

    private List<SupplymentData> _normalData = new();
    private List<SupplymentData> _rareData   = new();
    private List<SupplymentData> _uniqueData = new();

    public SupplymentPool()
    {
        DataManager dataManager = DataManager.Instance;
        int supplyDatasCount = dataManager.SupplyDatasCount;
        SupplymentData supplyData;

        Debug.Log(supplyDatasCount);
        for (int i = 1; i <= supplyDatasCount; i++)
        {
            supplyData = dataManager.GetSupplyData(i);

            switch (supplyData.Grade)
            {
                case SupplymentData.SupplyGrade.Normal:
                    _normalData.Add(supplyData);
                    break;
                case SupplymentData.SupplyGrade.Rare:
                    _rareData.Add(supplyData);
                    break;
                case SupplymentData.SupplyGrade.Unique:
                    _uniqueData.Add(supplyData);
                    break;
            }
        }
    }

    public void ListShuffle()
    {
        ClearShuffleList();
        _normalShuffleList = _normalData.ToList();
        _rareShuffleList   = _rareData.ToList();
        _uniqueShuffleList = _uniqueData.ToList();

        Shuffle(_normalShuffleList);
        Shuffle(_rareShuffleList);
        Shuffle(_uniqueShuffleList);
    }

    public void ClearShuffleList()
    {
        if(_normalShuffleList != null) _normalShuffleList.Clear();
        if(_rareShuffleList   != null) _rareShuffleList.Clear();
        if(_uniqueShuffleList != null) _uniqueShuffleList.Clear();
    }

    public SupplymentData Pop(GradeType grade)
    {
        List<SupplymentData> supplymentList;
        switch (grade)
        {
            case GradeType.Normal:
                supplymentList = _normalShuffleList;
                break;
            case GradeType.Rare:
                supplymentList = _rareShuffleList;
                break;
            case GradeType.Unique:
                supplymentList = _uniqueShuffleList;
                break;
            default:
                supplymentList = new();
                break;
        }

        int end = supplymentList.Count - 1;
        SupplymentData supplyData = supplymentList[end];
        supplymentList.RemoveAt(end);

        return supplyData;
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;

        //맨 뒤부터 자리 확정
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);

            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}

[Serializable]
public class SupplymentRewardTable
{
    [SerializeField] private float _normalProbability;
    [SerializeField] private float _rareProbability;
    [SerializeField] private float _uniqueProbability;

    public SupplymentRewardTable(float normalProbability = 0f, float rareProbability = 0f, float uniqueProbability = 0f)
    {
        _normalProbability = normalProbability;
        _rareProbability   = rareProbability;
        _uniqueProbability = uniqueProbability;
    }

    public GradeType GetRewardGrade()
    {
        float randomValue = UnityEngine.Random.Range(0f, 100f);

        if (randomValue <= _normalProbability)
        {
            return GradeType.Normal;
        }
        randomValue -= _normalProbability;

        if (randomValue <= _rareProbability)
        {
            return GradeType.Rare;
        }

        return GradeType.Unique;
    }

    public SupplymentData GetSupplyment(SupplymentPool dataList, GradeType grade)
    {
        return dataList.Pop(grade);
    }
}


[Serializable]
public class DefaultMoneyRewardTable : MoneyRewardTable
{
    [SerializeField] private float _duration;

    public float Duration => _duration;

    public DefaultMoneyRewardTable(int minDrop = 0, int maxDrop = 0, float duration = 0f) : base(minDrop, maxDrop)
    {
        _duration = duration;
    }
}

[Serializable]
public class MoneyRewardTable
{
    [SerializeField] private int _minDrop;
    [SerializeField] private int _maxDrop;

    public int GetMoney()
    {
        return UnityEngine.Random.Range(_minDrop, _maxDrop + 1);
    }

    public MoneyRewardTable(int minDrop = 0, int maxDrop = 0)
    {
        _minDrop = minDrop;
        _maxDrop = maxDrop;
    }
}

public class RewardPool : MonoBehaviour
{
    [Header("----- Reward Table ------")]
    [SerializeField] private BattleRewardTable[]       _battleRewardTables;
    [SerializeField] private SupplymentRewardTable[]   _supplymentRewardTables;
    [SerializeField] private ArtifactRewardTable[]     _artifactRewardTables;
    [SerializeField] private MoneyRewardTable[]        _selectMoneyRewardTables;
    [SerializeField] private DefaultMoneyRewardTable[] _defaultMoneyRewardTables;

    [Header("----- Value ------")]
    [SerializeField] private BattleType _battleType;
    [SerializeField] private Sprite     _sprite;

    private SupplymentPool _supplymentPool;

    public RewardCardData? SelectItem = null;

    private void Start()
    {
        _supplymentPool = new();
    }

    public void GetDropMoneyInstance(ref int Money, ref float Duration)
    {
        int index = (int)_battleType;

        Money    = _defaultMoneyRewardTables[index].GetMoney();
        Duration = _defaultMoneyRewardTables[index].Duration;
    }

    public void InitRewardPool()
    {
        _supplymentPool.ListShuffle();
    }

    public RewardCardData GetRewardCardData()
    {
        int battleType = (int)_battleType;

        int        id          = 0;
        int        amount      = 1;
        RewardType category    = _battleRewardTables[battleType].GetRewardType();
        GradeType  grade       = GradeType.Normal;
        string     name        = "";
        Sprite     image       = _sprite;
        string     description = "";

        switch (category)
        {
            case RewardType.Supplyment:
                grade                     = _supplymentRewardTables[battleType].GetRewardGrade();
                SupplymentData supplyment = _supplymentRewardTables[battleType].GetSupplyment(_supplymentPool, grade);
                id                        = supplyment.Id;
                name                      = supplyment.Name;
                description               = supplyment.Description;
                break;
            case RewardType.Artifact:
                grade       = _artifactRewardTables[battleType].GetRewardGrade();
                name        = "유물";
                description = "유물이다";
                break;
            case RewardType.Money:
                grade       = GradeType.Normal;
                name        = "돈";
                amount      = _selectMoneyRewardTables[battleType].GetMoney();
                description = amount.ToString();
                break;
        }

        RewardCardData data = new RewardCardData(id, amount, grade, category, name, image, description);
        return data;
    }

}
