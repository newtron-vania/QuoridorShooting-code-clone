

//Effect고유의 이벤트
//EffectInstance랑 비슷하면 대체 가능
//이펙트로직 기본 이벤트는 인스턴스 이벤트랑 같이 진행하고 분기되는건 인터페이스로 관리?
using System.Collections.Generic;

public enum EffectEvent
{
    Start,
    End,
}


public interface IBaseEffectLogic
{
    //이것도 인터페이스 분리 가능하긴하네요
    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList);

    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList);

}


