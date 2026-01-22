using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




//자신에게 걸린 상태이상 관리
//BaseCharacter에 각각 부착
//
public class StatuseffectController 
{
    public Dictionary<StatuseffectInstance.Type, StatuseffectInstance> OwnedStatuseffectDict = new(); //타입 당 걸린 이펙트?

    public int AdditionalAttackActionCount = 0;
    public int AdditionalMoveActionCount = 0;


    public StatuseffectController()
    {
        Init();
    }


    public void Init()
    {

    }

    #region Funcs for Character
    public bool CheckUnderStatuseffect(StatuseffectInstance.Type statuseffectType)
    {
        return OwnedStatuseffectDict.ContainsKey(statuseffectType);
    }
    #endregion

    #region Funcs for EffectInstance
    public void AddStatusEffectInstance(StatuseffectInstance instance)
    {
        if(OwnedStatuseffectDict.TryGetValue(instance.StatuseffectType, out var effectInstance))
        {
            //이미 있는 상태면 종료하고 덮어씌우기 
            //ToDo: 인스턴스 타입별로 갱신되는 방법 다르게? 지속시간 증가된다거나 큰쪽으로 덮어씌워진다던가 
            effectInstance.InvokeInstanceEvent(EffectInstanceEvent.End);
            //덮어씌울 때Source가 달라지는거 명심
            OwnedStatuseffectDict[instance.StatuseffectType] = instance;
        }
        else
        {
            OwnedStatuseffectDict.Add(instance.StatuseffectType, instance);
        }

    }
    #endregion


    #region Funcs for StatuseffectManage
    public void ClearStatuseffectByTag(StatuseffectTag statuseffectTag)
    {
        foreach(var instance in OwnedStatuseffectDict.Values.ToList())
        {
            if (instance.TagSet.Contains(statuseffectTag))
            {
                instance.InvokeInstanceEvent(EffectInstanceEvent.End);
                OwnedStatuseffectDict.Remove(instance.StatuseffectType);
            }
        }
    }

    #endregion
    #region EventHandle
    //게임 이벤트
    public void InvokeGameEvent(HM.EventType eventType)
    {
        switch (eventType)
        {
            case HM.EventType.OnTurnStart:
                OnTurnStart();
                break;
            case HM.EventType.OnRoundEnd:
               
                break;
            case HM.EventType.OnTurnEnd:
                OnTurnEnd();
                break;
        }

    }
    
    
    //플레이어 컨트롤러에 의해 자기 턴 종료시에만 실행중 
    private void OnTurnEnd()
    {
        foreach (var instance in OwnedStatuseffectDict.Values.ToList())
        {
            instance.OnGameEvent(HM.EventType.OnTurnEnd);
            if (instance.Duration < 0)
            {
                instance.InvokeInstanceEvent(EffectInstanceEvent.End);
                OwnedStatuseffectDict.Remove(instance.StatuseffectType);
            }
        }
    }




    private void OnTurnStart()
    {
        foreach(var instance in OwnedStatuseffectDict.Values.ToList())
        {
            instance.OnGameEvent(HM.EventType.OnTurnStart);
        }
    }

    public void OnAttack(BaseCharacter attacker,IEffectableProvider target)
    {
        foreach(var statuseffectInstance in OwnedStatuseffectDict.Values)
        {
            statuseffectInstance.InvokeAttackEventEffect(AttackEvent.OnAttack, attacker,target);
        }
    }
    #endregion

}
