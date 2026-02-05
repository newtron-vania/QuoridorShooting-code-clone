using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Test Relic 1: 상시 스탯 향상
/// Always 조건으로 필드의 모든 아군 캐릭터 스탯 10% 증가
/// Phase 2B에서 DataDrivenRelicInstance로 대체 예정
/// </summary>
public class AlwaysStatBoostRelic : RelicInstance
{
    public override void Execute(RelicSignal signal)
    {
        if (!CheckAllConditions(signal))
            return;

        List<BaseCharacter> allies = GetAllAllies();

        foreach (var ally in allies)
        {
            if (_effect != null && _effect.AffectedTargets.Contains(ally))
                continue;

            _effect?.Apply(ally, signal, ConvertLogicParams());
        }
    }

    private List<BaseCharacter> GetAllAllies()
    {
        var allies = new List<BaseCharacter>();
        // BattleSystem에서 아군 캐릭터 가져오기
        // Phase 2B에서 RelicTargetResolver로 대체
        return allies;
    }
}

/// <summary>
/// Test Relic 2: HP 50% 이하일 때 스탯 증가
/// HPRatioCondition으로 체크, HP가 50% 이상으로 회복되면 효과 제거 (가역적)
/// </summary>
public class LowHpStatBoostRelic : RelicInstance
{
    public override void Execute(RelicSignal signal)
    {
        if (signal.Subject is not BaseCharacter character)
            return;

        if (CheckAllConditions(signal))
        {
            if (_effect != null && !_effect.AffectedTargets.Contains(character))
            {
                _effect.Apply(character, signal, ConvertLogicParams());
                Debug.Log($"[LowHpStatBoostRelic] Applied to {character.CharacterName} (HP: {character.Hp}/{character.MaxHp})");
            }
        }
        else
        {
            if (_effect != null && _effect.AffectedTargets.Contains(character))
            {
                _effect.Remove(character);
                Debug.Log($"[LowHpStatBoostRelic] Removed from {character.CharacterName} (HP: {character.Hp}/{character.MaxHp})");
            }
        }
    }
}

/// <summary>
/// Test Relic 3: 피격 시 2턴 동안 스탯 증가
/// DamagedCondition으로 피격 체크, TimedStatModifier로 2턴 후 자동 제거
/// </summary>
public class DamagedBuffRelic : RelicInstance
{
    private float _lastTriggerTurn = -999f;

    public override void Execute(RelicSignal signal)
    {
        if (signal.Subject is not BaseCharacter character)
            return;

        if (!CheckAllConditions(signal))
            return;

        // 쿨다운 체크
        if (_data.TriggerParams != null && _data.TriggerParams.TryGetValue("CooldownTurns", out var cooldownToken))
        {
            float cooldown = cooldownToken.Value<float>();
            float currentTurn = GameManager.Instance.Turn;

            if (currentTurn - _lastTriggerTurn < cooldown)
            {
                Debug.Log($"[DamagedBuffRelic] Cooldown active (Remaining: {cooldown - (currentTurn - _lastTriggerTurn)} turns)");
                return;
            }
        }

        _effect?.Apply(character, signal, ConvertLogicParams());
        _lastTriggerTurn = GameManager.Instance.Turn;

        if (_data.LogicParams != null && _data.LogicParams.ContainsKey("Duration"))
            Debug.Log($"[DamagedBuffRelic] Applied to {character.CharacterName}, Duration: {_data.LogicParams["Duration"]} turns");
    }
}

