using System;
using System.Collections.Generic;
using CharacterDefinition;
using UnityEngine;

public class StatuseffectInstance : EffectInstance, IDurableEffect
{
    //Id별 상태 이름
    public enum Type
    {
        Blinded = 1,  // Fixed typo: Blined → Blinded
        Holded,
        Provocation,
        Exhausted,
        Accelerate,
        Clear,
        Toxic,
        Poisoned,
        Ignite,
        Burnt,
        Weaken,
        Glacial,
        Frozen,
        Lifedrain,
        Unresistable,
        PowerUp,
        Resistable,
        TimeDistortion,
        Rotten,

    }

    public IEffectableProvider Target { get; private set; }

    public int Duration { get; set; } = 0;

    public readonly Type StatuseffectType;

    public HashSet<StatuseffectTag> TagSet = new();

    /// <summary>
    /// GroupModifier: 상태이상 효과의 통합 관리 객체
    /// 스탯 변경, 행동 제한, 지속 피해, 콜백 효과를 하나로 관리
    /// </summary>
    public StatuseffectModifierGroup ModifierGroup { get; private set; }

    // IDurableEffect 구현
    public string EffectId => $"Statuseffect_{StatuseffectType}_{GetHashCode()}";
    public BaseCharacter Owner => Target.GetEffectable<BaseCharacter>();
    public bool IsActive => Duration >= 0;
    public int Priority { get; private set; }

    /// <summary>
    /// 변경하면 위험한 데이터
    /// </summary>
    public readonly EffectData InstanceData;

    public StatuseffectInstance(StatManager statManager, StatuseffectData statusEffectData, BaseCharacter source, IEffectableProvider target, EffectData instanceData) : base(source, statManager)
    {
        Target = target;

        //아마 이펙트 데이터 그대로 주입할 예정인듯
        foreach (var effectData in statusEffectData.EffectDataList)
        {
            _effectDataList.Add(effectData);
        }

        TagSet = statusEffectData.Tags;

        //Id별 타입 초기화
        StatuseffectType = (Type)statusEffectData.Id;

        //effectData를 기반으로 지속시간 초기화
        InstanceData = instanceData;

        //지속시간 무제한 태그 검사
        if (TagSet.Contains(StatuseffectTag.Immutable))
            Duration = 999;
        else
            Duration = InstanceData.Get<int>("Duration");

        // Priority 초기화 (JSON에서 지정, 기본값 0)
        Priority = statusEffectData.Priority;

        // GroupModifier 생성 (Factory 패턴)
        string groupId = $"Statuseffect_{StatuseffectType}_{GetHashCode()}";
        ModifierGroup = StatuseffectModifierFactory.CreateModifier(groupId, StatuseffectType, this);

        // DurableEffectRegistry에 등록
        if (Duration >= 0 && DurableEffectRegistry.Instance != null)
        {
            DurableEffectRegistry.Instance.RegisterEffect(this);
        }

    }

    //타겟 찾기 //필수
    public override List<IEffectableProvider> FindEffectTargetList(TargetType effectTargetType)
    {
        //ToDo: 타겟 검사 후 맞으면? 타겟 반환
        List<IEffectableProvider> targetList = new();
        targetList.Add(Target);
        return targetList;
    }

    #region EventHandle
    //Instance 이벤트
    public override void InvokeInstanceEvent(EffectInstanceEvent instanceEvent)
    {
        switch (instanceEvent)
        {
            case EffectInstanceEvent.Start:
                Target.GetEffectable<IStatuseffectParticipant>().StatuseffectController.AddStatusEffectInstance(this);

                // GroupModifier 효과 적용 (스탯 변경, 행동 제한 등)
                ModifierGroup?.ApplyAllEffects(Target, _statManager);
                break;
        }

        base.InvokeInstanceEvent(instanceEvent);
    }

    //게임 이벤트
    public void OnGameEvent(HM.EventType eventType)
    {
        // GroupModifier 턴 시작 콜백 실행 (Poisoned, Burnt 등의 DoT 효과)
        if (eventType == HM.EventType.OnTurnStart)
        {
            var character = Target.GetEffectable<BaseCharacter>();
            if (character != null)
            {
                ModifierGroup?.TriggerOnTurnStart(character);
            }
        }

        foreach (EffectData effectData in _effectDataList)
        {
            List<IEffectableProvider> targetList = FindEffectTargetList(effectData.Target);
            IBaseEffectLogic effectLogic = _statManager.GetEffectLogic(effectData.Type);
            effectLogic.EffectByGameEvent(eventType, this, effectData, targetList);
        }
    }

    /// <summary>
    /// DurableEffectRegistry에서 호출되는 턴 시작 처리
    /// Poisoned, Burnt 등 DoT 효과의 OnTurnStart 콜백 실행
    /// </summary>
    public void ProcessTurnStart()
    {
        OnGameEvent(HM.EventType.OnTurnStart);
    }

    /// <summary>
    /// DurableEffectRegistry에서 호출되는 턴 종료 처리
    /// </summary>
    public void ProcessTurnEnd()
    {
        Duration--;
        OnGameEvent(HM.EventType.OnTurnEnd);

        if (Duration <= 0)
        {
            OnExpire();
        }
    }

    /// <summary>
    /// Duration 만료 시 호출
    /// </summary>
    public void OnExpire()
    {
        // GroupModifier 효과 제거 (스탯 변경, 행동 제한 등 복원)
        ModifierGroup?.RemoveAllEffects(Target, _statManager);

        InvokeInstanceEvent(EffectInstanceEvent.End);

        if (DurableEffectRegistry.Instance != null)
        {
            DurableEffectRegistry.Instance.UnregisterEffect(this);
        }

        Debug.Log($"[INFO]StatuseffectInstance::OnExpire() - {EffectId} expired");
    }

    public void InvokeAttackEventEffect(AttackEvent attackEvent, BaseCharacter attacker, IEffectableProvider target)
    {
        // GroupModifier 공격 시 콜백 실행 (Toxic, Ignite, Glacial, Lifedrain 등)
        if (attackEvent == AttackEvent.OnAttack)
        {
            ModifierGroup?.TriggerOnAttack(attacker, target);
        }

        foreach (var effectData in _effectDataList)
        {
            var attackEventEffect = _statManager.GetEffectLogic(effectData.Type) as IAttackEventEffectLogic;
            attackEventEffect?.EffectByAttackEvent(attackEvent, this, effectData, attacker, target);
        }
    }



    #endregion

    #region Func for EffectLogic

    #endregion

}
