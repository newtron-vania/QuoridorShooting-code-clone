using System;

public class StatModifier
{
    public string Id { get; private set; }
    public StatType TargetStat { get; private set; }
    public StatModifierType ModifierType { get; private set; }
    public float Value { get; private set; }
    public int Priority { get; private set; }
    public Action OnRemove { get; private set; }

    public StatModifier(string id, StatType targetStat, StatModifierType modifierType, float value, int priority = 0, Action onRemove = null)
    {
        Id = id;
        TargetStat = targetStat;
        ModifierType = modifierType;
        Value = value;
        Priority = priority;
        OnRemove = onRemove;
    }

    public int GetTotalPriority()
    {
        return (int)ModifierType + Priority;
    }

    public float Apply(float baseValue, float currentValue)
    {
        switch (ModifierType)
        {
            case StatModifierType.FlatBaseAdd:
                return currentValue + Value;

            case StatModifierType.PercentBaseAdd:
                return currentValue + (baseValue * Value);

            case StatModifierType.FlatCurrentAdd:
                return currentValue + Value;

            case StatModifierType.PercentCurrentAdd:
                return currentValue * (1 + Value);

            default:
                return currentValue;
        }
    }
}