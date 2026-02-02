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

    // GroupModifier 시스템: 행동 제한 플래그 관리
    private ActionRestrictionFlags _activeRestrictions = ActionRestrictionFlags.None;
    private Dictionary<ActionRestrictionFlags, int> _restrictionRefCounts = new();

    public ActionRestrictionFlags ActiveRestrictions => _activeRestrictions;


    public StatuseffectController()
    {
        Init();
    }


    public void Init()
    {
        _restrictionRefCounts.Clear();
        _activeRestrictions = ActionRestrictionFlags.None;
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
        if(OwnedStatuseffectDict.TryGetValue(instance.StatuseffectType, out var existingInstance))
        {
            // 이미 같은 타입의 상태이상이 존재하는 경우
            // Duration 비교: 더 큰 Duration을 가진 효과로 덮어쓰기
            if (instance.Duration > existingInstance.Duration)
            {
                // 새 효과의 Duration이 더 크면 기존 효과 제거 후 새 효과 적용
                Debug.Log($"[INFO] StatuseffectController::AddStatusEffectInstance - Replacing {instance.StatuseffectType} (Duration {existingInstance.Duration} → {instance.Duration})");
                existingInstance.OnExpire(); // GroupModifier 제거 + End 이벤트
                OwnedStatuseffectDict[instance.StatuseffectType] = instance;
            }
            else
            {
                // 기존 효과의 Duration이 더 크거나 같으면 새 효과 무시
                Debug.Log($"[INFO] StatuseffectController::AddStatusEffectInstance - Ignoring {instance.StatuseffectType} (Duration {instance.Duration} ≤ {existingInstance.Duration})");
                // 새 인스턴스는 등록되지 않으므로 Registry에서 제거 필요
                if (DurableEffectRegistry.Instance != null)
                {
                    DurableEffectRegistry.Instance.UnregisterEffect(instance);
                }
            }
        }
        else
        {
            // 새로운 상태이상 추가
            OwnedStatuseffectDict.Add(instance.StatuseffectType, instance);
            Debug.Log($"[INFO] StatuseffectController::AddStatusEffectInstance - Added {instance.StatuseffectType} (Duration {instance.Duration})");
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

    #region GroupModifier - ActionRestriction Methods

    /// <summary>
    /// 행동 제한 플래그 적용 (참조 카운트 방식)
    /// 여러 상태이상이 동일한 제한을 걸 수 있으므로 참조 카운트로 관리
    /// </summary>
    public void ApplyRestriction(ActionRestrictionFlags restriction)
    {
        if (restriction == ActionRestrictionFlags.None)
            return;

        // 각 플래그별로 참조 카운트 증가
        foreach (ActionRestrictionFlags flag in Enum.GetValues(typeof(ActionRestrictionFlags)))
        {
            if (flag == ActionRestrictionFlags.None)
                continue;

            if ((restriction & flag) != 0)
            {
                if (!_restrictionRefCounts.ContainsKey(flag))
                    _restrictionRefCounts[flag] = 0;

                _restrictionRefCounts[flag]++;
                _activeRestrictions |= flag;
            }
        }

        Debug.Log($"[INFO] StatuseffectController::ApplyRestriction - Active restrictions: {_activeRestrictions}");
    }

    /// <summary>
    /// 행동 제한 플래그 제거 (참조 카운트 방식)
    /// 참조 카운트가 0이 되면 플래그 제거
    /// </summary>
    public void RemoveRestriction(ActionRestrictionFlags restriction)
    {
        if (restriction == ActionRestrictionFlags.None)
            return;

        // 각 플래그별로 참조 카운트 감소
        foreach (ActionRestrictionFlags flag in Enum.GetValues(typeof(ActionRestrictionFlags)))
        {
            if (flag == ActionRestrictionFlags.None)
                continue;

            if ((restriction & flag) != 0)
            {
                if (_restrictionRefCounts.ContainsKey(flag))
                {
                    _restrictionRefCounts[flag]--;

                    if (_restrictionRefCounts[flag] <= 0)
                    {
                        _restrictionRefCounts.Remove(flag);
                        _activeRestrictions &= ~flag; // 플래그 제거
                    }
                }
            }
        }

        Debug.Log($"[INFO] StatuseffectController::RemoveRestriction - Active restrictions: {_activeRestrictions}");
    }

    /// <summary>
    /// 특정 제한이 활성화되어 있는지 확인
    /// </summary>
    public bool HasRestriction(ActionRestrictionFlags restriction)
    {
        return (_activeRestrictions & restriction) != 0;
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
        // Duration 처리는 DurableEffectRegistry가 담당
        // 만료된 StatuseffectInstance Dictionary 정리만 수행
        foreach (var instance in OwnedStatuseffectDict.Values.ToList())
        {
            // IsActive로 만료 확인 (Duration <= 0)
            if (!instance.IsActive)
            {
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
