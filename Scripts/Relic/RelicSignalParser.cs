using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EventManager의 raw eventData를 RelicSignal로 변환
/// DamageEventData 구조체 지원 (§11.12)
/// </summary>
public static class RelicSignalParser
{
    public static RelicSignal Parse(RelicTriggerType triggerType, object eventData)
    {
        var signal = new RelicSignal
        {
            TriggerType = triggerType,
            RuntimeParams = new Dictionary<string, float>(),
            Timestamp = Time.time
        };

        switch (triggerType)
        {
            case RelicTriggerType.OnDamaged:
            case RelicTriggerType.OnDealDamage:
                ParseDamagedEvent(signal, eventData);
                break;

            case RelicTriggerType.OnCharacterDeath:
                ParseDeathEvent(signal, eventData);
                break;

            case RelicTriggerType.OnHpChanged:
            case RelicTriggerType.OnHpBelowThreshold:
                ParseHpChangedEvent(signal, eventData);
                break;

            case RelicTriggerType.OnTurnStart:
            case RelicTriggerType.OnTurnEnd:
            case RelicTriggerType.OnFirstTurn:
                ParseTurnEvent(signal, eventData);
                break;

            case RelicTriggerType.OnBattleWin:
            case RelicTriggerType.OnBattleStart:
            case RelicTriggerType.OnBattleLose:
            case RelicTriggerType.OnStageComplete:
                ParseBattleEvent(signal, eventData);
                break;

            case RelicTriggerType.OnSkillUsed:
                ParseSkillEvent(signal, eventData);
                break;

            case RelicTriggerType.OnHitSuccess:
            case RelicTriggerType.OnHitFail:
            case RelicTriggerType.OnDefenseSuccess:
            case RelicTriggerType.OnDefenseFail:
                ParseCombatDetailEvent(signal, eventData);
                break;

            default:
                if (eventData is BaseCharacter character)
                    signal.Subject = character;
                else
                    signal.Subject = eventData;
                break;
        }

        return signal;
    }

    private static void ParseDamagedEvent(RelicSignal signal, object eventData)
    {
        // DamageEventData 구조체가 있는 경우
        if (eventData is DamageEventData damageData)
        {
            signal.Subject = damageData.Target;
            signal.Attacker = damageData.Attacker;
            signal.DamageAmount = damageData.FinalDamage;
            signal.ContextData = damageData;
            signal.SkillId = damageData.SkillId;
            signal.RuntimeParams["DamageAmount"] = damageData.FinalDamage;
            signal.RuntimeParams["RawDamage"] = damageData.RawDamage;
            signal.RuntimeParams["IsCritical"] = damageData.IsCritical ? 1f : 0f;
        }
        // 하위호환: BaseCharacter만 전달되는 경우
        else if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
            signal.DamageAmount = 0f;
            signal.RuntimeParams["DamageAmount"] = 0f;
        }
    }

    private static void ParseDeathEvent(RelicSignal signal, object eventData)
    {
        if (eventData is DamageEventData damageData)
        {
            signal.Subject = damageData.Target;
            signal.Attacker = damageData.Attacker;
            signal.ContextData = damageData;
        }
        else if (eventData is BaseCharacter deadCharacter)
        {
            signal.Subject = deadCharacter;
        }
    }

    private static void ParseHpChangedEvent(RelicSignal signal, object eventData)
    {
        if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
            signal.RuntimeParams["CurrentHp"] = character.Hp;
            signal.RuntimeParams["MaxHp"] = character.MaxHp;
            signal.RuntimeParams["HpRatio"] = (float)character.Hp / character.MaxHp;
        }
    }

    private static void ParseTurnEvent(RelicSignal signal, object eventData)
    {
        if (eventData is int turnNumber)
        {
            signal.RuntimeParams["TurnNumber"] = turnNumber;
        }
        else if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
        }

        // 현재 턴 정보 항상 추가
        signal.RuntimeParams["CurrentTurn"] = GameManager.Instance.Turn;
    }

    private static void ParseBattleEvent(RelicSignal signal, object eventData)
    {
        signal.Subject = eventData;
    }

    private static void ParseSkillEvent(RelicSignal signal, object eventData)
    {
        if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
        }
        // TODO: SkillEventData 구조체 지원 시 SkillId 파싱 추가
    }

    private static void ParseCombatDetailEvent(RelicSignal signal, object eventData)
    {
        if (eventData is DamageEventData damageData)
        {
            signal.Subject = damageData.Target;
            signal.Attacker = damageData.Attacker;
            signal.DamageAmount = damageData.FinalDamage;
            signal.ContextData = damageData;
            signal.RuntimeParams["DamageAmount"] = damageData.FinalDamage;
        }
        else if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
        }
    }
}
