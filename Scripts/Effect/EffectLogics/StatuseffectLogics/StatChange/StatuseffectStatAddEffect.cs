using System.Collections.Generic;
using UnityEngine;

public class StatuseffectStatAddEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        StatuseffectInstance statusEffectInstance = effectInstance as StatuseffectInstance;

        switch (eventType)
        {
            case HM.EventType.OnTurnStart:
                break;
        }
    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //필수!!
        StatuseffectInstance statusEffectInstance = effectInstance as StatuseffectInstance;
        foreach(var target in targetList)
        {
            switch (effectEvent)
            {
                case EffectEvent.Start:
                    StartEffect(statusEffectInstance, effectData, target);
                    break;
                case EffectEvent.End:
                    EndEffect(statusEffectInstance, effectData, target);
                    break;
            }
        }
     
    }


    private void StartEffect(StatuseffectInstance statuseffectInstance, EffectData effectData, IEffectableProvider target)
    {
        var charcter = target.GetEffectable<BaseCharacter>();
        //버프 시작할 때 적용한번 해주기
        switch (effectData.Type)
        {
            case EffectType.StatuseffectStatAdd_Avd:
                charcter.CharacterStat.Avd += statuseffectInstance.InstanceData.Get<float>("StatValue");
                break;
            case EffectType.StatuseffectStatAdd_Atk:
                charcter.CharacterStat.Atk += statuseffectInstance.InstanceData.Get<int>("StatValue");
                break;
            default:
                break;
        }

    }

    private void EndEffect(StatuseffectInstance statuseffectInstance, EffectData effectData, IEffectableProvider target)
    {
        var charcter = target.GetEffectable<BaseCharacter>();
        //버프 끝날 때
        switch (effectData.Type)
        {
            case EffectType.StatuseffectStatAdd_Avd:
                charcter.CharacterStat.Avd -= statuseffectInstance.InstanceData.Get<float>("StatValue");
                break;
            case EffectType.StatuseffectStatAdd_Atk:
                charcter.CharacterStat.Atk -= statuseffectInstance.InstanceData.Get<int>("StatValue");
                break;
            default:
                break;
        }
    }
}