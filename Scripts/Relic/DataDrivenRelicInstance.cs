using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using EventType = HM.EventType;

/// <summary>
/// 93개 유물을 단일 클래스로 처리하는 데이터 주도 유물 인스턴스
/// - 복수 효과 순차 실행 (RelicEffectEntry 리스트)
/// - 효과별 조건 평가 (IRelicEffectCondition)
/// - 전역 데이터 바인딩 (GlobalParamBinding → GlobalDataProvider)
/// - 가역 효과 추적 (IsReversible)
/// - EventType → TriggerType 역매핑 (복수 트리거 지원)
/// - 런타임 상태 관리 (RelicRuntimeState)
/// </summary>
public class DataDrivenRelicInstance : RelicInstance
{
    // 인스턴스 런타임 상태 (쿨다운, 카운터 등)
    private RelicRuntimeState _state;

    // 적용 중인 가역 효과 추적: (effectIndex, target) → effect
    private readonly Dictionary<(int effectIndex, BaseCharacter target), IRelicEffect> _activeReversibleEffects = new();

    // 효과별 조건 인스턴스 캐시: effectIndex → condition
    private readonly Dictionary<int, IRelicEffectCondition> _effectConditions = new();

    public override void Register(RelicData data)
    {
        _data = data;
        _state = new RelicRuntimeState();
        _boundEventTypes = new List<EventType>();

        // 계층 1 조건 생성 (트리거 레벨)
        _conditions = new List<IRelicCondition>();
        foreach (var triggerType in data.TriggerTypes)
        {
            var condition = RelicFactory.CreateCondition(triggerType);
            _conditions.Add(condition);
        }

        // 계층 2 조건 사전 생성 (효과 레벨)
        if (data.Effects != null)
        {
            for (int i = 0; i < data.Effects.Count; i++)
            {
                var entry = data.Effects[i];
                if (entry.Condition != null)
                {
                    var effectCondition = RelicEffectConditionFactory.Create(entry.Condition);
                    if (effectCondition != null)
                        _effectConditions[i] = effectCondition;
                }
            }
        }

        // 이벤트 구독
        foreach (var triggerType in data.TriggerTypes)
        {
            var eventType = RelicTriggerMapper.GetSystemEventType(triggerType);
            if (eventType != EventType.None && !_boundEventTypes.Contains(eventType))
            {
                _boundEventTypes.Add(eventType);
                HM.EventManager.Instance.AddEvent(eventType, this);
            }
        }

        // Always 유물: 전투 상태 변화 이벤트 자동 구독 + 즉시 실행
        if (data.TriggerTypes.Contains(RelicTriggerType.Always))
        {
            SubscribeAlwaysEvents();

            Execute(new RelicSignal
            {
                TriggerType = RelicTriggerType.Always,
                RuntimeParams = new Dictionary<string, float>(),
                Timestamp = 0f
            });
        }
    }

    /// <summary>
    /// Always 유물은 전투 상태 변화에 반응하여 재평가
    /// </summary>
    private void SubscribeAlwaysEvents()
    {
        if (!_boundEventTypes.Contains(EventType.OnCharacterDead))
        {
            _boundEventTypes.Add(EventType.OnCharacterDead);
            HM.EventManager.Instance.AddEvent(EventType.OnCharacterDead, this);
        }

        if (!_boundEventTypes.Contains(EventType.OnBattleStart))
        {
            _boundEventTypes.Add(EventType.OnBattleStart);
            HM.EventManager.Instance.AddEvent(EventType.OnBattleStart, this);
        }
    }

    // === 이벤트 수신: EventType → TriggerType 역매핑 ===

    public override void OnEvent(EventType eventType, Component sender, object param = null)
    {
        // Always 유물의 전투 상태 변화 처리
        if (_data.TriggerTypes.Contains(RelicTriggerType.Always))
        {
            if (eventType == EventType.OnCharacterDead)
            {
                HandleAlwaysOnCharacterDead(param);
                return;
            }
            if (eventType == EventType.OnBattleStart)
            {
                HandleAlwaysOnBattleStart();
                return;
            }
        }

        // 역매핑: 수신한 EventType에 대응하는 TriggerType 탐색
        foreach (var triggerType in _data.TriggerTypes)
        {
            if (triggerType == RelicTriggerType.Always) continue;

            if (RelicTriggerMapper.GetSystemEventType(triggerType) == eventType)
            {
                var signal = RelicSignalParser.Parse(triggerType, param);
                if (signal != null)
                    Execute(signal);
            }
        }
    }

    // === 핵심 실행 로직 ===

    public override void Execute(RelicSignal signal)
    {
        // 1. 트리거 조건 확인 (계층 1)
        if (!CheckAllConditions(signal)) return;

        // 2. 타겟 리스트 해석
        var targets = RelicTargetResolver.Resolve(
            _data.TargetFilter, _data.TargetFilterParams, signal);

        // 3. Effects가 없으면 하위호환 (Phase 1 LogicType 경로)
        if (_data.Effects == null || _data.Effects.Count == 0)
        {
            ExecuteLegacy(signal, targets);
            return;
        }

        // 4. 각 효과를 순차 실행
        for (int i = 0; i < _data.Effects.Count; i++)
        {
            var effectEntry = _data.Effects[i];

            // 4a. 효과별 조건 확인 (계층 2)
            _effectConditions.TryGetValue(i, out var effectCondition);

            // 4b. 전역 데이터 바인딩 해석
            var resolvedParams = ResolveGlobalBindings(effectEntry);

            // 4c. 타겟별 조건 평가 + 효과 적용/제거
            foreach (var target in targets)
            {
                bool conditionMet = effectCondition == null ||
                    effectCondition.Evaluate(target, signal, effectEntry.Condition?.ConditionParams);

                var key = (i, target);

                if (conditionMet)
                {
                    // 가역 효과: 이미 적용 중이면 skip (중복 방지)
                    if (effectEntry.IsReversible && _activeReversibleEffects.ContainsKey(key))
                        continue;

                    var effect = RelicFactory.CreateEffect(effectEntry.EffectType);
                    effect.Apply(target, signal, resolvedParams);

                    // 가역 효과 추적
                    if (effectEntry.IsReversible)
                        _activeReversibleEffects[key] = effect;

                    // Stateful 조건 업데이트
                    if (effectCondition is IStatefulRelicCondition stateful)
                        stateful.OnConditionPassed();
                }
                else if (effectEntry.IsReversible && _activeReversibleEffects.TryGetValue(key, out var activeEffect))
                {
                    // 조건 미충족 → 가역 효과 제거
                    activeEffect.Remove(target);
                    _activeReversibleEffects.Remove(key);
                }
            }
        }
    }

    // === 전역 데이터 바인딩 해석 ===

    private Dictionary<string, JToken> ResolveGlobalBindings(RelicEffectEntry entry)
    {
        var resolved = new Dictionary<string, JToken>(entry.Params);

        if (entry.GlobalBindings == null) return resolved;

        foreach (var binding in entry.GlobalBindings)
        {
            float baseValue = GlobalDataProvider.Resolve(binding.GlobalSource);

            if (!string.IsNullOrEmpty(binding.Expression))
                baseValue = RelicExpressionEvaluator.Evaluate(baseValue, binding.Expression);

            resolved[binding.ParamKey] = JToken.FromObject(baseValue);
        }

        return resolved;
    }

    // === Always 유물 전투 상태 변화 처리 ===

    private void HandleAlwaysOnCharacterDead(object param)
    {
        BaseCharacter deadCharacter = null;
        if (param is DamageEventData dmgData)
            deadCharacter = dmgData.Target;
        else if (param is BaseCharacter bc)
            deadCharacter = bc;

        if (deadCharacter == null) return;

        // 사망 캐릭터에 적용된 가역 효과 제거
        var keysToRemove = new List<(int, BaseCharacter)>();
        foreach (var kvp in _activeReversibleEffects)
        {
            if (kvp.Key.target == deadCharacter)
            {
                kvp.Value.Remove(deadCharacter);
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (var key in keysToRemove)
            _activeReversibleEffects.Remove(key);
    }

    private void HandleAlwaysOnBattleStart()
    {
        // 기존 가역 효과 전부 제거
        foreach (var kvp in _activeReversibleEffects)
            kvp.Value.Remove(kvp.Key.target);
        _activeReversibleEffects.Clear();

        // Stateful 조건 리셋
        foreach (var cond in _effectConditions.Values)
        {
            if (cond is IStatefulRelicCondition stateful)
                stateful.Reset(RelicResetScope.Battle);
        }
        _state.ResetForScope(RelicResetScope.Battle);

        // 전체 재적용
        Execute(new RelicSignal
        {
            TriggerType = RelicTriggerType.Always,
            RuntimeParams = new Dictionary<string, float>(),
            Timestamp = Time.time
        });
    }

    // === Phase 1 하위호환 ===

    private void ExecuteLegacy(RelicSignal signal, List<BaseCharacter> targets)
    {
        if (_effect == null) return;

        var legacyParams = ConvertLogicParams();
        foreach (var target in targets)
        {
            if (!_effect.AffectedTargets.Contains(target))
                _effect.Apply(target, signal, legacyParams);
        }
    }

    // === 해제 ===

    public override void Unregister()
    {
        // 모든 가역 효과 일괄 제거
        foreach (var kvp in _activeReversibleEffects)
            kvp.Value.Remove(kvp.Key.target);
        _activeReversibleEffects.Clear();

        base.Unregister();
    }

    // === Save/Load ===

    public RelicSaveData ToSaveData()
    {
        // Stateful 조건 상태 저장
        foreach (var kvp in _effectConditions)
        {
            if (kvp.Value is IStatefulRelicCondition stateful)
                _state.ConditionStates[$"effect_{kvp.Key}"] = stateful.GetState();
        }

        return new RelicSaveData
        {
            RelicId = _data.Id,
            RuntimeState = _state
        };
    }

    public void LoadState(RelicRuntimeState state)
    {
        _state = state;

        // Stateful 조건 상태 복원
        foreach (var kvp in _effectConditions)
        {
            string stateKey = $"effect_{kvp.Key}";
            if (kvp.Value is IStatefulRelicCondition stateful
                && _state.ConditionStates.TryGetValue(stateKey, out var condState))
            {
                stateful.SetState(condState);
            }
        }
    }
}
