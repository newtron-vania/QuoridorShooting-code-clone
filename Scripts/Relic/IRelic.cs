using System;
using System.Collections.Generic;
using System.Linq;
using HM;
using UnityEngine;
using EventType = HM.EventType;

/// <summary>
/// 유물 발동 트리거 타입
/// </summary>
public static class RelicTriggerMapper
{
    private static readonly Dictionary<RelicTriggerType, EventType> _map = new()
    {
        { RelicTriggerType.OnTurnStart, EventType.OnTurnStart },
        { RelicTriggerType.OnTurnEnd, EventType.OnTurnEnd },
        { RelicTriggerType.OnDamaged, EventType.OnCharacterDamaged },
        { RelicTriggerType.OnHpChanged, EventType.OnPlayerHpChanged }, // 또는 OnEnemyHpChanged와 연동
        { RelicTriggerType.OnCharacterDeath, EventType.OnCharacterDead },
        { RelicTriggerType.OnBattleWin, EventType.OnGameFinish },
        { RelicTriggerType.OnBattleStart, EventType.OnCharacterTurnStart }
    };

    public static EventType GetSystemEventType(RelicTriggerType triggerType)
    {
        return _map.TryGetValue(triggerType, out var eventType) ? eventType : EventType.None;
    }
}

/// <summary>
/// 시스템 이벤트 발생 시 유물에게 전달되는 데이터 패킷
/// object 기반으로 다양한 Subject/ContextData 지원
/// </summary>
public class RelicSignal
{
    public RelicTriggerType TriggerType;

    // 이벤트 주체 (BaseCharacter, RewardContext, Tile, GameManager 등)
    public object Subject;

    // 추가 컨텍스트 데이터 (DamageEventData, 드롭 아이템 리스트 등)
    public object ContextData;

    // 실시간 파라미터 (수치 데이터)
    public Dictionary<string, float> RuntimeParams;

    // 이벤트 발생 시간
    public float Timestamp;
}


public interface IRelic
{
    int Id { get; }

    // 데이터만으로 스스로를 시스템에 등록
    void Register(RelicData data);

    // EventManager의 콜백 입구
    void Execute(RelicSignal signal);
    void Unregister();
}

// 조건 판단: 이 시그널의 주체가 유물의 효과 대상(Target)이 될 수 있는지 확인
public interface IRelicCondition
{
    bool IsSatisfied(RelicSignal signal, Dictionary<string, float> conditionParams);
}

/// <summary>
/// 유물 효과 적용 전략 인터페이스
/// object 기반으로 다양한 대상 지원 (BaseCharacter, RewardContext, Tile, Manager 등)
/// </summary>
public interface IRelicEffect
{
    // 대상에게 효과 적용 (가변 Param 기반 계산식 포함)
    void Apply(object target, RelicSignal signal, Dictionary<string, float> logicParams);

    // 대상에게서 효과 제거
    void Remove(object target);

    // 현재 효과가 적용된 대상 목록
    HashSet<object> AffectedTargets { get; }
}

/// <summary>
/// 유물의 추상 베이스 클래스
/// 구체적인 유물은 이를 상속받아 Execute를 구현
/// </summary>
public abstract class RelicInstance : IRelic, IEventListener
{
    public int Id => _data.Id;
    protected RelicData _data;
    protected List<IRelicCondition> _conditions;
    protected IRelicEffect _effect;
    protected List<EventType> _boundEventTypes;

    // 데이터로부터 자신을 초기화하고 EventManager에 자동 등록
    public virtual void Register(RelicData data)
    {
        _data = data;
        _boundEventTypes = new List<EventType>();

        // Factory를 통해 Condition 리스트 생성
        _conditions = new List<IRelicCondition>();
        foreach (var triggerType in data.TriggerTypes)
        {
            var condition = RelicFactory.CreateCondition(triggerType);
            _conditions.Add(condition);
        }

        // Factory를 통해 Effect 생성
        _effect = RelicFactory.CreateEffect(data.LogicType);

        // 각 TriggerType에 맞는 시스템 이벤트를 찾아 구독
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

    // IEventListener 구현
    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        // 첫 번째 TriggerType으로 파싱
        var signal = RelicSignalParser.Parse(_data.TriggerTypes[0], param);
        if (signal != null)
        {
            Execute(signal);
        }
    }

    // 시스템 이벤트 수신 시 실행되는 진입점 (하위 클래스에서 구현)
    public abstract void Execute(RelicSignal signal);

    // EventManager로부터 구독 해제 및 정리
    public virtual void Unregister()
    {
        // 모든 대상에게서 효과 제거
        foreach (var target in _effect.AffectedTargets.ToArray())
        {
            _effect.Remove(target);
        }

        // EventManager 구독 해제
        foreach (var eventType in _boundEventTypes)
        {
            EventManager.Instance.RemoveEvent(eventType, this);
        }
    }

    // 모든 조건이 충족되는지 체크 (AND 조건)
    protected bool CheckAllConditions(RelicSignal signal)
    {
        foreach (var condition in _conditions)
        {
            if (!condition.IsSatisfied(signal, _data.TriggerParams))
            {
                return false;
            }
        }
        return true;
    }
}