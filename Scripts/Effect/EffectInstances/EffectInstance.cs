using System.Collections.Generic;
using UnityEngine;



//인스턴스 고유 이벤트
public enum EffectInstanceEvent
{
    Start,
    End,
}

public abstract class EffectInstance 
{
    //일단 베이스캐릭터만 효과의 원본이 되게  나중에 추가로 생기면 그때 고민
    public readonly BaseCharacter Source;

    protected readonly EffectSystem _effectSystem;

    protected readonly List<EffectData> _effectDataList = new List<EffectData>();

    public EffectInstance(BaseCharacter source, EffectSystem effectSystem)
    {
        Source = source;
        _effectSystem = effectSystem;
    }

    public abstract List<IEffectableProvider> FindEffectTargetList(TargetType effectTargetType);

    public virtual void InvokeInstanceEvent(EffectInstanceEvent instanceEvent)  
    {
        EffectEvent effectEvent;
        switch (instanceEvent)
        {
            case EffectInstanceEvent.Start:
                effectEvent = EffectEvent.Start;
                break;
            case EffectInstanceEvent.End:
                effectEvent = EffectEvent.End;
                break;
            default:
                return;
        }

        //EffectData를 통해 EffectLogic을 가져와서 실행
        foreach (EffectData effectData in _effectDataList)
        {
            List<IEffectableProvider> targetList = FindEffectTargetList(effectData.Target);

            Debug.Log($"[INFO]EffectInstance::InvokeInstanceEvent EventType: {effectEvent} EffectType: {effectData.Type} on {targetList.Count} targets");
            IBaseEffectLogic effectLogic = _effectSystem.EffectLogicDict[effectData.Type];
            effectLogic.EffectByEffectEvent(effectEvent, this, effectData, targetList);
        }
    }
}
