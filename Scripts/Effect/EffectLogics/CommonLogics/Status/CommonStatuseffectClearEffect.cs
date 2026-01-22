using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonStatuseffectClearEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

        switch (effectEvent)
        {
            case EffectEvent.Start:
                StartEffect(effectInstance, effectData, targetList);
                break;
            case EffectEvent.End:
                break;
        }
    }


    private void StartEffect(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        foreach (var target in targetList)
        {
            var statuseffectParticipant = target.GetEffectable<IStatuseffectParticipant>();

            switch (effectData.Type)
            {
                case EffectType.CommonStatuseffectClear_ByTag:
                    statuseffectParticipant.StatuseffectController.ClearStatuseffectByTag(effectData.Get<StatuseffectTag>("StatuseffectTag"));
                    break;
            }
        }
    }
}