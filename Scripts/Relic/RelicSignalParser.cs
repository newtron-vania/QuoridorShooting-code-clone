using System.Collections.Generic;

/// <summary>
/// EventManager의 raw eventData를 RelicSignal로 변환
/// </summary>
public static class RelicSignalParser
{
    // EventData를 RelicSignal로 파싱
    public static RelicSignal Parse(RelicTriggerType triggerType, object eventData)
    {
        var signal = new RelicSignal
        {
            TriggerType = triggerType,
            RuntimeParams = new Dictionary<string, float>(),
            Timestamp = 0f
        };

        // TriggerType에 따라 eventData 파싱
        switch (triggerType)
        {
            case RelicTriggerType.OnDamaged:
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
                ParseTurnEvent(signal, eventData);
                break;

            case RelicTriggerType.OnBattleWin:
            case RelicTriggerType.OnBattleStart:
                ParseBattleEvent(signal, eventData);
                break;

            default:
                // 기본 처리: eventData가 BaseCharacter라고 가정
                if (eventData is BaseCharacter character)
                {
                    signal.Subject = character;
                }
                else
                {
                    signal.Subject = eventData;
                }
                break;
        }

        return signal;
    }

    // 피격 이벤트 파싱
    private static void ParseDamagedEvent(RelicSignal signal, object eventData)
    {
        // TODO: DamageEventData 구조에 맞게 파싱
        // 임시로 BaseCharacter로 처리
        if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
            signal.RuntimeParams["DamageAmount"] = 0f; // 실제 구현 시 데미지량 추출
        }
    }

    // 사망 이벤트 파싱
    private static void ParseDeathEvent(RelicSignal signal, object eventData)
    {
        if (eventData is BaseCharacter deadCharacter)
        {
            signal.Subject = deadCharacter;
            signal.RuntimeParams["DeathPosition"] = 0f; // 실제 구현 시 위치 정보 추출
        }
    }

    // HP 변동 이벤트 파싱
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

    // 턴 이벤트 파싱
    private static void ParseTurnEvent(RelicSignal signal, object eventData)
    {
        // eventData가 턴 번호(int) 또는 캐릭터일 수 있음
        if (eventData is int turnNumber)
        {
            signal.RuntimeParams["TurnNumber"] = turnNumber;
        }
        else if (eventData is BaseCharacter character)
        {
            signal.Subject = character;
        }
    }

    // 전투 이벤트 파싱
    private static void ParseBattleEvent(RelicSignal signal, object eventData)
    {
        // eventData가 BattleSystem 또는 RewardContext일 수 있음
        signal.Subject = eventData;
    }
}
