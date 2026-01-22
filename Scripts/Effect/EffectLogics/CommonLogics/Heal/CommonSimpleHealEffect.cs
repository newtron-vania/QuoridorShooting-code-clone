using System.Collections.Generic;

public class CommonSimpleHealEffect : IBaseEffectLogic
{
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
                    Heal(effectInstance, effectData, targetList);
                    break;
                case EffectEvent.End:
                    
                    break;
            }
        }
    }

    private void Heal(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        foreach (var target in targetList)
        {
            var character = target.GetEffectable<BaseCharacter>();
            switch (effectData.Type)
            {
                case EffectType.CommonSimpleHeal:
                    int healAmount = effectData.Get<int>("HealAmount");
                    character.characterStat.Hp+= healAmount;
                    break;
                default:
                    break;
            }
        }
    }
}
