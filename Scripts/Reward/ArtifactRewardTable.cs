using System;
using UnityEngine;

[Serializable]
public class ArtifactRewardTable
{
    [SerializeField] private float _normalProbability;
    [SerializeField] private float _rareProbability;
    [SerializeField] private float _uniqueProbability;

    public ArtifactRewardTable(float normalProbability = 0f, float rareProbability = 0f, float uniqueProbability = 0f)
    {
        _normalProbability = normalProbability;
        _rareProbability = rareProbability;
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