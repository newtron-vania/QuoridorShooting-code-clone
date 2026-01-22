using System;
using System.Collections.Generic;

public class StatuseffectInstance : EffectInstance
{
    //Id별 상태 이름
    public enum Type
    {
        Blined = 1,
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
    /// 변경하면 위험한 데이터
    /// </summary>
    public readonly EffectData InstanceData;

    public StatuseffectInstance(EffectSystem effectSystem, StatuseffectData statusEffectData, BaseCharacter source, IEffectableProvider target, EffectData instanceData) : base(source, effectSystem)
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
                break;
        }

        base.InvokeInstanceEvent(instanceEvent);
    }

    //게임 이벤트
    public void OnGameEvent(HM.EventType eventType)
    {
        switch (eventType)
        {
            case HM.EventType.OnTurnEnd:
                Duration--;
                break;
            
        }

        foreach (EffectData effectData in _effectDataList)
        {
            List<IEffectableProvider> targetList = FindEffectTargetList(effectData.Target);
            IBaseEffectLogic effectLogic = _effectSystem.EffectLogicDict[effectData.Type];
            effectLogic.EffectByGameEvent(eventType, this, effectData, targetList);
        }
    }

    public void InvokeAttackEventEffect(AttackEvent attackEvent, BaseCharacter attacker, IEffectableProvider target)
    {
        foreach (var effectData in _effectDataList)
        {
            var attackEventEffect = _effectSystem.EffectLogicDict[effectData.Type] as IAttackEventEffectLogic;
            attackEventEffect?.EffectByAttackEvent(attackEvent, this, effectData, attacker, target);
        }
    }



    #endregion

    #region Func for EffectLogic

    #endregion

}
