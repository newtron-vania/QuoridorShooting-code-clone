using System;
using System.Collections.Generic;
using System.Linq;
using HM;
using Newtonsoft.Json.Linq;
using UnityEngine;
using EventType = HM.EventType;

/// <summary>
/// 유물 발동 트리거 타입 → EventType 매핑
/// </summary>
public static class RelicTriggerMapper
{
    private static readonly Dictionary<RelicTriggerType, EventType> _map = new()
    {
        // 기존 매핑
        { RelicTriggerType.OnTurnStart,       EventType.OnTurnStart },
        { RelicTriggerType.OnTurnEnd,         EventType.OnTurnEnd },
        { RelicTriggerType.OnDamaged,         EventType.OnCharacterDamaged },
        { RelicTriggerType.OnHpChanged,       EventType.OnPlayerHpChanged },
        { RelicTriggerType.OnCharacterDeath,  EventType.OnCharacterDead },
        { RelicTriggerType.OnBattleWin,       EventType.OnGameFinish },
        { RelicTriggerType.OnBattleStart,     EventType.OnBattleStart },

        // 추가 매핑
        { RelicTriggerType.OnFirstTurn,       EventType.OnTurnStart },
        { RelicTriggerType.OnMove,            EventType.OnCellEnter },
        { RelicTriggerType.OnTileEnter,       EventType.OnCellEnter },
        { RelicTriggerType.OnTileExit,        EventType.OnCellExit },
        { RelicTriggerType.OnStageComplete,   EventType.OnGameFinish },

        // 전투 세부 이벤트
        { RelicTriggerType.OnHitSuccess,      EventType.OnHitSuccess },
        { RelicTriggerType.OnHitFail,         EventType.OnHitFail },
        { RelicTriggerType.OnDefenseSuccess,  EventType.OnDefenseSuccess },
        { RelicTriggerType.OnDefenseFail,     EventType.OnDefenseFail },
        { RelicTriggerType.OnSkillUsed,       EventType.OnSkillUsed },

        // 상태이상 이벤트
        { RelicTriggerType.OnStatusApplied,   EventType.OnStatusApplied },
        { RelicTriggerType.OnStatusRemoved,   EventType.OnStatusRemoved },
    };

    // 역매핑 캐시: EventType → 해당하는 모든 RelicTriggerType
    private static readonly Dictionary<EventType, List<RelicTriggerType>> _reverseMap;

    static RelicTriggerMapper()
    {
        _reverseMap = new Dictionary<EventType, List<RelicTriggerType>>();
        foreach (var kvp in _map)
        {
            if (!_reverseMap.ContainsKey(kvp.Value))
                _reverseMap[kvp.Value] = new List<RelicTriggerType>();
            _reverseMap[kvp.Value].Add(kvp.Key);
        }
    }

    public static EventType GetSystemEventType(RelicTriggerType triggerType)
    {
        return _map.TryGetValue(triggerType, out var eventType) ? eventType : EventType.None;
    }

    /// <summary>
    /// EventType에 대응하는 모든 RelicTriggerType 반환 (역매핑)
    /// </summary>
    public static List<RelicTriggerType> GetTriggerTypes(EventType eventType)
    {
        return _reverseMap.TryGetValue(eventType, out var triggers)
            ? triggers : new List<RelicTriggerType>();
    }
}

/// <summary>
/// 피해 이벤트에 동반되는 일시적 데이터
/// 전투 시스템에서 생성하여 EventManager로 전달
/// </summary>
public struct DamageEventData
{
    public BaseCharacter Attacker;
    public BaseCharacter Target;
    public float RawDamage;
    public float FinalDamage;
    public bool IsCritical;
    public int SkillId;
}

/// <summary>
/// 시스템 이벤트 발생 시 유물에게 전달되는 데이터 패킷
/// </summary>
public class RelicSignal
{
    public RelicTriggerType TriggerType;

    // 이벤트 주체 (피격자, 사망자, 행동자)
    public object Subject;

    // === 일시적 이벤트 데이터 ===

    // 공격자 참조 (OnDamaged, OnDealDamage 시)
    public BaseCharacter Attacker;

    // 실제 피해량 (OnDamaged 시)
    public float DamageAmount;

    // 사용된 스킬 ID (OnSkillUsed 시)
    public int SkillId;

    // 추가 컨텍스트 데이터 (DamageEventData 등)
    public object ContextData;

    // 실시간 파라미터 (수치 데이터)
    public Dictionary<string, float> RuntimeParams;

    // 이벤트 발생 시간
    public float Timestamp;
}


public interface IRelic
{
    int Id { get; }
    void Register(RelicData data);
    void Execute(RelicSignal signal);
    void Unregister();
}

/// <summary>
/// 트리거 레벨 조건 판단 (계층 1)
/// </summary>
public interface IRelicCondition
{
    bool IsSatisfied(RelicSignal signal, Dictionary<string, JToken> conditionParams);
}

/// <summary>
/// 효과 레벨 조건 판단 (계층 2)
/// 효과 실행 여부를 결정하는 세부 조건
/// </summary>
public interface IRelicEffectCondition
{
    bool Evaluate(BaseCharacter target, RelicSignal signal, Dictionary<string, JToken> condParams);
}

/// <summary>
/// 상태를 보존하는 조건 — 발동 횟수, 쿨다운 등 추적
/// </summary>
public interface IStatefulRelicCondition : IRelicEffectCondition
{
    void Reset(RelicResetScope scope);
    void OnConditionPassed();
    Dictionary<string, object> GetState();
    void SetState(Dictionary<string, object> state);
}

/// <summary>
/// 유물 효과 적용 전략 인터페이스
/// </summary>
public interface IRelicEffect
{
    void Apply(object target, RelicSignal signal, Dictionary<string, JToken> resolvedParams);
    void Remove(object target);
    HashSet<object> AffectedTargets { get; }
}

/// <summary>
/// 유물의 추상 베이스 클래스
/// </summary>
public abstract class RelicInstance : IRelic, IEventListener
{
    public int Id => _data.Id;
    protected RelicData _data;
    protected List<IRelicCondition> _conditions;
    protected IRelicEffect _effect;
    protected List<EventType> _boundEventTypes;

    public virtual void Register(RelicData data)
    {
        _data = data;
        _boundEventTypes = new List<EventType>();

        // Condition 리스트 생성
        _conditions = new List<IRelicCondition>();
        foreach (var triggerType in data.TriggerTypes)
        {
            var condition = RelicFactory.CreateCondition(triggerType);
            _conditions.Add(condition);
        }

        // Effect 생성 (하위호환: LogicType이 있는 경우)
        if (data.LogicType.HasValue)
            _effect = RelicFactory.CreateEffect(data.LogicType.Value);

        // 각 TriggerType에 맞는 시스템 이벤트 구독
        foreach (var triggerType in data.TriggerTypes)
        {
            var eventType = RelicTriggerMapper.GetSystemEventType(triggerType);
            if (eventType != EventType.None && !_boundEventTypes.Contains(eventType))
            {
                _boundEventTypes.Add(eventType);
                EventManager.Instance.AddEvent(eventType, this);
            }

            // Always 타입은 등록 시점에 즉시 실행
            if (triggerType == RelicTriggerType.Always)
            {
                Execute(new RelicSignal
                {
                    TriggerType = RelicTriggerType.Always,
                    RuntimeParams = new Dictionary<string, float>(),
                    Timestamp = 0f
                });
            }
        }
    }

    // IEventListener 구현 — EventType → TriggerType 역매핑
    public virtual void OnEvent(EventType eventType, Component sender, object param = null)
    {
        // 수신한 EventType에 대응하는 TriggerType 결정
        foreach (var triggerType in _data.TriggerTypes)
        {
            if (RelicTriggerMapper.GetSystemEventType(triggerType) == eventType)
            {
                var signal = RelicSignalParser.Parse(triggerType, param);
                if (signal != null)
                    Execute(signal);
            }
        }
    }

    public abstract void Execute(RelicSignal signal);

    public virtual void Unregister()
    {
        // 효과 제거
        if (_effect != null)
        {
            foreach (var target in _effect.AffectedTargets.ToArray())
                _effect.Remove(target);
        }

        // EventManager 구독 해제
        foreach (var eventType in _boundEventTypes)
            EventManager.Instance.RemoveEvent(eventType, this);
    }

    // 모든 트리거 조건이 충족되는지 체크 (AND 조건)
    protected bool CheckAllConditions(RelicSignal signal)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.IsSatisfied(signal, _data.TriggerParams))
                return false;
        }
        return true;
    }

    // Phase 1 하위호환: LogicParams (float) → JToken 변환
    protected Dictionary<string, JToken> ConvertLogicParams()
    {
        var result = new Dictionary<string, JToken>();
        if (_data.LogicParams != null)
        {
            foreach (var kvp in _data.LogicParams)
                result[kvp.Key] = JToken.FromObject(kvp.Value);
        }
        return result;
    }
}
