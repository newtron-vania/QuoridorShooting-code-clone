
using System.Collections.Generic;

public class CommonSimpleStatAddEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        foreach(var target in targetList)
        {
            switch (effectEvent)
            {
                case EffectEvent.Start:
                    AddFixedStat(effectInstance, effectData, targetList);
                    break;
                case EffectEvent.End:
                    ReduceFixedStat(effectInstance, effectData, targetList);
                    break;
            }
        }
    }


    private void AddFixedStat(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        foreach (var target in targetList)
        {
            var character = target.GetEffectable<BaseCharacter>();
            //버프 시작할 때 적용한번 해주기
            switch (effectData.Type)
            {
                case EffectType.CommonStatAdd_ActionPointPerTurn:  // 원본: CommonStatAdd_ApRecovery
                    character.characterStat.ApRecovery += effectData.Get<int>("ExtraStat");
                    break;
                case EffectType.CommonStatAdd_DamageResistance:  // 원본: CommonStatAdd_Avd
                    character.characterStat.Avd += effectData.Get<float>("ExtraStat");
                    break;
                default:
                    break;
            }
        }

    }

    private void ReduceFixedStat(EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        foreach (var target in targetList)
        {
            var character = target.GetEffectable<BaseCharacter>();
            //버프 끝날 때
            switch (effectData.Type)
            {
                case EffectType.CommonStatAdd_ActionPointPerTurn:  // 원본: CommonStatAdd_ApRecovery
                    character.characterStat.ApRecovery -= effectData.Get<int>("ExtraStat");
                    break;
                case EffectType.CommonStatAdd_DamageResistance:  // 원본: CommonStatAdd_Avd
                    character.characterStat.Avd -= effectData.Get<float>("ExtraStat");
                    break;
                default:
                    break;
            }
        }
    }
}