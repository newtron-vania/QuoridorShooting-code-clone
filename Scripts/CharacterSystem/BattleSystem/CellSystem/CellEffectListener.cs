using UnityEngine;
using HM;
using Battle;
using EventType = HM.EventType;

/// <summary>
/// 셀 이벤트를 수신하여 셀 상태별 효과를 적용하는 리스너
/// EventManager에 등록되어 OnCellEnter/Stay/Exit 이벤트 처리
/// </summary>
public class CellEffectListener : IEventListener
{
    private BattleSystem _battleSystem;

    public CellEffectListener(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;

        // EventManager에 셀 이벤트 등록
        EventManager.Instance.AddEvent(EventType.OnCellEnter, this);
        EventManager.Instance.AddEvent(EventType.OnCellStay, this);
        EventManager.Instance.AddEvent(EventType.OnCellExit, this);
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        if (!(param is CellManager.CellEventData cellEventData))
        {
            Debug.LogWarning("[WARN] CellEffectListener::OnEvent() - param is not CellEventData");
            return;
        }

        switch (eventType)
        {
            case EventType.OnCellEnter:
                OnCellEnter(cellEventData);
                break;
            case EventType.OnCellStay:
                OnCellStay(cellEventData);
                break;
            case EventType.OnCellExit:
                OnCellExit(cellEventData);
                break;
        }
    }

    /// <summary>
    /// 셀 진입 시 효과 처리
    /// </summary>
    private void OnCellEnter(CellManager.CellEventData data)
    {
        foreach (var state in data.Cell.GetStates())
        {
            switch (state)
            {
                case CellState.Slime:
                    // 미끈젤리: 진입 방향으로 추가 이동
                    ApplySlimeEffect(data.Character);
                    break;

                case CellState.Poison:
                case CellState.SmokeBomb:
                case CellState.Fire:
                    // 진입 시에는 효과 없음 (Stay에서 처리)
                    break;
            }
        }
    }

    /// <summary>
    /// 셀에 머무를 때 효과 처리 (턴 시작 등)
    /// 각 셀 상태에 해당하는 StatusEffect를 부여
    /// </summary>
    private void OnCellStay(CellManager.CellEventData data)
    {
        foreach (var state in data.Cell.GetStates())
        {
            switch (state)
            {
                case CellState.Poison:
                    // 독 지대: 독 상태이상 부여
                    ApplyPoisonStatusEffect(data.Character);
                    break;

                case CellState.SmokeBomb:
                    // 연막 지대: 침묵 상태이상 부여
                    ApplySmokeBombStatusEffect(data.Character);
                    break;

                case CellState.Fire:
                    // 화염 지대: 화상 상태이상 부여
                    ApplyFireStatusEffect(data.Character);
                    break;

                case CellState.Slime:
                    // 미끈젤리: Stay 시에는 효과 없음
                    break;
            }
        }
    }

    /// <summary>
    /// 셀 퇴출 시 효과 처리
    /// </summary>
    private void OnCellExit(CellManager.CellEventData data)
    {
        foreach (var state in data.Cell.GetStates())
        {
            switch (state)
            {
                case CellState.SmokeBomb:
                    // 연막 지대 벗어남 (특별한 처리 없음, CanAttack이 자동으로 복구됨)
                    Debug.Log($"[INFO] CellEffectListener::OnCellExit() - {data.Character.CharacterName} exited SmokeBomb area");
                    break;

                case CellState.Poison:
                case CellState.Slime:
                case CellState.Fire:
                    // 퇴출 시 특별한 효과 없음
                    break;
            }
        }
    }

    #region 셀 상태이상 부여

    /// <summary>
    /// 독 지대 상태이상 부여
    /// StatuseffectId: 독 지대에서 부여되는 독 상태
    /// </summary>
    private void ApplyPoisonStatusEffect(BaseCharacter character)
    {
        // TODO: StatuseffectData.json에 CellPoisonStatusEffect 정의 필요
        // 임시로 StatuseffectId 가정 (실제 구현 시 JSON에서 정의된 ID 사용)
        int poisonStatusEffectId = 100; // 독 지대 상태이상 ID (가정)

        // StatManager를 통해 상태이상 생성
        var statusEffectInstance = StatManager.Instance.CreateStatuseffectInstance(poisonStatusEffectId, character, character);

        if (statusEffectInstance != null)
        {
            // StatuseffectInstance.InvokeInstanceEvent(Start) 호출
            // 이 메서드가 내부에서 StatuseffectController.AddStatuseffect()를 호출함
            statusEffectInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
            Debug.Log($"[INFO] CellEffectListener::ApplyPoisonStatusEffect() - Applied poison status to {character.CharacterName}");
        }
        else
        {
            Debug.LogWarning($"[WARN] CellEffectListener::ApplyPoisonStatusEffect() - Failed to create poison status effect (Id: {poisonStatusEffectId})");
        }
    }

    /// <summary>
    /// 연막 지대 상태이상 부여
    /// StatuseffectId: 연막 지대에서 부여되는 침묵 상태
    /// </summary>
    private void ApplySmokeBombStatusEffect(BaseCharacter character)
    {
        // TODO: StatuseffectData.json에 CellSmokeBombStatusEffect 정의 필요
        int smokeBombStatusEffectId = 101; // 연막 지대 상태이상 ID (가정)

        var statusEffectInstance = StatManager.Instance.CreateStatuseffectInstance(smokeBombStatusEffectId, character, character);

        if (statusEffectInstance != null)
        {
            statusEffectInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
            Debug.Log($"[INFO] CellEffectListener::ApplySmokeBombStatusEffect() - Applied smoke status to {character.CharacterName}");
        }
        else
        {
            Debug.LogWarning($"[WARN] CellEffectListener::ApplySmokeBombStatusEffect() - Failed to create smoke status effect (Id: {smokeBombStatusEffectId})");
        }
    }

    /// <summary>
    /// 화염 지대 상태이상 부여
    /// StatuseffectId: 화염 지대에서 부여되는 화상 상태
    /// </summary>
    private void ApplyFireStatusEffect(BaseCharacter character)
    {
        // TODO: StatuseffectData.json에 CellFireStatusEffect 정의 필요
        int fireStatusEffectId = 102; // 화염 지대 상태이상 ID (가정)

        var statusEffectInstance = StatManager.Instance.CreateStatuseffectInstance(fireStatusEffectId, character, character);

        if (statusEffectInstance != null)
        {
            statusEffectInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
            Debug.Log($"[INFO] CellEffectListener::ApplyFireStatusEffect() - Applied fire status to {character.CharacterName}");
        }
        else
        {
            Debug.LogWarning($"[WARN] CellEffectListener::ApplyFireStatusEffect() - Failed to create fire status effect (Id: {fireStatusEffectId})");
        }
    }

    /// <summary>
    /// 미끈젤리 효과: 이동 방향으로 추가 1칸 미끄러짐
    /// OnCellEnter에서 호출됨 (상태이상이 아닌 즉시 효과)
    /// </summary>
    private void ApplySlimeEffect(BaseCharacter character)
    {
        // PrevPosition에서 현재 Position으로의 방향 계산
        Vector2Int direction = character.Position - character.PrevPosition;

        if (direction == Vector2Int.zero)
        {
            Debug.Log($"[INFO] CellEffectListener::ApplySlimeEffect() - {character.CharacterName} has no movement direction, skipping");
            return;
        }

        // SlipForward 호출 (1칸 추가 이동)
        int slipDistance = 1;
        character.SlipForward(character.PrevPosition, slipDistance);
        Debug.Log($"[INFO] CellEffectListener::ApplySlimeEffect() - {character.CharacterName} slips forward by {slipDistance}");
    }

    #endregion

    /// <summary>
    /// 이벤트 리스너 해제
    /// </summary>
    public void Dispose()
    {
        EventManager.Instance.RemoveEvent(EventType.OnCellEnter, this);
        EventManager.Instance.RemoveEvent(EventType.OnCellStay, this);
        EventManager.Instance.RemoveEvent(EventType.OnCellExit, this);
    }
}
