using System;
using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using Newtonsoft.Json.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════
//  Stateless 효과 조건 구현체
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// 대상의 체력 비율이 임계값 이하인지 확인
/// ConditionParams: { "Threshold": 0.3 }  → 체력 30% 이하
/// </summary>
public class HpBelowEffectCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null || target.MaxHp <= 0) return false;

        float threshold = condParams != null && condParams.TryGetValue("Threshold", out var t)
            ? t.Value<float>() : 0.3f;

        return (float)target.Hp / target.MaxHp <= threshold;
    }
}

/// <summary>
/// 대상의 체력이 최대인지 확인
/// ConditionParams: 없음
/// </summary>
public class HpFullEffectCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null) return false;
        return target.Hp >= target.MaxHp;
    }
}

/// <summary>
/// 대상에게 특정 태그의 상태이상이 존재하는지 확인
/// ConditionParams: { "Tag": "Buff" }  → StatuseffectTag.Buff
/// </summary>
public class HasStatusTagEffectCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null) return false;
        if (condParams == null || !condParams.TryGetValue("Tag", out var tagToken)) return false;

        if (!Enum.TryParse<StatuseffectTag>(tagToken.Value<string>(), true, out var requiredTag))
            return false;

        var controller = target.StatuseffectController;
        return controller.OwnedStatuseffectDict.Values
            .Any(instance => instance.TagSet.Contains(requiredTag));
    }

    /// <summary>
    /// 해당 태그를 가진 상태이상 개수 반환 (GlobalBinding 등에서 활용)
    /// </summary>
    public static int CountStatusWithTag(BaseCharacter target, StatuseffectTag tag)
    {
        if (target == null) return 0;
        return target.StatuseffectController.OwnedStatuseffectDict.Values
            .Count(instance => instance.TagSet.Contains(tag));
    }
}

/// <summary>
/// 대상이 이전 턴에 행동하지 않았는지 확인
/// ConditionParams: 없음
/// 판단 기준: 현재 턴에 남은 행동 횟수가 최대값과 동일하면 비활동으로 판단
/// </summary>
public class InactiveLastTurnCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null) return false;

        // 이전 턴에 행동 여부: CanAttack과 CanMove가 모두 남아있으면 행동하지 않은 것으로 간주
        // 실제 게임 로직에서 행동 추적이 추가되면 개선 가능
        return target.CanAttack && target.CanMove;
    }
}

/// <summary>
/// 현재 턴이 지정 간격의 배수인지 확인
/// ConditionParams: { "Interval": 5 }  → 5턴마다
/// </summary>
public class TurnIntervalEffectCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (condParams == null || !condParams.TryGetValue("Interval", out var intervalToken))
            return false;

        int interval = intervalToken.Value<int>();
        if (interval <= 0) return false;

        int currentTurn = GameManager.Instance.Turn;
        return currentTurn > 0 && currentTurn % interval == 0;
    }
}

/// <summary>
/// 대상의 CharacterType이 지정 타입과 일치하는지 확인
/// ConditionParams: { "Type": "Awakener" }
/// </summary>
public class CharacterTypeCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null || condParams == null) return false;
        if (!condParams.TryGetValue("Type", out var typeToken)) return false;

        if (!Enum.TryParse<CharacterType>(typeToken.Value<string>(), true, out var requiredType))
            return false;

        return target.Type == requiredType;
    }
}

/// <summary>
/// 대상의 CharacterClass가 지정 클래스와 일치하는지 확인
/// ConditionParams: { "Class": "Tanker" }
/// </summary>
public class CharacterClassCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null || condParams == null) return false;
        if (!condParams.TryGetValue("Class", out var classToken)) return false;

        if (!Enum.TryParse<CharacterClass>(classToken.Value<string>(), true, out var requiredClass))
            return false;

        return target.Class == requiredClass;
    }
}

/// <summary>
/// 대상의 CharacterType이 지정 타입이 아닌지 확인
/// ConditionParams: { "Type": "Pathfinder" }
/// </summary>
public class ExcludeTypeCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null || condParams == null) return false;
        if (!condParams.TryGetValue("Type", out var typeToken)) return false;

        if (!Enum.TryParse<CharacterType>(typeToken.Value<string>(), true, out var excludedType))
            return true; // 파싱 실패 시 제외 대상 아님 → 통과

        return target.Type != excludedType;
    }
}

/// <summary>
/// 현재 스테이지가 보스 스테이지인지 확인
/// ConditionParams: 없음
/// </summary>
public class BossTargetCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (StageManager.Instance == null || StageManager.Instance.StageDic == null)
            return false;

        int curId = StageManager.Instance.CurStageId;
        if (StageManager.Instance.StageDic.TryGetValue(curId, out var stage))
            return stage.Type == Stage.StageType.Boss;

        return false;
    }
}

/// <summary>
/// 대상에게 특정 상태이상이 현재 적용 중인지 확인
/// ConditionParams: { "StatusType": "Poisoned" }  → StatuseffectInstance.Type.Poisoned
/// </summary>
public class StatusActiveCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (target == null || condParams == null) return false;
        if (!condParams.TryGetValue("StatusType", out var statusToken)) return false;

        if (!Enum.TryParse<StatuseffectInstance.Type>(statusToken.Value<string>(), true, out var statusType))
            return false;

        return target.StatuseffectController.CheckUnderStatuseffect(statusType);
    }
}

/// <summary>
/// 시그널의 피해량이 임계값 이하인지 확인
/// ConditionParams: { "Threshold": 4 }  → 4 이하 피해
/// </summary>
public class DamageBelowCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (signal == null || condParams == null) return false;
        if (!condParams.TryGetValue("Threshold", out var thresholdToken)) return false;

        float threshold = thresholdToken.Value<float>();

        // 시그널의 DamageAmount 우선, 없으면 RuntimeParams 확인
        float damage = signal.DamageAmount;
        if (damage <= 0 && signal.RuntimeParams != null)
            signal.RuntimeParams.TryGetValue("DamageAmount", out damage);

        return damage > 0 && damage <= threshold;
    }
}

/// <summary>
/// 보유 유물 개수가 N개 이상인지 확인
/// ConditionParams: { "MinCount": 5 }
/// </summary>
public class OwnRelicCountCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (condParams == null || !condParams.TryGetValue("MinCount", out var countToken))
            return false;

        int minCount = countToken.Value<int>();
        int ownedCount = RelicManager.Instance.GetActiveRelicIds().Count;
        return ownedCount >= minCount;
    }
}

/// <summary>
/// 확률 기반 조건 — n% 확률로 통과
/// ConditionParams: { "Chance": 0.25 }  → 25% 확률
/// </summary>
public class ChanceCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        float chance = condParams != null && condParams.TryGetValue("Chance", out var c)
            ? c.Value<float>() : 0.5f;
        return UnityEngine.Random.value <= chance;
    }
}

/// <summary>
/// 전장 내 특정 클래스 아군이 N명 이상인지 확인
/// ConditionParams: { "Class": "Tanker", "MinCount": 1 }
/// </summary>
public class AllyClassMinCountCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (condParams == null) return false;
        if (!condParams.TryGetValue("Class", out var classToken)) return false;
        if (!Enum.TryParse<CharacterClass>(classToken.Value<string>(), true, out var cls))
            return false;

        int minCount = condParams.TryGetValue("MinCount", out var mc) ? mc.Value<int>() : 1;
        return (int)GlobalDataProvider.Resolve($"AllyCount_{cls}") >= minCount;
    }
}

/// <summary>
/// 전장 내 특정 타입 아군이 N명 이상인지 확인
/// ConditionParams: { "Type": "Awakener", "MinCount": 2 }
/// </summary>
public class AllyTypeMinCountCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        if (condParams == null) return false;
        if (!condParams.TryGetValue("Type", out var typeToken)) return false;
        if (!Enum.TryParse<CharacterType>(typeToken.Value<string>(), true, out var type))
            return false;

        int minCount = condParams.TryGetValue("MinCount", out var mc) ? mc.Value<int>() : 1;
        return (int)GlobalDataProvider.Resolve($"AllyCount_{type}") >= minCount;
    }
}

/// <summary>
/// 현재 턴이 게임 첫 턴인지 확인 (OnTurnStart 기반 유물용)
/// ConditionParams: 없음
/// </summary>
public class FirstTurnCondition : IRelicEffectCondition
{
    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        return GameManager.Instance.Turn <= 1;
    }
}

// ═══════════════════════════════════════════════════════════════
//  Stateful 효과 조건 구현체
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// 스테이지당 N회 발동 제한
/// ConditionParams: { "MaxCount": 1 }  → 스테이지당 1회
/// </summary>
public class FirstPerStageCondition : IStatefulRelicCondition
{
    private int _triggerCount;

    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        int maxTriggers = condParams != null && condParams.TryGetValue("MaxCount", out var mc)
            ? mc.Value<int>() : 1;
        return _triggerCount < maxTriggers;
    }

    public void OnConditionPassed() => _triggerCount++;

    public void Reset(RelicResetScope scope)
    {
        if (scope == RelicResetScope.Stage || scope == RelicResetScope.Battle)
            _triggerCount = 0;
    }

    public Dictionary<string, object> GetState() =>
        new() { { "triggerCount", _triggerCount } };

    public void SetState(Dictionary<string, object> state)
    {
        if (state.TryGetValue("triggerCount", out var val))
            _triggerCount = Convert.ToInt32(val);
    }
}

/// <summary>
/// 전투당 N회 발동 제한
/// ConditionParams: { "MaxCount": 1 }  → 전투당 1회
/// </summary>
public class FirstPerBattleCondition : IStatefulRelicCondition
{
    private int _triggerCount;

    public bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams)
    {
        int maxTriggers = condParams != null && condParams.TryGetValue("MaxCount", out var mc)
            ? mc.Value<int>() : 1;
        return _triggerCount < maxTriggers;
    }

    public void OnConditionPassed() => _triggerCount++;

    public void Reset(RelicResetScope scope)
    {
        if (scope == RelicResetScope.Battle)
            _triggerCount = 0;
    }

    public Dictionary<string, object> GetState() =>
        new() { { "triggerCount", _triggerCount } };

    public void SetState(Dictionary<string, object> state)
    {
        if (state.TryGetValue("triggerCount", out var val))
            _triggerCount = Convert.ToInt32(val);
    }
}

// ═══════════════════════════════════════════════════════════════
//  팩토리
// ═══════════════════════════════════════════════════════════════

/// <summary>
/// ConditionType 문자열 → IRelicEffectCondition 인스턴스 생성
/// </summary>
public static class RelicEffectConditionFactory
{
    private static readonly Dictionary<string, Func<IRelicEffectCondition>> _creators = new(StringComparer.OrdinalIgnoreCase)
    {
        { "HpBelow",           () => new HpBelowEffectCondition() },
        { "HpFull",            () => new HpFullEffectCondition() },
        { "HasStatusTag",      () => new HasStatusTagEffectCondition() },
        { "DidNotActLastTurn", () => new InactiveLastTurnCondition() },
        { "TurnInterval",      () => new TurnIntervalEffectCondition() },
        { "CharType",          () => new CharacterTypeCondition() },
        { "CharClass",         () => new CharacterClassCondition() },
        { "ExcludeType",       () => new ExcludeTypeCondition() },
        { "BossTarget",        () => new BossTargetCondition() },
        { "StatusActive",      () => new StatusActiveCondition() },
        { "DamageBelow",       () => new DamageBelowCondition() },
        { "FirstPerStage",     () => new FirstPerStageCondition() },
        { "FirstPerBattle",    () => new FirstPerBattleCondition() },
        { "OwnRelicCount",     () => new OwnRelicCountCondition() },
        { "Chance",            () => new ChanceCondition() },
        { "AllyClassMinCount", () => new AllyClassMinCountCondition() },
        { "AllyTypeMinCount",  () => new AllyTypeMinCountCondition() },
        { "FirstTurn",         () => new FirstTurnCondition() },
    };

    /// <summary>
    /// RelicEffectCondition 데이터로부터 IRelicEffectCondition 인스턴스 생성
    /// </summary>
    public static IRelicEffectCondition Create(RelicEffectCondition conditionData)
    {
        if (conditionData == null || string.IsNullOrEmpty(conditionData.ConditionType))
            return null;

        if (_creators.TryGetValue(conditionData.ConditionType, out var creator))
            return creator();

        Debug.LogWarning($"[WARN] RelicEffectConditionFactory::Create - Unknown ConditionType: {conditionData.ConditionType}");
        return null;
    }
}
