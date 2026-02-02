using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterDefinition;
using HM;
using Battle;

/// <summary>
/// Duration 효과 등록 및 관리를 위한 중앙 집중식 레지스트리
/// BattleTurnController를 통해 턴 종료 시 모든 Duration 효과를 처리
/// Priority 기반 실행 순서 제어 (PriorityEventQueue 사용)
/// </summary>
public class DurableEffectRegistry : Singleton<DurableEffectRegistry>, ITurnEventHandler
{
    /// <summary>
    /// ITurnEventHandler 구현 - 핸들러 이름
    /// </summary>
    public string HandlerName => "DurableEffectRegistry";

    // 캐릭터별 Duration 효과 그룹화 (Priority 기반 우선순위 큐)
    private Dictionary<BaseCharacter, PriorityEventQueue<IDurableEffect>> _characterEffects = new Dictionary<BaseCharacter, PriorityEventQueue<IDurableEffect>>();

    // EventManager 직접 구독 제거 - BattleTurnController를 통해 관리됨
    // void OnEnable() - 제거됨
    // void OnDisable() - 제거됨
    // public void OnEvent() - 제거됨

    #region ITurnEventHandler Implementation

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 시작 시 호출
    /// Poisoned, Burnt 등 DoT 효과 처리
    /// </summary>
    public void OnTurnStart()
    {
        ProcessAllDurableEffectsOnTurnStart();
    }

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 종료 시 호출
    /// BattleTurnController에서 자동으로 호출됨
    /// </summary>
    public void OnTurnEnd()
    {
        ProcessAllDurableEffects();
    }

    #endregion

    /// <summary>
    /// Duration 효과 등록 (Priority 기반)
    /// </summary>
    public void RegisterEffect(IDurableEffect effect)
    {
        if (effect == null || effect.Owner == null)
        {
            Debug.LogWarning("[WARN]DurableEffectRegistry::RegisterEffect() - Effect or Owner is null");
            return;
        }

        if (!_characterEffects.TryGetValue(effect.Owner, out var queue))
        {
            queue = new PriorityEventQueue<IDurableEffect>();
            _characterEffects[effect.Owner] = queue;
        }

        queue.Register(effect, effect.Priority);
        Debug.Log($"[INFO]DurableEffectRegistry::RegisterEffect() - Registered {effect.EffectId} (Priority: {effect.Priority}) for {effect.Owner.CharacterName}");
    }

    /// <summary>
    /// Duration 효과 해제
    /// </summary>
    public void UnregisterEffect(IDurableEffect effect)
    {
        if (effect == null || effect.Owner == null)
        {
            return;
        }

        if (_characterEffects.TryGetValue(effect.Owner, out var queue))
        {
            queue.Unregister(effect);
            Debug.Log($"[INFO]DurableEffectRegistry::UnregisterEffect() - Unregistered {effect.EffectId} for {effect.Owner.CharacterName}");
        }
    }

    /// <summary>
    /// 모든 Duration 효과 처리 (턴 시작 시)
    /// Priority 순서대로 실행
    /// </summary>
    private void ProcessAllDurableEffectsOnTurnStart()
    {
        int totalEffects = _characterEffects.Values.Sum(queue => queue.Count);
        Debug.Log($"[INFO] DurableEffectRegistry::ProcessAllDurableEffectsOnTurnStart() - Processing {totalEffects} effects for {_characterEffects.Count} characters");

        foreach (var queue in _characterEffects.Values)
        {
            // PriorityEventQueue는 자동으로 Priority 순서대로 반환
            foreach (var effect in queue)
            {
                if (effect.IsActive)
                {
                    Debug.Log($"  → Executing: {effect.EffectId} (Priority: {effect.Priority})");
                    effect.ProcessTurnStart();
                }
            }
        }
    }

    /// <summary>
    /// 모든 Duration 효과 처리 (턴 종료 시)
    /// Priority 순서대로 실행
    /// </summary>
    private void ProcessAllDurableEffects()
    {
        int totalEffects = _characterEffects.Values.Sum(queue => queue.Count);
        Debug.Log($"[INFO] DurableEffectRegistry::ProcessAllDurableEffects() - Processing {totalEffects} effects for {_characterEffects.Count} characters");

        foreach (var queue in _characterEffects.Values)
        {
            // PriorityEventQueue는 자동으로 Priority 순서대로 반환
            foreach (var effect in queue)
            {
                if (effect.IsActive)
                {
                    Debug.Log($"  → Executing: {effect.EffectId} (Priority: {effect.Priority})");
                    effect.ProcessTurnEnd();
                }
            }
        }
    }

    /// <summary>
    /// 특정 캐릭터의 모든 효과 제거
    /// </summary>
    public void ClearCharacterEffects(BaseCharacter character)
    {
        if (_characterEffects.TryGetValue(character, out var queue))
        {
            foreach (var effect in queue)
            {
                effect.OnExpire();
            }
            queue.Clear();
            _characterEffects.Remove(character);
            Debug.Log($"[INFO]DurableEffectRegistry::ClearCharacterEffects() - Cleared all effects for {character.CharacterName}");
        }
    }

    /// <summary>
    /// 디버그용 - 현재 등록된 모든 효과 출력 (Priority 순서)
    /// </summary>
    public void DebugPrintAllEffects()
    {
        Debug.Log($"[DEBUG]DurableEffectRegistry - Total characters: {_characterEffects.Count}");
        foreach (var kvp in _characterEffects)
        {
            Debug.Log($"[DEBUG] {kvp.Key.CharacterName}: {kvp.Value.Count} effects");
            foreach (var effect in kvp.Value)
            {
                Debug.Log($"[DEBUG]   - {effect.EffectId} (Priority: {effect.Priority}, Duration: {effect.Duration})");
            }
        }
    }
}
