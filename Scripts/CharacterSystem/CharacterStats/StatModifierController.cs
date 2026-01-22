using System;
using System.Collections.Generic;
using System.Linq;

public class StatModifierController
{
    private BaseStat _baseStat;
    private DynamicStat _dynamicStat;
    private Dictionary<StatType, List<StatModifier>> _modifiers;
    private IStatRemovalStrategy _removalStrategy;

    // 스탯 간 종속 관계 정의 (주 스탯 → 종속 스탯)
    private static readonly Dictionary<StatType, StatType> _dependentStats = new Dictionary<StatType, StatType>
    {
        { StatType.MaxHp, StatType.Hp } // MaxHp 변동 시 현재 Hp도 조정
    };

    // 각 스탯별 마지막 적용된 값 추적
    private Dictionary<StatType, float> _lastAppliedValues;

    public StatModifierController(BaseStat baseStat, DynamicStat dynamicStat, IStatRemovalStrategy removalStrategy = null)
    {
        _baseStat = baseStat;
        _dynamicStat = dynamicStat;
        _removalStrategy = removalStrategy ?? new RatioPreservationStrategy();
        _modifiers = new Dictionary<StatType, List<StatModifier>>();
        _lastAppliedValues = new Dictionary<StatType, float>();

        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            _modifiers[statType] = new List<StatModifier>();
            _lastAppliedValues[statType] = GetBaseValue(statType);
        }
    }

    public void SetRemovalStrategy(IStatRemovalStrategy strategy)
    {
        _removalStrategy = strategy ?? new RatioPreservationStrategy();
    }

    public void AddModifier(StatModifier modifier)
    {
        _modifiers[modifier.TargetStat].Add(modifier);
        _modifiers[modifier.TargetStat] = _modifiers[modifier.TargetStat]
            .OrderBy(m => m.GetTotalPriority())
            .ToList();

        RecalculateStat(modifier.TargetStat, modifier);
    }

    public bool RemoveModifier(string modifierId)
    {
        foreach (var statType in _modifiers.Keys)
        {
            var modifier = _modifiers[statType].FirstOrDefault(m => m.Id == modifierId);
            if (modifier != null)
            {
                _modifiers[statType].Remove(modifier);
                modifier.OnRemove?.Invoke();
                RecalculateStat(statType, modifier);
                return true;
            }
        }
        return false;
    }

    public void ClearModifiers(StatType statType)
    {
        _modifiers[statType].Clear();
        RecalculateStat(statType);
    }

    public void ClearAllModifiers()
    {
        foreach (var statType in _modifiers.Keys)
        {
            _modifiers[statType].Clear();
        }
        RecalculateAllStats();
    }

    public float GetCalculatedValue(StatType statType)
    {
        return CalculateStat(statType);
    }

    private float CalculateStat(StatType statType)
    {
        float baseValue = GetBaseValue(statType);
        float currentValue = baseValue;

        foreach (var modifier in _modifiers[statType])
        {
            currentValue = modifier.Apply(baseValue, currentValue);
        }

        return currentValue;
    }

    private void RecalculateStat(StatType statType, StatModifier lastModifier = null)
    {
        float calculatedValue = CalculateStat(statType);

        // 종속 스탯이 있는 경우, 변화량을 추적하여 함께 조정
        if (_dependentStats.ContainsKey(statType))
        {
            float oldValue = _lastAppliedValues[statType];
            ApplyToDynamicStat(statType, calculatedValue);
            float newValue = calculatedValue;

            // 실제 변화가 있을 때만 종속 스탯 조정
            if (System.Math.Abs(newValue - oldValue) > 0.001f)
            {
                SyncDependentStat(statType, oldValue, newValue, lastModifier);
                _lastAppliedValues[statType] = newValue;
            }
        }
        else
        {
            ApplyToDynamicStat(statType, calculatedValue);
        }
    }

    private void RecalculateAllStats()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            RecalculateStat(statType);
        }
    }

    private float GetBaseValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp: return _baseStat.MaxHp;
            case StatType.Atk: return _baseStat.Atk;
            case StatType.Avd: return _baseStat.Avd;
            case StatType.Ap: return _baseStat.BaseAp;
            case StatType.ApRecovery: return _baseStat.ApRecovery;
            case StatType.CharacterType: return (int)_baseStat.Type;
            case StatType.CharacterClass: return (int)_baseStat.Class;
            case StatType.SkillId: return _baseStat.SkillId;
            default: return 0;
        }
    }

    private void ApplyToDynamicStat(StatType statType, float value)
    {
        switch (statType)
        {
            case StatType.MaxHp:
                _dynamicStat.SetMaxHp((int)value);
                break;
            case StatType.Atk:
                _dynamicStat.SetAtk((int)value);
                break;
            case StatType.Avd:
                _dynamicStat.SetAvd(value);
                break;
            case StatType.Ap:
                _dynamicStat.SetBaseAp((int)value);
                break;
            case StatType.ApRecovery:
                _dynamicStat.SetApRecovery((int)value);
                break;
            case StatType.CharacterType:
                _dynamicStat.SetType((CharacterDefinition.CharacterType)(int)value);
                break;
            case StatType.CharacterClass:
                _dynamicStat.SetClass((CharacterDefinition.CharacterClass)(int)value);
                break;
            case StatType.SkillId:
                _dynamicStat.SetSkillId((int)value);
                break;
        }
    }

    private void HandleRemoval(StatType statType, float oldValue, float newValue)
    {
        _removalStrategy.HandleRemoval(statType, oldValue, newValue, _dynamicStat);
    }

    public List<StatModifier> GetModifiers(StatType statType)
    {
        return new List<StatModifier>(_modifiers[statType]);
    }

    public bool HasModifier(string modifierId)
    {
        return _modifiers.Values.Any(list => list.Any(m => m.Id == modifierId));
    }

    // 종속 스탯 동기화
    private void SyncDependentStat(StatType primaryStat, float oldValue, float newValue, StatModifier lastModifier)
    {
        if (primaryStat == StatType.MaxHp)
        {
            SyncHpWithMaxHp(oldValue, newValue, lastModifier);
        }
    }

    // MaxHp 변동 시 Hp 조정
    private void SyncHpWithMaxHp(float oldMaxHp, float newMaxHp, StatModifier lastModifier)
    {
        int currentHp = _dynamicStat.Hp;
        int newHp = currentHp;

        if (lastModifier == null)
        {
            // No modifier: clamp HP to new MaxHp (at least 1)
            newHp = System.Math.Max(1, System.Math.Min((int)newMaxHp, currentHp));
        }
        else
        {
            switch (lastModifier.ModifierType)
            {
                case StatModifierType.FlatBaseAdd:
                case StatModifierType.FlatCurrentAdd:
                    // 덧셈 연산
                    float flatDelta = newMaxHp - oldMaxHp;
                    newHp = currentHp + (int)flatDelta;
                    break;

                case StatModifierType.PercentBaseAdd:
                case StatModifierType.PercentCurrentAdd:
                    // 곱셈 연산
                    if (oldMaxHp > 0)
                    {
                        float ratio = newMaxHp / oldMaxHp;
                        newHp = (int)(currentHp * ratio);
                    }
                    break;
            }
            newHp = System.Math.Max(1, System.Math.Min((int)newMaxHp, newHp));
        }
        _dynamicStat.SetHp(newHp);
    }
}
