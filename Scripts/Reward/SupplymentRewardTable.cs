using System;
using UnityEngine;

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

    /// <summary>
    /// 보급품 등급 반환
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 보급품 등급에 따른 보급품 반환
    /// </summary>
    /// <param name="dataList"></param>
    /// <param name="grade"></param>
    /// <returns></returns>
    public SupplymentData GetSupplyment(SupplymentPool dataList, GradeType grade)
    {
        return dataList.Pop(grade);
    }
}
