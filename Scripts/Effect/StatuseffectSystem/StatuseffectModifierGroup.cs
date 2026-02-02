using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 상태이상 효과의 통합 관리 클래스
/// 스탯 변경, 행동 제한, 지속 피해, 콜백 효과를 하나의 객체로 관리
/// </summary>
public class StatuseffectModifierGroup
{
    public string GroupId { get; private set; }
    public StatuseffectInstance.Type EffectType { get; private set; }

    // 1. 스탯 변경 (StatManager 통합)
    public List<StatModifier> StatModifiers { get; private set; }

    // 2. 행동 제한 (플래그 기반)
    public ActionRestrictionFlags ActionRestriction { get; private set; }

    // 3. 지속 피해 (DoT - Damage over Time)
    public int? DamagePerTurn { get; private set; }

    // 4. 공격 시 효과 콜백
    public Action<BaseCharacter, IEffectableProvider> OnAttackCallback { get; private set; }

    // 5. 턴 시작 시 효과 콜백
    public Action<BaseCharacter> OnTurnStartCallback { get; private set; }

    // 6. 효과 적용/제거 시 콜백
    public Action<BaseCharacter> OnApplyCallback { get; private set; }
    public Action<BaseCharacter> OnRemoveCallback { get; private set; }

    // 7. 특수 효과용 파라미터 저장소
    public Dictionary<string, object> Parameters { get; private set; }

    public StatuseffectModifierGroup(string groupId, StatuseffectInstance.Type effectType)
    {
        GroupId = groupId;
        EffectType = effectType;
        StatModifiers = new List<StatModifier>();
        ActionRestriction = ActionRestrictionFlags.None;
        Parameters = new Dictionary<string, object>();
    }

    #region Builder Methods

    public void AddStatModifier(StatModifier modifier)
    {
        StatModifiers.Add(modifier);
    }

    public void SetRestrictions(
        bool? canAttack = null,
        bool? canMove = null,
        bool? canUseSkill = null,
        bool? canBuild = null)
    {
        ActionRestriction = ActionRestrictionFlags.None;

        if (canAttack == false) ActionRestriction |= ActionRestrictionFlags.CannotAttack;
        if (canMove == false) ActionRestriction |= ActionRestrictionFlags.CannotMove;
        if (canUseSkill == false) ActionRestriction |= ActionRestrictionFlags.CannotUseSkill;
        if (canBuild == false) ActionRestriction |= ActionRestrictionFlags.CannotBuild;
    }

    public void SetDamagePerTurn(int damage)
    {
        DamagePerTurn = damage;
    }

    public void SetOnAttackCallback(Action<BaseCharacter, IEffectableProvider> callback)
    {
        OnAttackCallback = callback;
    }

    public void SetOnTurnStartCallback(Action<BaseCharacter> callback)
    {
        OnTurnStartCallback = callback;
    }

    public void SetCallbacks(Action<BaseCharacter> onApply = null, Action<BaseCharacter> onRemove = null)
    {
        OnApplyCallback = onApply;
        OnRemoveCallback = onRemove;
    }

    public void AddParameter(string key, object value)
    {
        Parameters[key] = value;
    }

    #endregion

    #region Application Methods

    /// <summary>
    /// 모든 효과를 캐릭터에 적용
    /// </summary>
    public void ApplyAllEffects(IEffectableProvider target, StatManager statManager)
    {
        var character = target.GetEffectable<BaseCharacter>();
        if (character == null)
        {
            Debug.LogWarning($"[WARN] StatuseffectGroupModifier::ApplyAllEffects - Target is not a BaseCharacter");
            return;
        }

        // 1. 스탯 변경 적용 (StatManager 통합)
        foreach (var modifier in StatModifiers)
        {
            character.characterStat.AddModifier(modifier);
        }

        // 2. 행동 제한 적용
        if (ActionRestriction != ActionRestrictionFlags.None)
        {
            var statuseffectParticipant = target.GetEffectable<IStatuseffectParticipant>();
            statuseffectParticipant?.StatuseffectController.ApplyRestriction(ActionRestriction);
        }

        // 3. OnApplyCallback 실행
        OnApplyCallback?.Invoke(character);

        Debug.Log($"[INFO] StatuseffectGroupModifier::ApplyAllEffects - Applied {EffectType} to {character.CharacterName}");
    }

    /// <summary>
    /// 모든 효과를 캐릭터에서 제거
    /// </summary>
    public void RemoveAllEffects(IEffectableProvider target, StatManager statManager)
    {
        var character = target.GetEffectable<BaseCharacter>();
        if (character == null)
        {
            Debug.LogWarning($"[WARN] StatuseffectGroupModifier::RemoveAllEffects - Target is not a BaseCharacter");
            return;
        }

        // 1. 스탯 변경 제거
        foreach (var modifier in StatModifiers)
        {
            character.characterStat.RemoveModifier(modifier.Id);
        }

        // 2. 행동 제한 제거
        if (ActionRestriction != ActionRestrictionFlags.None)
        {
            var statuseffectParticipant = target.GetEffectable<IStatuseffectParticipant>();
            statuseffectParticipant?.StatuseffectController.RemoveRestriction(ActionRestriction);
        }

        // 3. OnRemoveCallback 실행
        OnRemoveCallback?.Invoke(character);

        Debug.Log($"[INFO] StatuseffectGroupModifier::RemoveAllEffects - Removed {EffectType} from {character.CharacterName}");
    }

    /// <summary>
    /// 지속 피해 처리 (OnTurnStart/OnTurnEnd에서 호출)
    /// </summary>
    public void ProcessDamagePerTurn(BaseCharacter character)
    {
        if (DamagePerTurn.HasValue && DamagePerTurn.Value > 0)
        {
            // Source는 StatuseffectInstance가 가지고 있으므로, 여기서는 null로 처리
            // 실제 호출 시 StatuseffectInstance.Source를 전달해야 함
            Debug.Log($"[INFO] StatuseffectGroupModifier::ProcessDamagePerTurn - {character.CharacterName} takes {DamagePerTurn.Value} damage");
        }
    }

    /// <summary>
    /// 공격 시 효과 발동
    /// </summary>
    public void TriggerOnAttack(BaseCharacter attacker, IEffectableProvider target)
    {
        OnAttackCallback?.Invoke(attacker, target);
    }

    /// <summary>
    /// 턴 시작 시 효과 발동
    /// </summary>
    public void TriggerOnTurnStart(BaseCharacter character)
    {
        OnTurnStartCallback?.Invoke(character);
    }

    #endregion
}
