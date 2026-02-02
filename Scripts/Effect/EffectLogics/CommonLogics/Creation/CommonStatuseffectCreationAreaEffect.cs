using System.Collections.Generic;
using UnityEngine;

public class CommonStatuseffectCreationAreaEffect : IBaseEffectLogic, IMovableEventEffectLogic
{
    private readonly StatManager _statManager;
    public CommonStatuseffectCreationAreaEffect(StatManager statManager)
    {
        _statManager = statManager;
    }


    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //필수!!
        SkillInstance skillInstance = effectInstance as SkillInstance;
        switch (effectEvent)
        {
            case EffectEvent.Start:
                StartEffect(skillInstance, effectData, targetList);
                break;
            case EffectEvent.End:
                EndEffect(skillInstance, effectData, targetList);
                break;
        }
    }

    private void StartEffect(SkillInstance skillInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

        foreach (var target in targetList)
            OnCellEnter(skillInstance, effectData, target);
    }

    private void EndEffect(SkillInstance skillInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        if (effectData.Type != EffectType.CommonStatuseffectCreationArea_AreaOnly)
            return;
        //영역에 있던 사람들 디버프 제거
        foreach (var target in targetList)
        {
            var statuseffectController = target.GetEffectable<IStatuseffectParticipant>();
            OnCellExit(skillInstance, effectData, target);
        }

    }

    public void EffectByMovableEvent(MovableEvent movableEvent, EffectInstance effectInstance, EffectData effectData, IEffectableProvider target)
    {
        //필수!!
        SkillInstance skillInstance = effectInstance as SkillInstance;

        switch (movableEvent)
        {
            case MovableEvent.OnCellEnter:
                OnCellEnter(skillInstance, effectData, target);
                break;
            case MovableEvent.OnCellExit:
                if (effectData.Type == EffectType.CommonStatuseffectCreationArea_AreaOnly)
                    OnCellExit(skillInstance, effectData, target);
                break;
        }
    }

    private void OnCellEnter(SkillInstance skillInstance, EffectData effectData, IEffectableProvider target)
    {
        var statuseffectTarget = target.GetEffectable<IStatuseffectParticipant>();

        //들어온 사람들 디버프
        ApplyStatuseffect(skillInstance, effectData, effectData.Get<int>("StatuseffectId"), target);

    }

    private void OnCellExit(SkillInstance skillInstance, EffectData effectData, IEffectableProvider target)
    {
        var statuseffectTarget = target.GetEffectable<IStatuseffectParticipant>();

        //나간 사람들 원래대로
        StatuseffectInstance statuseffect = statuseffectTarget.StatuseffectController.OwnedStatuseffectDict[(StatuseffectInstance.Type)effectData.Get<int>("StatuseffectId")];
        statuseffect.InvokeInstanceEvent(EffectInstanceEvent.End); //이런건 컨트롤러에서 리무브할 때 호출하게 변경해도 될 듯
        statuseffectTarget.StatuseffectController.OwnedStatuseffectDict.Remove(statuseffect.StatuseffectType);
    }


    private void ApplyStatuseffect(EffectInstance effectInstance, EffectData effectData, int statuseffectId, IEffectableProvider target)
    {
        StatuseffectData statuseffectData = DataManager.Instance.GetStatuseffectData(statuseffectId);
        if (statuseffectData == null)
        {
            Debug.LogError($"[ERROR]SkillStateChangeEffect::ApplyStatuseffect: StatuseffectId {statuseffectId} not found in DataManager");
            return;
        }

        StatuseffectInstance newInstance
              = new StatuseffectInstance(_statManager, statuseffectData, effectInstance.Source, target, effectData);
        newInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
    }
}