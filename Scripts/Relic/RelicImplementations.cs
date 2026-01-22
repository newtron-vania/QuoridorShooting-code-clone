using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test Relic 1: 상시 스탯 향상
/// Always 조건으로 필드의 모든 아군 캐릭터 스탯 10% 증가
/// </summary>
public class AlwaysStatBoostRelic : RelicInstance
{
    public override void Execute(RelicSignal signal)
    {
        // 모든 조건 체크 (Always는 항상 true)
        if (!CheckAllConditions(signal))
        {
            return;
        }

        // 필드의 모든 아군 캐릭터 탐색
        List<BaseCharacter> allies = GetAllAllies();

        foreach (var ally in allies)
        {
            // 이미 효과가 적용된 대상인지 체크
            if (_effect.AffectedTargets.Contains(ally))
            {
                continue;
            }

            // 효과 적용
            _effect.Apply(ally, signal, _data.LogicParams);
        }
    }

    // 모든 아군 캐릭터 반환
    private List<BaseCharacter> GetAllAllies()
    {
        var allies = new List<BaseCharacter>();

        // // BattleSystem에서 아군 캐릭터 가져오기
        // if (BattleSystem.Instance != null)
        // {
        //     var playerGroup = BattleSystem.Instance.GetCharacterGroup(CharacterIdentification.Player);
        //     if (playerGroup != null)
        //     {
        //         allies.AddRange(playerGroup);
        //     }
        // }

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
        // Subject가 BaseCharacter인지 확인
        if (signal.Subject is not BaseCharacter character)
        {
            return;
        }

        // 모든 조건 체크 (HP 50% 이하)
        if (CheckAllConditions(signal))
        {
            // 조건 충족: 효과 적용
            if (!_effect.AffectedTargets.Contains(character))
            {
                _effect.Apply(character, signal, _data.LogicParams);
                Debug.Log($"[LowHpStatBoostRelic] Applied to {character.CharacterName} (HP: {character.Hp}/{character.MaxHp})");
            }
        }
        else
        {
            // 조건 미충족: 효과 제거 (가역적)
            if (_effect.AffectedTargets.Contains(character))
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
    // 쿨다운 관리 (중복 발동 방지)
    private float _lastTriggerTurn = -999f;

    public override void Execute(RelicSignal signal)
    {
        // Subject가 BaseCharacter인지 확인
        if (signal.Subject is not BaseCharacter character)
        {
            return;
        }

        // 모든 조건 체크 (피격 발생)
        if (!CheckAllConditions(signal))
        {
            return;
        }

        // 쿨다운 체크
        if (_data.TriggerParams != null && _data.TriggerParams.ContainsKey("CooldownTurns"))
        {
            float cooldown = _data.TriggerParams["CooldownTurns"];
            float currentTurn = GameManager.Instance.Turn;

            if (currentTurn - _lastTriggerTurn < cooldown)
            {
                Debug.Log($"[DamagedBuffRelic] Cooldown active (Remaining: {cooldown - (currentTurn - _lastTriggerTurn)} turns)");
                return;
            }
        }

        // 효과 적용 (TimedStatModifier가 자동으로 Duration 관리)
        _effect.Apply(character, signal, _data.LogicParams);
        _lastTriggerTurn = GameManager.Instance.Turn;

        Debug.Log($"[DamagedBuffRelic] Applied to {character.CharacterName}, Duration: {_data.LogicParams["Duration"]} turns");
    }
}
