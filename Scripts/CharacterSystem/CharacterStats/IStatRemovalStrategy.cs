using UnityEngine;

public interface IStatRemovalStrategy
{
    void HandleRemoval(StatType statType, float oldValue, float newValue, DynamicStat dynamicStat);
}

public class NoAdjustmentStrategy : IStatRemovalStrategy
{
    public void HandleRemoval(StatType statType, float oldValue, float newValue, DynamicStat dynamicStat)
    {
    }
}

public class RatioPreservationStrategy : IStatRemovalStrategy
{
    public void HandleRemoval(StatType statType, float oldValue, float newValue, DynamicStat dynamicStat)
    {
        if (statType == StatType.MaxHp && oldValue > 0)
        {
            int currentHp = dynamicStat.Hp;
            float hpRatio = (float)currentHp / oldValue;
            int newHp = Mathf.Max(1, (int)(newValue * hpRatio));
            dynamicStat.SetHp(newHp);
        }
    }
}
