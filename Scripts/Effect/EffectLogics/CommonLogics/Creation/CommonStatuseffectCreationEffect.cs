using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * EffectInstance의 Source가 정의되면 사용가능 안되면 불가능
 * Source를 BaseCharacter로 제한 한다던가
 * 유물도 부여가능하면 Source가 될 수 있는 인터페이스가 필요함
 */
public class CommonStatuseffectCreationEffect : IBaseEffectLogic
{
    private StatManager _statManager;

    public CommonStatuseffectCreationEffect(StatManager statManager)
    {
        _statManager = statManager;
    }

    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList) { }


    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

        switch (effectEvent)
        {
            case EffectEvent.Start:
                if (effectData.Type == EffectType.CommonStatuseffectCreation)
                    ApplyStatuseffect(effectInstance, effectData, targetList);
                break;
            case EffectEvent.End:
                if (effectData.Type == EffectType.CommonStatuseffectCreation_OnEnd)
                    ApplyStatuseffect(effectInstance, effectData, targetList);
                break;
        }


    }

    private void ApplyStatuseffect(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        StatuseffectData statuseffectData = DataManager.Instance.GetStatuseffectData(effectData.Get<int>("StatuseffectId"));
        if (statuseffectData == null)
        {
            Debug.LogError($"[ERROR]CommonStatuseffectCreationEffect::ApplyStatuseffect: StatuseffectId {effectData.Get<int>("StatuseffectId")} not found in DataManager");
            return;
        }

        foreach (var target in targetList)
        {
            StatuseffectInstance newInstance
              = new StatuseffectInstance(_statManager, statuseffectData, effectInstance.Source, target, effectData);
            newInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
        }
    }


}
