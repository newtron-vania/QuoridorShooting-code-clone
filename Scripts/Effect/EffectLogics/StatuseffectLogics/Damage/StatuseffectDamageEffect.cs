using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatuseffectDamageEffect :  IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //필수!!
        StatuseffectInstance statuseffectInstance = effectInstance as StatuseffectInstance;

        foreach (var target in targetList)
        {
            switch (eventType)
            {
                case HM.EventType.OnTurnStart:
                    //턴 시작할 때 데미지 주기
                    if(effectData.Type==EffectType.StatuseffectDamage_OnTurnStart)
                    {
                        var character = target.GetEffectable<BaseCharacter>();
                        //effectData의 Param이 아니라 인스턴스가 만들어질 때의 인스턴스데이터에서 적용
                        int damage = statuseffectInstance.InstanceData.Get<int>("DamageValue");
                        character.TakeDamage(statuseffectInstance.Source,damage);
                    }    

                    break;
            }
        }
    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //필수!!
        StatuseffectInstance statusEffectInstance = effectInstance as StatuseffectInstance;
        foreach (var target in targetList)
        {
            switch (effectEvent)
            {
                case EffectEvent.Start:
                  
                    break;
                case EffectEvent.End:

                    break;
            }
        }
    }
}
