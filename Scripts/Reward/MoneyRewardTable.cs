using System;
using UnityEngine;

[Serializable]
public class MoneyRewardTable
{
    [SerializeField] private int _minDrop;
    [SerializeField] private int _maxDrop;

    /// <summary>
    /// 지급해야 할 재화 반환
    /// </summary>
    /// <returns></returns>
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