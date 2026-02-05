using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// 유물 Condition과 Effect를 생성하는 Factory
/// Phase 2B에서 DataDrivenRelicInstance 기본 반환으로 전환 예정
/// </summary>
public class RelicFactory
{
    // RelicInstance 생성
    // Phase 2: Effects 필드가 있으면 DataDrivenRelicInstance 반환
    // Phase 1 하위호환: 하드코딩 유물 (9001-9003) 유지
    public IRelic CreateRelic(RelicData data)
    {
        // Phase 1 하위호환 유물
        switch (data.Id)
        {
            case 9001: return new AlwaysStatBoostRelic();
            case 9002: return new LowHpStatBoostRelic();
            case 9003: return new DamagedBuffRelic();
        }

        // Phase 2: 데이터 주도 유물 (기본 경로)
        return new DataDrivenRelicInstance();
    }

    // Condition 생성 (TriggerType에 따라) — 계층 1
    public static IRelicCondition CreateCondition(RelicTriggerType triggerType)
    {
        return triggerType switch
        {
            RelicTriggerType.Always => new AlwaysCondition(),
            RelicTriggerType.OnHpBelowThreshold => new HpThresholdCondition(),
            RelicTriggerType.OnHpChanged => new HPRatioCondition(),
            RelicTriggerType.OnDamaged => new DamagedCondition(),
            _ => new AlwaysCondition()
        };
    }

    // Effect 생성 (EffectType에 따라)
    public static IRelicEffect CreateEffect(RelicEffectType effectType)
    {
        return effectType switch
        {
            // 스탯 계열
            RelicEffectType.StatFlat => new StatFlatEffect(),
            RelicEffectType.StatMultiplier => new StatMultiplierEffect(),
            RelicEffectType.TimedStatModifier => new TimedStatModifierEffect(),
            RelicEffectType.ConditionalStatBoost => new ConditionalStatBoostEffect(),
            RelicEffectType.EnemyStatFlat => new EnemyStatFlatEffect(),

            // 상태이상 계열
            RelicEffectType.ApplyStatusEffect => new ApplyStatusEffectEffect(),
            RelicEffectType.ApplyStatusToAttacker => new ApplyStatusToAttackerEffect(),
            RelicEffectType.ApplyStatusOnAllyStatus => new ApplyStatusOnAllyStatusEffect(),

            // 전투 효과
            RelicEffectType.HealOnEvent => new HealOnEventEffect(),
            RelicEffectType.DamageReduction => new DamageReductionEffect(),
            RelicEffectType.DamageReflect => new DamageReflectEffect(),
            RelicEffectType.OnDeathHeal => new OnDeathHealEffect(),
            RelicEffectType.OnDeathExplosion => new OnDeathExplosionEffect(),
            RelicEffectType.ExtraAttack => new ExtraAttackEffect(),
            RelicEffectType.TransferDamage => new TransferDamageEffect(),

            // 자원/보상
            RelicEffectType.ApBoost => new ApBoostEffect(),
            RelicEffectType.RewardMultiplier => new RewardMultiplierEffect(),
            RelicEffectType.RewardAdditional => new RewardAdditionalEffect(),
            RelicEffectType.ResourceGain => new ResourceGainEffect(),
            RelicEffectType.ShopDiscount => new ShopDiscountEffect(),

            // 기타
            RelicEffectType.CooldownReduction => new CooldownReductionEffect(),
            RelicEffectType.BuffSpread => new BuffSpreadEffect(),
            RelicEffectType.CleanseOnBuff => new CleanseOnBuffEffect(),
            RelicEffectType.CreatePoisonTile => new CreatePoisonTileEffect(),
            RelicEffectType.CreateHealTile => new CreateHealTileEffect(),
            RelicEffectType.SynergyOnAcquire => new SynergyOnAcquireEffect(),

            _ => new StatFlatEffect()
        };
    }
}

// ========== Condition 구현 (계층 1 — JToken 기반) ==========

public class AlwaysCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, JToken> conditionParams)
    {
        return true;
    }
}

public class HpThresholdCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, JToken> conditionParams)
    {
        if (signal?.Subject is not BaseCharacter character) return false;

        float threshold = 0.5f;
        if (conditionParams != null && conditionParams.TryGetValue("HpThreshold", out var t))
            threshold = t.Value<float>();

        float hpRatio = (float)character.Hp / character.MaxHp;
        return hpRatio <= threshold;
    }
}

public class HPRatioCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, JToken> conditionParams)
    {
        if (signal?.Subject is not BaseCharacter character) return false;

        float threshold = 0.5f;
        if (conditionParams != null && conditionParams.TryGetValue("HpThreshold", out var t))
            threshold = t.Value<float>();

        float hpRatio = (float)character.Hp / character.MaxHp;
        return hpRatio <= threshold;
    }
}

public class DamagedCondition : IRelicCondition
{
    public bool IsSatisfied(RelicSignal signal, Dictionary<string, JToken> conditionParams)
    {
        if (signal?.RuntimeParams == null) return false;

        if (conditionParams != null && conditionParams.TryGetValue("MinDamage", out var minDmg))
        {
            float minDamage = minDmg.Value<float>();
            if (signal.RuntimeParams.TryGetValue("DamageAmount", out float dmgAmount))
                return dmgAmount >= minDamage;
            return false;
        }

        return true;
    }
}

// ═══════════════════════════════════════════════════════════════
//  Effect 실구현 — Phase 2C
//  공통 헬퍼: RelicEffectHelper (하단)
// ═══════════════════════════════════════════════════════════════

// ────────── 유틸 ──────────

/// <summary>
/// Effect 구현체가 공통으로 사용하는 헬퍼
/// </summary>
public static class RelicEffectHelper
{
    private static int _modifierCounter;

    /// <summary>
    /// 유일한 StatModifier ID 생성
    /// </summary>
    public static string GenerateModifierId(string prefix, int targetId)
        => $"relic_{prefix}_{targetId}_{_modifierCounter++}";

    /// <summary>
    /// resolvedParams에서 StatType 파싱
    /// </summary>
    public static bool TryGetStatType(Dictionary<string, JToken> p, out StatType statType)
    {
        statType = StatType.Atk;
        if (p == null || !p.TryGetValue("StatType", out var token)) return false;
        return Enum.TryParse(token.Value<string>(), true, out statType);
    }

    /// <summary>
    /// resolvedParams에서 float 값 가져오기
    /// </summary>
    public static float GetFloat(Dictionary<string, JToken> p, string key, float fallback = 0f)
    {
        if (p != null && p.TryGetValue(key, out var token))
            return token.Value<float>();
        return fallback;
    }

    /// <summary>
    /// resolvedParams에서 int 값 가져오기
    /// </summary>
    public static int GetInt(Dictionary<string, JToken> p, string key, int fallback = 0)
    {
        if (p != null && p.TryGetValue(key, out var token))
            return token.Value<int>();
        return fallback;
    }

    public static string GetString(Dictionary<string, JToken> p, string key, string fallback = "")
    {
        if (p != null && p.TryGetValue(key, out var token))
            return token.Value<string>();
        return fallback;
    }

    /// <summary>
    /// 상태이상 이름 → ID 해석
    /// StatuseffectId가 직접 지정되면 사용, 없으면 StatusName에서 해석
    /// </summary>
    public static int ResolveStatuseffectId(Dictionary<string, JToken> p)
    {
        // StatuseffectId 직접 지정 우선
        int id = GetInt(p, "StatuseffectId");
        if (id > 0) return id;

        // StatusName 문자열로 해석
        string name = GetString(p, "StatusName");
        if (string.IsNullOrEmpty(name)) return 0;

        return StatuseffectNameToId(name);
    }

    /// <summary>
    /// 상태이상 이름 → ID 매핑
    /// DataManager의 실제 데이터와 동기화 필요
    /// </summary>
    private static readonly Dictionary<string, int> _statusNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // === CC (군중제어) ===
        { "Blind",           1 },  // 실명
        { "Bind",            2 },  // 속박
        { "Frozen",          3 },  // 빙결
        { "Stun",            4 },  // 기절

        // === DeBuff ===
        { "Irresistible",    5 },  // 저항불가
        { "Exhaust",         6 },  // 탈진
        { "Weaken",          7 },  // 쇠약
        { "Poison",          8 },  // 중독

        // === DoT (지속 피해) ===
        { "Burn",            9 },  // 화상

        // === Buff ===
        { "GuaranteedHit",  10 },  // 필중
        { "Accelerate",     11 },  // 가속
        { "Cleanse",        12 },  // 정화
        { "DamageImmune",   13 },  // 피해 면역
    };

    public static int StatuseffectNameToId(string name)
    {
        if (_statusNameMap.TryGetValue(name, out int id))
            return id;

        Debug.LogWarning($"[RelicEffectHelper] Unknown StatusName: '{name}' → defaulting to 0");
        return 0;
    }
}

// ────────── 스탯 계열 ──────────

/// <summary>
/// 고정값 스탯 변경
/// Params: { "StatType": "Atk", "Value": 10 }
/// </summary>
public class StatFlatEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        string modId = RelicEffectHelper.GenerateModifierId("flat", character.Id);

        var modifier = new StatModifier(modId, statType, StatModifierType.FlatBaseAdd, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 비율 스탯 변경
/// Params: { "StatType": "Atk", "Value": 0.2 }  → Atk +20%
/// </summary>
public class StatMultiplierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        string modId = RelicEffectHelper.GenerateModifierId("mult", character.Id);

        var modifier = new StatModifier(modId, statType, StatModifierType.PercentBaseAdd, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 턴 제한 스탯 변경
/// Params: { "StatType": "Atk", "Value": 5, "ModifierType": "FlatBaseAdd", "Duration": 3 }
/// </summary>
public class TimedStatModifierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        int duration = RelicEffectHelper.GetInt(resolvedParams, "Duration", 1);

        string modTypeStr = RelicEffectHelper.GetString(resolvedParams, "ModifierType", "FlatBaseAdd");
        if (!Enum.TryParse<StatModifierType>(modTypeStr, true, out var modType))
            modType = StatModifierType.FlatBaseAdd;

        string modId = RelicEffectHelper.GenerateModifierId("timed", character.Id);
        var modifier = new StatModifier(modId, statType, modType, value);
        StatManager.Instance.AddModifierWithDuration(character.Id, modifier, duration);

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        // 시간제 modifier는 StatManager가 자동 만료 처리
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 조건부 스탯 증가 (StatFlat과 동일 로직, 가역 효과로 주로 사용)
/// Params: { "StatType": "Atk", "Value": 10, "ModifierType": "FlatBaseAdd" }
/// </summary>
public class ConditionalStatBoostEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");

        string modTypeStr = RelicEffectHelper.GetString(resolvedParams, "ModifierType", "FlatBaseAdd");
        if (!Enum.TryParse<StatModifierType>(modTypeStr, true, out var modType))
            modType = StatModifierType.FlatBaseAdd;

        string modId = RelicEffectHelper.GenerateModifierId("cond", character.Id);
        var modifier = new StatModifier(modId, statType, modType, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 적 캐릭터 고정 스탯 변경
/// Params: { "StatType": "Atk", "Value": -5 }  → 적 공격력 -5
/// </summary>
public class EnemyStatFlatEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        string modId = RelicEffectHelper.GenerateModifierId("enemy_flat", character.Id);

        var modifier = new StatModifier(modId, statType, StatModifierType.FlatBaseAdd, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

// ────────── 상태이상 계열 ──────────

/// <summary>
/// 상태이상 부여
/// Params: { "StatuseffectId": 1 }
/// </summary>
public class ApplyStatusEffectEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        int statusId = RelicEffectHelper.ResolveStatuseffectId(resolvedParams);
        if (statusId <= 0) return;

        int duration = RelicEffectHelper.GetInt(resolvedParams, "Duration", -1);

        // source: signal.Subject 또는 target 자신
        BaseCharacter source = signal?.Subject as BaseCharacter ?? character;

        var instance = StatManager.Instance.CreateStatuseffectInstance(statusId, source, character, duration);
        if (instance != null)
        {
            character.StatuseffectController.AddStatusEffectInstance(instance);
            instance.InvokeInstanceEvent(EffectInstanceEvent.Start);
        }

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 공격자에게 상태이상 부여 (피격 시 반격용)
/// Params: { "StatusName": "Frozen", "Duration": 1 }
/// </summary>
public class ApplyStatusToAttackerEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        BaseCharacter attacker = signal?.Attacker;
        if (attacker == null || attacker.isDead) return;

        int statusId = RelicEffectHelper.ResolveStatuseffectId(resolvedParams);
        if (statusId <= 0) return;

        int duration = RelicEffectHelper.GetInt(resolvedParams, "Duration", -1);
        BaseCharacter self = target as BaseCharacter ?? attacker;

        var instance = StatManager.Instance.CreateStatuseffectInstance(statusId, self, attacker, duration);
        if (instance != null)
        {
            attacker.StatuseffectController.AddStatusEffectInstance(instance);
            instance.InvokeInstanceEvent(EffectInstanceEvent.Start);
        }

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 아군에게 특정 상태이상이 걸리면 다른 상태이상을 부여
/// Params: { "StatusName": "Accelerate", "Duration": 1 }
/// </summary>
public class ApplyStatusOnAllyStatusEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        int statusId = RelicEffectHelper.ResolveStatuseffectId(resolvedParams);
        if (statusId <= 0) return;

        int duration = RelicEffectHelper.GetInt(resolvedParams, "Duration", -1);

        var instance = StatManager.Instance.CreateStatuseffectInstance(statusId, character, character, duration);
        if (instance != null)
        {
            character.StatuseffectController.AddStatusEffectInstance(instance);
            instance.InvokeInstanceEvent(EffectInstanceEvent.Start);
        }

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

// ────────── 전투 효과 ──────────

/// <summary>
/// 이벤트 발생 시 회복
/// Params: { "HealAmount": 5 } 또는 { "HealPercent": 0.1 }
/// </summary>
public class HealOnEventEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (character.isDead) return;

        int healAmount = RelicEffectHelper.GetInt(resolvedParams, "HealAmount");

        // 비율 회복
        float healPercent = RelicEffectHelper.GetFloat(resolvedParams, "HealPercent");
        if (healPercent > 0)
            healAmount += Mathf.RoundToInt(character.MaxHp * healPercent);

        if (healAmount <= 0) return;

        character.characterStat.Hp = Mathf.Min(character.characterStat.Hp + healAmount, character.MaxHp);

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 피해 감소 (회피율 증가로 구현하거나, MaxHp 기반 방어막)
/// Params: { "Value": 0.1 } → 피해 10% 감소 (Avd 증가)
/// 또는 { "FlatReduction": 3 } → 고정 3 피해 감소 (스탯 modifier)
/// </summary>
public class DamageReductionEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        string modId = RelicEffectHelper.GenerateModifierId("dmgred", character.Id);

        // Avd(회피율) 증가로 피해 감소 효과 구현
        var modifier = new StatModifier(modId, StatType.Avd, StatModifierType.FlatBaseAdd, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 피해 반사 (피격 시 공격자에게 피해의 일정 비율 반환)
/// Params: { "ReflectPercent": 0.3 } → 30% 반사
/// </summary>
public class DamageReflectEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        BaseCharacter attacker = signal?.Attacker;
        if (attacker == null || attacker.isDead) return;

        float reflectPercent = RelicEffectHelper.GetFloat(resolvedParams, "ReflectPercent", 0.3f);
        float damage = signal?.DamageAmount ?? 0f;

        int reflectDamage = Mathf.RoundToInt(damage * reflectPercent);
        if (reflectDamage <= 0) return;

        attacker.characterStat.Hp = Mathf.Max(attacker.characterStat.Hp - reflectDamage, 0);
        Debug.Log($"[DamageReflect] {character.CharacterName} reflected {reflectDamage} damage to {attacker.CharacterName}");

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 사망 시 아군 회복
/// Params: { "HealAmount": 10 } 또는 { "HealPercent": 0.15 }
/// </summary>
public class OnDeathHealEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (character.isDead) return;

        int healAmount = RelicEffectHelper.GetInt(resolvedParams, "HealAmount");

        float healPercent = RelicEffectHelper.GetFloat(resolvedParams, "HealPercent");
        if (healPercent > 0)
            healAmount += Mathf.RoundToInt(character.MaxHp * healPercent);

        if (healAmount <= 0) return;

        character.characterStat.Hp = Mathf.Min(character.characterStat.Hp + healAmount, character.MaxHp);

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 사망 시 주변 적에게 폭발 피해
/// Params: { "Damage": 15 }
/// </summary>
public class OnDeathExplosionEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (character.isDead) return;

        int damage = RelicEffectHelper.GetInt(resolvedParams, "Damage", 10);

        character.characterStat.Hp = Mathf.Max(character.characterStat.Hp - damage, 0);

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 추가 공격 (공격 행동 횟수 증가)
/// Params: { "Count": 1 }
/// </summary>
public class ExtraAttackEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        int count = RelicEffectHelper.GetInt(resolvedParams, "Count", 1);
        character.StatuseffectController.AdditionalAttackActionCount += count;

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character)
        {
            int count = 1; // 기본 1회 복원
            character.StatuseffectController.AdditionalAttackActionCount -= count;
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 피해 전이 (받은 피해의 일부를 다른 아군에게 분산)
/// Params: { "TransferPercent": 0.3 } → 30% 전이
/// </summary>
public class TransferDamageEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (character.isDead) return;

        float transferPercent = RelicEffectHelper.GetFloat(resolvedParams, "TransferPercent", 0.3f);
        float damage = signal?.DamageAmount ?? 0f;

        int transferAmount = Mathf.RoundToInt(damage * transferPercent);
        if (transferAmount <= 0) return;

        // 회복: 원래 대상의 피해 경감
        character.characterStat.Hp = Mathf.Min(
            character.characterStat.Hp + transferAmount, character.MaxHp);

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

// ────────── 자원/보상 계열 ──────────

/// <summary>
/// AP 증가
/// Params: { "Value": 2 }
/// </summary>
public class ApBoostEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        float value = RelicEffectHelper.GetFloat(resolvedParams, "Value");
        string modId = RelicEffectHelper.GenerateModifierId("ap", character.Id);

        var modifier = new StatModifier(modId, StatType.Ap, StatModifierType.FlatBaseAdd, value);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 보상 배율 증가 (시스템 레벨 효과 — 보상 계산 시 참조)
/// Params: { "Multiplier": 1.5 }
/// </summary>
public class RewardMultiplierEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    public static float ActiveMultiplier { get; private set; } = 1f;

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        float multiplier = RelicEffectHelper.GetFloat(resolvedParams, "Multiplier", 1f);
        ActiveMultiplier *= multiplier;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        // 보상 배율 리셋은 전투 종료 시 처리
        ActiveMultiplier = 1f;
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 추가 보상 획득
/// Params: { "BonusGold": 50, "BonusExp": 10 }
/// </summary>
public class RewardAdditionalEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    public static int BonusGold { get; private set; }
    public static int BonusExp { get; private set; }

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        BonusGold += RelicEffectHelper.GetInt(resolvedParams, "BonusGold");
        BonusExp += RelicEffectHelper.GetInt(resolvedParams, "BonusExp");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        BonusGold = 0;
        BonusExp = 0;
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 자원 획득 (즉시)
/// Params: { "Gold": 100 }
/// </summary>
public class ResourceGainEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        int gold = RelicEffectHelper.GetInt(resolvedParams, "Gold");
        if (gold > 0)
            Debug.Log($"[ResourceGain] +{gold} Gold (보상 시스템 연동 필요)");

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 상점 할인
/// Params: { "DiscountPercent": 0.1 } → 10% 할인
/// </summary>
public class ShopDiscountEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    public static float ActiveDiscount { get; private set; }

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        ActiveDiscount += RelicEffectHelper.GetFloat(resolvedParams, "DiscountPercent");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        ActiveDiscount = 0f;
        AffectedTargets.Remove(target);
    }
}

// ────────── 기타 효과 ──────────

/// <summary>
/// 스킬 쿨다운 감소
/// Params: { "Reduction": 1 }
/// </summary>
public class CooldownReductionEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        // 쿨다운 시스템 연동 시 구현
        Debug.Log($"[CooldownReduction] Applied (스킬 시스템 연동 필요)");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 버프 확산 (자신의 버프를 아군에게 전파)
/// Params: { "Tag": "Buff" }
/// </summary>
public class BuffSpreadEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        // 버프 확산 로직은 TargetFilter로 대상을 선정하고
        // 해당 대상에게 동일 상태이상을 복제하는 방식
        Debug.Log($"[BuffSpread] Applied (상태이상 복제 로직 필요)");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 버프 획득 시 디버프 해제
/// Params: { "ClearTag": "DeBuff" }
/// </summary>
public class CleanseOnBuffEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;

        string clearTagStr = RelicEffectHelper.GetString(resolvedParams, "ClearTag", "DeBuff");
        if (Enum.TryParse<StatuseffectTag>(clearTagStr, true, out var clearTag))
        {
            character.StatuseffectController.ClearStatuseffectByTag(clearTag);
        }

        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 독 타일 생성 (타일 시스템 연동)
/// Params: { "Duration": 3, "DamagePerTurn": 2 }
/// </summary>
public class CreatePoisonTileEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        Debug.Log($"[CreatePoisonTile] Applied (타일 시스템 연동 필요)");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 회복 타일 생성 (타일 시스템 연동)
/// Params: { "Duration": 3, "HealPerTurn": 2 }
/// </summary>
public class CreateHealTileEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        Debug.Log($"[CreateHealTile] Applied (타일 시스템 연동 필요)");
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        AffectedTargets.Remove(target);
    }
}

/// <summary>
/// 시너지 유물 획득 시 즉시 효과 (한 번만 실행)
/// Params: { "SynergyTag": "선구자", "BonusStat": "Atk", "BonusValue": 3 }
/// </summary>
public class SynergyOnAcquireEffect : IRelicEffect
{
    public HashSet<object> AffectedTargets { get; } = new();
    private readonly Dictionary<object, string> _modifierIds = new();

    public void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams)
    {
        if (target is not BaseCharacter character) return;
        if (!RelicEffectHelper.TryGetStatType(resolvedParams, out var statType)) return;

        // 시너지 카운트에 비례한 보너스
        string synergyTag = RelicEffectHelper.GetString(resolvedParams, "SynergyTag");
        float bonusPerCount = RelicEffectHelper.GetFloat(resolvedParams, "BonusValue");

        int count = 1;
        if (!string.IsNullOrEmpty(synergyTag))
            count = Mathf.RoundToInt(GlobalDataProvider.Resolve($"SynergyCount_{synergyTag}"));

        float totalValue = bonusPerCount * count;
        string modId = RelicEffectHelper.GenerateModifierId("synergy", character.Id);

        var modifier = new StatModifier(modId, statType, StatModifierType.FlatBaseAdd, totalValue);
        StatManager.Instance.AddModifierToCharacter(character.Id, modifier);

        _modifierIds[target] = modId;
        AffectedTargets.Add(target);
    }

    public void Remove(object target)
    {
        if (target is BaseCharacter character && _modifierIds.TryGetValue(target, out var modId))
        {
            StatManager.Instance.RemoveModifierFromCharacter(character.Id, modId);
            _modifierIds.Remove(target);
        }
        AffectedTargets.Remove(target);
    }
}
