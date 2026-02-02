
using System.Collections.Generic;
using UnityEngine;

//ToDo: 상태이상 대미지를 몇 줄거고 몇 턴 지속 할건지에 대한 정보를 어디서 받아올 건지 기획과 꼭 논의!!!!!!!!!!!!!!!!!!
public class StatuseffectDamageStatuseffectCreationEffect : IBaseEffectLogic, IAttackEventEffectLogic
{

    private StatManager _statManager;

    public StatuseffectDamageStatuseffectCreationEffect(StatManager statManager)
    {
        _statManager = statManager;
    }

    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

    }
    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
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
    public void EffectByAttackEvent(AttackEvent attackEvent, EffectInstance effectInstance, EffectData effectData, BaseCharacter attacker, IEffectableProvider target)
    {
        StatuseffectInstance statuseffectInstance = effectInstance as StatuseffectInstance;
        switch (attackEvent)
        {
            case AttackEvent.OnAttack:
                if (effectData.Type == EffectType.StatuseffectDamageStatuseffectCreation_OnAttack)
                {
                    ApplyStatuseffect(statuseffectInstance, effectData, attacker, target);
                }
                break;
        }
    }

    public void ApplyStatuseffect(StatuseffectInstance statuseffectInstance, EffectData effectData, BaseCharacter attacker, IEffectableProvider target)
    {

        StatuseffectData statuseffectData = DataManager.Instance.GetStatuseffectData(effectData.Get<int>("StatuseffectId"));
        if (statuseffectData == null)
        {

            Debug.LogError($"[ERROR]CommonStatuseffectCreationEffect::ApplyStatuseffect: StatuseffectId {effectData.Get<int>("StatuseffectId")} not found in DataManager");
            return;
        }


        StatuseffectInstance newInstance
                  = new StatuseffectInstance(_statManager, statuseffectData, attacker, target, effectData);

        newInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);

    }
}
