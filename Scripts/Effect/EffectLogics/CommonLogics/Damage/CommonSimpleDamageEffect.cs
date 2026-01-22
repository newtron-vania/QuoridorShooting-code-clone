using System.Collections;
using System.Collections.Generic;
using System.Linq;



public class CommonSimpleDamageEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData,List<IEffectableProvider> targetList)
    {
        switch (eventType)
        {
            case HM.EventType.OnTurnStart:
                if (effectData.Type == EffectType.CommonSimpleDamage_OnTurnStart)
                    EffectTargetDamage(effectInstance, effectData, targetList);
                break;
        }
    }
    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        switch (effectEvent)
        {
            case EffectEvent.Start:
                if (effectData.Type == EffectType.CommonSimpleDamage)
                    EffectTargetDamage(effectInstance, effectData, targetList);
                break;
        }
    }

    private void EffectTargetDamage(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //데미지 입힐 수 있는 타겟들만 우선 베이스캐릭터로
        foreach (var target in targetList)
        {
            var character = target.GetEffectable<BaseCharacter>();
            character.TakeDamage(effectInstance.Source, effectData.Get<int>("Damage"));
        }
    }
}