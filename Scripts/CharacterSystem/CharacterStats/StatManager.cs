using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HM;

public class StatManager : Singleton<StatManager>, IEventListener
{
    private Dictionary<int, CharacterStat> _characterStats = new Dictionary<int, CharacterStat>();

    // 턴 기반 지속시간 추적
    private class TimedModifierData
    {
        public int CharacterId;
        public string ModifierId;
        public int RemainingTurns;
        public bool IsGroup;

        public TimedModifierData(int characterId, string modifierId, int remainingTurns, bool isGroup)
        {
            CharacterId = characterId;
            ModifierId = modifierId;
            RemainingTurns = remainingTurns;
            IsGroup = isGroup;
        }
    }

    private Dictionary<string, TimedModifierData> _timedModifiers = new Dictionary<string, TimedModifierData>();

    public override void Awake()
    {
        base.Awake();
        EventManager.Instance.AddEvent(HM.EventType.OnTurnEnd, this);
    }

    private void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveEvent(HM.EventType.OnTurnEnd, this);
        }
    }

    public void OnEvent(HM.EventType eventType, Component sender, object param = null)
    {
        if (eventType == HM.EventType.OnTurnEnd)
        {
            ProcessTurnEnd();
        }
    }

    public void RegisterCharacter(int characterId, CharacterStat characterStat)
    {
        if (!_characterStats.ContainsKey(characterId))
        {
            _characterStats[characterId] = characterStat;
        }
    }

    public void UnregisterCharacter(int characterId)
    {
        if (_characterStats.ContainsKey(characterId))
        {
            _characterStats.Remove(characterId);

            // 해당 캐릭터의 모든 시간제한 Modifier 제거
            var keysToRemove = _timedModifiers
                .Where(kvp => kvp.Value.CharacterId == characterId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _timedModifiers.Remove(key);
            }
        }
    }

    public CharacterStat GetCharacterStat(int characterId)
    {
        return _characterStats.ContainsKey(characterId) ? _characterStats[characterId] : null;
    }

    public void AddModifierToCharacter(int characterId, StatModifier modifier)
    {
        if (_characterStats.ContainsKey(characterId))
        {
            _characterStats[characterId].AddModifier(modifier);
        }
    }

    public void RemoveModifierFromCharacter(int characterId, string modifierId)
    {
        if (_characterStats.ContainsKey(characterId))
        {
            _characterStats[characterId].RemoveModifier(modifierId);

            // 시간제한 추적에서도 제거
            string trackingKey = GetTrackingKey(characterId, modifierId);
            if (_timedModifiers.ContainsKey(trackingKey))
            {
                _timedModifiers.Remove(trackingKey);
            }
        }
    }

    public void AddModifierToAll(StatModifier modifier)
    {
        foreach (var characterStat in _characterStats.Values)
        {
            characterStat.AddModifier(modifier);
        }
    }

    public void RemoveModifierFromAll(string modifierId)
    {
        foreach (var characterStat in _characterStats.Values)
        {
            characterStat.RemoveModifier(modifierId);
        }
    }

    public void ClearAllModifiers()
    {
        foreach (var characterStat in _characterStats.Values)
        {
            characterStat.ClearAllModifiers();
        }
    }

    public List<CharacterStat> GetAllCharacterStats()
    {
        return _characterStats.Values.ToList();
    }

    public int GetCharacterCount()
    {
        return _characterStats.Count;
    }

    // ========================================
    // 턴 기반 지속시간 시스템
    // ========================================

    // 턴 기반 지속시간을 가진 Modifier를 캐릭터에게 추가
    // characterId: 대상 캐릭터 ID
    // modifier: 적용할 Modifier
    // duration: 지속 턴 수 (-1 = 무한)
    public void AddModifierWithDuration(int characterId, StatModifier modifier, int duration)
    {
        if (!_characterStats.ContainsKey(characterId))
        {
            Debug.LogWarning($"StatManager::AddModifierWithDuration() - Character {characterId} not found. Cannot add timed modifier.");
            return;
        }

        // 캐릭터에 Modifier 추가
        _characterStats[characterId].AddModifier(modifier);

        // 영구 아닌 경우 지속시간 추적
        if (duration != -1)
        {
            string trackingKey = GetTrackingKey(characterId, modifier.Id);
            _timedModifiers[trackingKey] = new TimedModifierData(characterId, modifier.Id, duration, false);
            Debug.Log($"[INFO] StatManager::AddModifierWithDuration() - Added timed modifier '{modifier.Id}' to character {characterId} for {duration} turns");
        }
    }

    // 턴 기반 지속시간을 가진 ModifierGroup을 캐릭터에게 추가
    public void AddModifierGroupWithDuration(int characterId, ModifierGroup group, int duration)
    {
        if (!_characterStats.ContainsKey(characterId))
        {
            Debug.LogWarning($"StatManager::AddModifierGroupWithDuration() - Character {characterId} not found. Cannot add timed modifier group.");
            return;
        }

        CharacterStat character = _characterStats[characterId];

        // 그룹 등록 및 적용
        character.RegisterModifierGroup(group);
        character.TryApplyModifierGroup(group.GroupId);

        // 영구 아닌 경우 지속시간 추적
        if (duration != -1)
        {
            string trackingKey = GetTrackingKey(characterId, group.GroupId);
            _timedModifiers[trackingKey] = new TimedModifierData(characterId, group.GroupId, duration, true);
            Debug.Log($"[INFO] StatManager::AddModifierGroupWithDuration() - Added timed modifier group '{group.GroupId}' to character {characterId} for {duration} turns");
        }
    }

    // 모든 캐릭터에게 턴 기반 지속시간을 가진 Modifier 추가
    public void AddModifierToAllWithDuration(StatModifier modifier, int duration)
    {
        foreach (var kvp in _characterStats)
        {
            AddModifierWithDuration(kvp.Key, modifier, duration);
        }
    }

    // 모든 캐릭터에게 턴 기반 지속시간을 가진 ModifierGroup 추가
    public void AddModifierGroupToAllWithDuration(ModifierGroup group, int duration)
    {
        foreach (var kvp in _characterStats)
        {
            // 각 캐릭터마다 새 인스턴스 생성 (상태 공유 방지)
            var groupCopy = new ModifierGroup(
                group.GroupId,
                group.CanRepeat,
                null,
                null
            );

            // Modifier 복사
            foreach (var modifier in group.Modifiers)
            {
                groupCopy.AddModifier(modifier);
            }

            AddModifierGroupWithDuration(kvp.Key, groupCopy, duration);
        }
    }

    // 턴 종료 시 자동 호출: 지속시간 감소 및 만료된 Modifier/Group 제거
    public void ProcessTurnEnd()
    {
        if (_timedModifiers.Count == 0) return;

        var expiredKeys = new List<string>();

        // 지속시간 감소 및 만료된 Modifier 수집
        foreach (var kvp in _timedModifiers)
        {
            var data = kvp.Value;
            data.RemainingTurns--;

            if (data.RemainingTurns <= 0)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        // 만료된 Modifier/Group 제거
        foreach (var key in expiredKeys)
        {
            var data = _timedModifiers[key];

            if (_characterStats.ContainsKey(data.CharacterId))
            {
                var character = _characterStats[data.CharacterId];

                if (data.IsGroup)
                {
                    // ModifierGroup 제거
                    character.UnregisterModifierGroup(data.ModifierId);
                    Debug.Log($"[INFO] StatManager::ProcessTurnEnd() - Removed expired modifier group '{data.ModifierId}' from character {data.CharacterId}");
                }
                else
                {
                    // 개별 Modifier 제거
                    character.RemoveModifier(data.ModifierId);
                    Debug.Log($"[INFO] StatManager::ProcessTurnEnd() - Removed expired modifier '{data.ModifierId}' from character {data.CharacterId}");
                }
            }

            _timedModifiers.Remove(key);
        }

        if (expiredKeys.Count > 0)
        {
            Debug.Log($"[INFO] StatManager::ProcessTurnEnd() - Removed {expiredKeys.Count} expired modifier(s)");
        }
    }

    // 특정 Modifier/Group의 남은 턴 수 조회
    public int GetRemainingTurns(int characterId, string modifierId)
    {
        string trackingKey = GetTrackingKey(characterId, modifierId);
        if (_timedModifiers.ContainsKey(trackingKey))
        {
            return _timedModifiers[trackingKey].RemainingTurns;
        }
        return -1; // 찾을 수 없거나 영구
    }

    // 트래킹 키 생성 (characterId + modifierId 조합)
    private string GetTrackingKey(int characterId, string modifierId)
    {
        return $"{characterId}_{modifierId}";
    }

    // 모든 턴 기반 Modifier/Group 제거 (디버깅/테스트용)
    public void ClearAllTimedModifiers()
    {
        foreach (var data in _timedModifiers.Values)
        {
            if (_characterStats.ContainsKey(data.CharacterId))
            {
                var character = _characterStats[data.CharacterId];
                if (data.IsGroup)
                {
                    character.UnregisterModifierGroup(data.ModifierId);
                }
                else
                {
                    character.RemoveModifier(data.ModifierId);
                }
            }
        }

        _timedModifiers.Clear();
        Debug.Log("[INFO] StatManager::ClearAllTimedModifiers() - Cleared all timed modifiers");
    }
}
