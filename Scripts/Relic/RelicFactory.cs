using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유물 Condition과 Effect를 생성하는 Factory
/// </summary>
public class RelicFactory
{
    // RelicInstance 생성 (ID에 따라 구체화 클래스 반환)
    public IRelic CreateRelic(RelicData data)
    {
        return data.Id switch
        {
            9001 => new AlwaysStatBoostRelic(),        // Test Relic 1: 상시 스탯 향상
            9002 => new LowHpStatBoostRelic(),         // Test Relic 2: HP 50% 이하 스탯 증가
            9003 => new DamagedBuffRelic(),            // Test Relic 3: 피격 시 2턴 버프
            _ => new AlwaysStatBoostRelic()            // 기본값
        };
    }

    // Condition 생성 (TriggerType에 따라)
    public static IRelicCondition CreateCondition(RelicTriggerType triggerType)
    {
        return triggerType switch
        {
            RelicTriggerType.Always => new AlwaysCondition(),
            RelicTriggerType.OnHpBelowThreshold => new HpThresholdCondition(),
            RelicTriggerType.OnHpChanged => new HPRatioCondition(),
            RelicTriggerType.OnDamaged => new DamagedCondition(),
            _ => new AlwaysCondition() // 기본값
        };
    }

    // Effect 생성 (EffectType에 따라)
    public static IRelicEffect CreateEffect(RelicEffectType effectType)
    {
        return effectType switch
        {
            RelicEffectType.StatMultiplier => new StatMultiplierEffect(),
            RelicEffectType.StatFlat => new StatFlatEffect(),
            RelicEffectType.TimedStatModifier => new TimedStatModifierEffect(),
            RelicEffectType.ConditionalStatBoost => new ConditionalStatBoostEffect(),
            RelicEffectType.RewardMultiplier => new RewardMultiplierEffect(),
            RelicEffectType.OnDeathHeal => new OnDeathHealEffect(),
            RelicEffectType.CreatePoisonTile => new CreatePoisonTileEffect(),
            _ => new StatMultiplierEffect() // 기본값
        };
    }
}

// ========== Condition 구현 ==========

/// <summary>
/// Always 조건 (항상 true)
/// </summary>
public class AlwaysCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, float> conditionParams)
    {
        return true;
    }
}

/// <summary>
/// HP Threshold 조건
/// </summary>
public class HpThresholdCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, float> conditionParams)
    {
        if (signal?.Subject is not BaseCharacter character) return false;

        float threshold = conditionParams != null && conditionParams.ContainsKey("HpThreshold")
            ? conditionParams["HpThreshold"]
            : 0.5f;

        float hpRatio = (float)character.Hp / character.MaxHp;
        return hpRatio <= threshold;
    }
}

/// <summary>
/// HP Ratio 조건 (HP 변화 시 비율 체크)
/// </summary>
public class HPRatioCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, float> conditionParams)
    {
        if (signal?.Subject is not BaseCharacter character) return false;

        float threshold = conditionParams != null && conditionParams.ContainsKey("HpThreshold")
            ? conditionParams["HpThreshold"]
            : 0.5f;

        float hpRatio = (float)character.Hp / character.MaxHp;
        return hpRatio <= threshold;
    }
}

/// <summary>
/// 피격 조건
/// </summary>
public class DamagedCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, float> conditionParams)
    {
        if (signal?.RuntimeParams == null) return false;

        // 최소 데미지 체크
        if (conditionParams != null && conditionParams.ContainsKey("MinDamage"))
        {
            float minDamage = conditionParams["MinDamage"];
            if (signal.RuntimeParams.ContainsKey("DamageAmount"))
            {
                return signal.RuntimeParams["DamageAmount"] >= minDamage;
            }
            return false;
        }

        return true;
    }
}

// ========== Effect 구현 (Stub) ==========

/// <summary>
/// 스탯 퍼센트 증가 효과
/// </summary>
public class StatMultiplierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        // TODO: Phase 2에서 구현
        Debug.Log($"[StatMultiplierEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 스탯 고정값 증가 효과
/// </summary>
public class StatFlatEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[StatFlatEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 일정 턴 동안 스탯 증가 효과
/// </summary>
public class TimedStatModifierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[TimedStatModifierEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 조건부 스탯 증가 효과
/// </summary>
public class ConditionalStatBoostEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[ConditionalStatBoostEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 보상 증폭 효과
/// </summary>
public class RewardMultiplierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[RewardMultiplierEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 사망 시 주변 회복 효과
/// </summary>
public class OnDeathHealEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[OnDeathHealEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 독 타일 생성 효과
/// </summary>
public class CreatePoisonTileEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams)
    {
        Debug.Log($"[CreatePoisonTileEffect] Apply to {target}");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}
