using System;
using UnityEngine;

enum BattleType
{
    None   = -1,
    Normal = 0,
    Elite  = 1,
    Boss   = 2
}

[Serializable]
public class BattleRewardTable
{
    [SerializeField] private float _artifactProbability;
    [SerializeField] private float _supplymentProbability;
    [SerializeField] private float _moneyProbability;

    public BattleRewardTable(float artifactProbability = 0f, float supplymantProbability = 0f, float moneyProbability = 0f)
    {
        _artifactProbability   = artifactProbability;
        _supplymentProbability = supplymantProbability;
        _moneyProbability      = moneyProbability;
    }

    /// <summary>
    /// 보상 타입 반환
    /// </summary>
    /// <returns></returns>
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
