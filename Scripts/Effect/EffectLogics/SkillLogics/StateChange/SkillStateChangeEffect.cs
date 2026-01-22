using System.Collections.Generic;
using UnityEngine;


//인스턴스의 상태를 변화시켜주는 효과
//이 인스턴스 상태는 그냥 스킬 인스턴스 DB에 붙여 쓸 수 있으면 좋을 것 같슴다.
public class SkillStateChangeEffect : IBaseEffectLogic
{
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {

    }

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        //필수!!
        SkillInstance skillInstance = effectInstance as SkillInstance;
        switch (effectEvent)
        {
            case EffectEvent.Start:
                StartEffect(skillInstance, effectData, targetList);
                break;
            case EffectEvent.End:

                break;
        }
    }

    private void StartEffect(SkillInstance skillInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        skillInstance.ChangeInstanceState(effectData.Get<SkillStateType>("SkillStateType"));
    }

}


