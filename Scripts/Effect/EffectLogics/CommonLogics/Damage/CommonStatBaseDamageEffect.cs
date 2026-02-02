using System.Collections.Generic;


//Source가 ChatarcterBase 즉 스텟을 확인할 수 있어서 커먼하게 가능
public class CommonStatBaseDamageEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance instance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        
    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        switch (effectEvent)
        {
            case EffectEvent.Start:
                DamageEffect(effectInstance, effectData, targetList);
                break;
            case EffectEvent.End:
              
                break;
        }
    }

    private void DamageEffect(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

        int dmg=0;
        switch(effectData.Type)
        {
            case EffectType.CommonStatBaseDamage_AttackDamage:  // 원본: CommonStatBaseDamage_Atk

                dmg = effectInstance.Source.CharacterStat.Atk * effectData.Get<int>("StatMultiplier") + effectData.Get<int>("ExtraDamage");
                break;
        }

        //데미지를 입힐 수 있는 타겟이어야함 일단은 베이스캐릭터로
        foreach (var target in targetList)
        {
            var character = target.GetEffectable<BaseCharacter>();
            character.TakeDamage(effectInstance.Source,dmg);
        }

    }
    private void EndEffect(EffectInstance instance, EffectData effectData) { }
}
