using System.Collections.Generic;
using UnityEngine;
using Battle;

/// <summary>
/// 셀 상태 변경 Effect
/// SkillInstance를 통해 스킬 영역에 CellState 추가
/// Params:
///   - "CellState": CellState enum (Poison, SmokeBomb, Slime, Fire 등)
/// </summary>
public class CellStateChangeEffect : IBaseEffectLogic
{
    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        if (!(effectInstance is SkillInstance skillInstance))
        {
            Debug.LogWarning("[WARN] CellStateChangeEffect::EffectByEffectEvent() - effectInstance is not SkillInstance");
            return;
        }

        // Start 이벤트에서만 셀 상태 생성
        if (effectEvent != EffectEvent.Start)
            return;

        // Params에서 CellState 추출
        CellState cellState = effectData.Get<CellState>("CellState", CellState.None);
        if (cellState == CellState.None)
        {
            Debug.LogWarning("[WARN] CellStateChangeEffect::EffectByEffectEvent() - CellState is None, skipping");
            return;
        }

        // BattleSystem의 CellManager에 접근하여 셀 상태 추가
        BattleSystem battleSystem = GameManager.Instance.BattleSystem;
        if (battleSystem == null || battleSystem.CellManager == null)
        {
            Debug.LogError("[ERROR] CellStateChangeEffect::EffectByEffectEvent() - BattleSystem or CellManager is null");
            return;
        }

        // SkillInstance의 Duration 사용 (SkillData에서 정의)
        int duration = skillInstance.Duration;

        // SkillInstance의 Area 모든 셀에 상태 추가
        foreach (Vector2Int position in skillInstance.Area)
        {
            battleSystem.CellManager.SetCellState(position, cellState, duration);
            Debug.Log($"[INFO] CellStateChangeEffect::EffectByEffectEvent() - Added CellState: {cellState} at Position: {position} with Duration: {duration}");
        }
    }

    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        // 셀 상태 변경은 게임 이벤트로 처리하지 않음
    }
}
