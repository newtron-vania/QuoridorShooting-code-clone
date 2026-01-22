// 캐릭터가 턴당 가능한 액션횟수를 올리는 효과
// 시작할 때 한번 올려주는 건 맞는데 그 다음은 캐릭터 액션 횟수 리셋 타이밍 이후에 해줘야함 or 따로 스텟으로 빼거나
using System.Collections.Generic;

public class CommonActionCountAddEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

    }




    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

        switch (effectEvent)
        {
            case EffectEvent.Start:
                AddActionCount(effectData, targetList, effectData.Get<int>("ExtraActionCount"));
                break;
            case EffectEvent.End:
                AddActionCount(effectData, targetList, -effectData.Get<int>("ExtraActionCount"));
                break;
        }

    }

    private void AddActionCount(EffectData effectData, List<IEffectableProvider> targetList, int value)
    {
        foreach (var target in targetList)
        {
            var statuseffectTarget = target.GetEffectable<IStatuseffectParticipant>();

            switch (effectData.Type)
            {
                case EffectType.CommonActionCountAdd_Attack:
                    statuseffectTarget.StatuseffectController.AdditionalAttackActionCount += value;
                    break;
                case EffectType.CommonActionCountAdd_Move:
                    statuseffectTarget.StatuseffectController.AdditionalMoveActionCount += value;
                    break;
            }
        }
    }


   
}





