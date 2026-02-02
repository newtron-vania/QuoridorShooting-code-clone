using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HM;
using CharacterDefinition;

public class StatManager : Singleton<StatManager>, ITurnEventHandler
{
    /// <summary>
    /// ITurnEventHandler 구현 - 핸들러 이름
    /// </summary>
    public string HandlerName => "StatManager";

    private Dictionary<int, CharacterStat> _characterStats = new Dictionary<int, CharacterStat>();
    private Dictionary<int, BaseCharacter> _characters = new Dictionary<int, BaseCharacter>();

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

    // IDurableEffect 래퍼 클래스
    private class TimedModifierWrapper : IDurableEffect
    {
        private TimedModifierData _data;
        private StatManager _statManager;
        private BaseCharacter _owner;

        public int Duration
        {
            get => _data.RemainingTurns;
            set => _data.RemainingTurns = value;
        }

        public string EffectId => $"TimedModifier_{_data.CharacterId}_{_data.ModifierId}";
        public BaseCharacter Owner => _owner;
        public int Priority => 0;  // 스탯 modifier는 고정 우선순위 0
        public bool IsActive => Duration >= 0;

        public TimedModifierWrapper(TimedModifierData data, StatManager statManager, BaseCharacter owner)
        {
            _data = data;
            _statManager = statManager;
            _owner = owner;
        }

        public void ProcessTurnStart()
        {
            // 스탯 modifier는 턴 시작 시 특별한 처리 없음
            // Duration 감소는 ProcessTurnEnd()에서만 수행
        }

        public void ProcessTurnEnd()
        {
            Duration--;

            if (Duration <= 0)
            {
                OnExpire();
            }
        }

        public void OnExpire()
        {
            if (_statManager._characterStats.ContainsKey(_data.CharacterId))
            {
                var character = _statManager._characterStats[_data.CharacterId];

                if (_data.IsGroup)
                {
                    character.UnregisterModifierGroup(_data.ModifierId);
                    Debug.Log($"[INFO]TimedModifierWrapper::OnExpire() - Removed expired modifier group '{_data.ModifierId}' from character {_data.CharacterId}");
                }
                else
                {
                    character.RemoveModifier(_data.ModifierId);
                    Debug.Log($"[INFO]TimedModifierWrapper::OnExpire() - Removed expired modifier '{_data.ModifierId}' from character {_data.CharacterId}");
                }
            }

            string trackingKey = _statManager.GetTrackingKey(_data.CharacterId, _data.ModifierId);
            _statManager._timedModifiers.Remove(trackingKey);
            _statManager._timedModifierWrappers.Remove(trackingKey);

            if (DurableEffectRegistry.Instance != null)
            {
                DurableEffectRegistry.Instance.UnregisterEffect(this);
            }
        }
    }

    private Dictionary<string, TimedModifierWrapper> _timedModifierWrappers = new Dictionary<string, TimedModifierWrapper>();

    // Effect 조회 프록시
    private EffectLogicProvider _effectLogicProvider;

    public override void Awake()
    {
        base.Awake();
        // EventManager 직접 구독 제거 - BattleTurnController를 통해 관리됨
        // EventManager.Instance.AddEvent(HM.EventType.OnTurnEnd, this);

        // EffectLogicProvider 참조 초기화
        _effectLogicProvider = GameObject.FindObjectOfType<EffectLogicProvider>();
        if (_effectLogicProvider == null)
        {
            Debug.Log("[INFO]StatManager::Awake() - EffectLogicProvider not found, creating new instance");
            GameObject effectLogicProviderObj = new GameObject("EffectLogicProvider");
            _effectLogicProvider = effectLogicProviderObj.AddComponent<EffectLogicProvider>();
            DontDestroyOnLoad(effectLogicProviderObj);
        }
    }

    // EventManager 직접 구독 제거 - BattleTurnController를 통해 관리됨
    // private void OnDestroy() - 제거됨
    // public void OnEvent() - 제거됨

    #region ITurnEventHandler Implementation

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 시작 시 호출
    /// 현재는 사용하지 않음
    /// </summary>
    public void OnTurnStart()
    {
        // 턴 시작 시 스탯 관련 처리가 필요하면 여기에 구현
    }

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 종료 시 호출
    /// BattleTurnController에서 자동으로 호출됨
    /// </summary>
    public void OnTurnEnd()
    {
        ProcessTurnEnd();
    }

    #endregion

    // ===== Effect 관련 프록시 메서드 =====

    /// <summary>
    /// EffectLogic 조회 (EffectLogicProvider 위임)
    /// </summary>
    public IBaseEffectLogic GetEffectLogic(EffectType type)
    {
        return _effectLogicProvider?.GetEffectLogic(type);
    }

    /// <summary>
    /// StatuseffectData 조회 (DataManager 위임)
    /// </summary>
    public StatuseffectData GetStatuseffectData(int id)
    {
        return DataManager.Instance.GetStatuseffectData(id);
    }

    /// <summary>
    /// EffectLogicDict 전체 조회 (필요 시)
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyDictionary<EffectType, IBaseEffectLogic> GetEffectLogicDict()
    {
        return _effectLogicProvider?.EffectLogicDict;
    }

    /// <summary>
    /// StatuseffectInstance 생성 헬퍼 메서드
    /// EffectInstance 없이 직접 StatuseffectInstance를 생성해야 하는 경우 사용
    /// (예: 셀 효과, 아이템 효과 등)
    /// </summary>
    /// <param name="statusEffectId">StatuseffectData ID</param>
    /// <param name="source">효과 시전자 (셀 효과의 경우 캐릭터 자신이 source)</param>
    /// <param name="target">효과 대상</param>
    /// <param name="durationOverride">Duration 덮어쓰기 (선택, -1이면 StatuseffectData 기본값 사용)</param>
    /// <returns>생성된 StatuseffectInstance 또는 null</returns>
    public StatuseffectInstance CreateStatuseffectInstance(int statusEffectId, BaseCharacter source, BaseCharacter target, int durationOverride = -1)
    {
        // StatuseffectData 조회
        var statusEffectData = GetStatuseffectData(statusEffectId);
        if (statusEffectData == null)
        {
            Debug.LogError($"[ERROR] StatManager::CreateStatuseffectInstance() - StatuseffectData not found for ID: {statusEffectId}");
            return null;
        }

        // EffectData 생성 (Duration 설정용)
        var instanceData = new EffectData
        {
            Type = EffectType.None, // StatuseffectInstance에서는 사용 안 함
            Target = TargetType.Self
        };

        // Duration 설정
        if (durationOverride >= 0)
        {
            // Duration 덮어쓰기
            instanceData.Params["Duration"] = new Param { TypeName = "int", Raw = durationOverride.ToString() };
        }
        else
        {
            // StatuseffectData의 기본 Duration 사용
            // StatuseffectData에 EffectDataList가 없거나 Duration 파라미터가 없으면 0 사용
            int defaultDuration = 0;
            if (statusEffectData.EffectDataList != null &&
                statusEffectData.EffectDataList.Count > 0 &&
                statusEffectData.EffectDataList[0].Params.ContainsKey("Duration"))
            {
                defaultDuration = statusEffectData.EffectDataList[0].Get<int>("Duration");
            }
            instanceData.Params["Duration"] = new Param { TypeName = "int", Raw = defaultDuration.ToString() };
        }

        // StatuseffectInstance 생성
        var statusEffectInstance = new StatuseffectInstance(this, statusEffectData, source, target, instanceData);

        Debug.Log($"[INFO] StatManager::CreateStatuseffectInstance() - Created StatuseffectInstance (ID: {statusEffectId}, Duration: {statusEffectInstance.Duration})");

        return statusEffectInstance;
    }

    // ===== 기존 메서드 =====

    public void RegisterCharacter(int characterId, CharacterStat characterStat, BaseCharacter character = null)
    {
        if (!_characterStats.ContainsKey(characterId))
        {
            _characterStats[characterId] = characterStat;
            if (character != null)
            {
                _characters[characterId] = character;
            }
        }
    }

    public void UnregisterCharacter(int characterId)
    {
        if (_characterStats.ContainsKey(characterId))
        {
            _characterStats.Remove(characterId);
            _characters.Remove(characterId);

            // 해당 캐릭터의 모든 시간제한 Modifier 제거
            var keysToRemove = _timedModifiers
                .Where(kvp => kvp.Value.CharacterId == characterId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _timedModifiers.Remove(key);
                _timedModifierWrappers.Remove(key);
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

        // 영구 아닌 경우 지속시간 추적 및 DurableEffectRegistry 등록
        if (duration != -1)
        {
            string trackingKey = GetTrackingKey(characterId, modifier.Id);
            var timedModifierData = new TimedModifierData(characterId, modifier.Id, duration, false);
            _timedModifiers[trackingKey] = timedModifierData;

            // BaseCharacter 가져오기 (characterId로부터)
            BaseCharacter owner = GetBaseCharacterFromId(characterId);
            if (owner != null)
            {
                var wrapper = new TimedModifierWrapper(timedModifierData, this, owner);
                _timedModifierWrappers[trackingKey] = wrapper;

                if (DurableEffectRegistry.Instance != null)
                {
                    DurableEffectRegistry.Instance.RegisterEffect(wrapper);
                }
            }

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

        // 영구 아닌 경우 지속시간 추적 및 DurableEffectRegistry 등록
        if (duration != -1)
        {
            string trackingKey = GetTrackingKey(characterId, group.GroupId);
            var timedModifierData = new TimedModifierData(characterId, group.GroupId, duration, true);
            _timedModifiers[trackingKey] = timedModifierData;

            // BaseCharacter 가져오기
            BaseCharacter owner = GetBaseCharacterFromId(characterId);
            if (owner != null)
            {
                var wrapper = new TimedModifierWrapper(timedModifierData, this, owner);
                _timedModifierWrappers[trackingKey] = wrapper;

                if (DurableEffectRegistry.Instance != null)
                {
                    DurableEffectRegistry.Instance.RegisterEffect(wrapper);
                }
            }

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

    // characterId로부터 BaseCharacter 가져오기
    private BaseCharacter GetBaseCharacterFromId(int characterId)
    {
        if (_characters.ContainsKey(characterId))
        {
            return _characters[characterId];
        }
        return null;
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
