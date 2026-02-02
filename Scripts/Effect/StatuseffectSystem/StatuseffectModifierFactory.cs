using System;
using UnityEngine;

/// <summary>
/// StatuseffectGroupModifier 생성 및 구성을 담당하는 Factory 클래스
/// Excel "6. Effect Type" 데이터 (ID 1-15) 기반 구현
/// </summary>
public static class StatuseffectModifierFactory
{
    /// <summary>
    /// StatuseffectType에 따라 적절한 GroupModifier 생성
    /// </summary>
    public static StatuseffectModifierGroup CreateModifier(
        string groupId,
        StatuseffectInstance.Type effectType,
        StatuseffectInstance instance)
    {
        var modifier = new StatuseffectModifierGroup(groupId, effectType);

        switch (effectType)
        {
            case StatuseffectInstance.Type.Blinded:
                ConfigureBlinded(modifier, instance);
                break;

            case StatuseffectInstance.Type.Holded:
                ConfigureHolded(modifier, instance);
                break;

            case StatuseffectInstance.Type.Provocation:
                ConfigureProvocation(modifier, instance);
                break;

            case StatuseffectInstance.Type.Exhausted:
                ConfigureExhausted(modifier, instance);
                break;

            case StatuseffectInstance.Type.Accelerate:
                ConfigureAccelerate(modifier, instance);
                break;

            case StatuseffectInstance.Type.Clear:
                // Clear는 스킬로 구현되므로 GroupModifier 불필요
                Debug.LogWarning($"[WARN] StatuseffectModifierFactory - Clear is not a statuseffect, should be skill");
                break;

            case StatuseffectInstance.Type.Toxic:
                ConfigureToxic(modifier, instance);
                break;

            case StatuseffectInstance.Type.Poisoned:
                ConfigurePoisoned(modifier, instance);
                break;

            case StatuseffectInstance.Type.Ignite:
                ConfigureIgnite(modifier, instance);
                break;

            case StatuseffectInstance.Type.Burnt:
                ConfigureBurnt(modifier, instance);
                break;

            case StatuseffectInstance.Type.Weaken:
                ConfigureWeaken(modifier, instance);
                break;

            case StatuseffectInstance.Type.Glacial:
                ConfigureGlacial(modifier, instance);
                break;

            case StatuseffectInstance.Type.Frozen:
                ConfigureFrozen(modifier, instance);
                break;

            case StatuseffectInstance.Type.Lifedrain:
                ConfigureLifedrain(modifier, instance);
                break;

            case StatuseffectInstance.Type.Unresistable:
                ConfigureUnresistable(modifier, instance);
                break;

            case StatuseffectInstance.Type.PowerUp:
                ConfigurePowerUp(modifier, instance);
                break;

            case StatuseffectInstance.Type.Resistable:
                ConfigureResistable(modifier, instance);
                break;

            case StatuseffectInstance.Type.TimeDistortion:
                ConfigureTimeDistortion(modifier, instance);
                break;

            case StatuseffectInstance.Type.Rotten:
                ConfigureRotten(modifier, instance);
                break;

            default:
                Debug.LogWarning($"[WARN] StatuseffectModifierFactory - No configuration for {effectType}");
                break;
        }

        return modifier;
    }

    /// <summary>
    /// StatuseffectInstance 생성 헬퍼 메서드 (Factory 패턴 통합)
    /// </summary>
    public static StatuseffectInstance CreateInstance(
        int statuseffectId,
        BaseCharacter source,
        IEffectableProvider target,
        EffectData instanceData = null)
    {
        var statuseffectData = DataManager.Instance.GetStatuseffectData(statuseffectId);
        var instance = new StatuseffectInstance(
            StatManager.Instance,
            statuseffectData,
            source,
            target,
            instanceData
        );

        // GroupModifier 자동 생성 및 할당 (Phase 2에서 통합)
        // instance.GroupModifier = CreateModifier(instance.EffectId, instance.StatuseffectType, instance);

        return instance;
    }

    #region Configure Methods (ID 1-15)

    /// <summary>
    /// ID 1: Blinded (실명) - 일반 공격 불가
    /// </summary>
    private static void ConfigureBlinded(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetRestrictions(canAttack: false);
        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureBlinded - Attack disabled");
    }

    /// <summary>
    /// ID 2: Holded (속박) - 이동 불가
    /// </summary>
    private static void ConfigureHolded(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetRestrictions(canMove: false);
        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureHolded - Move disabled");
    }

    /// <summary>
    /// ID 3: Provocation (도발) - AI 타겟 강제 지정
    /// </summary>
    private static void ConfigureProvocation(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        BaseCharacter provoker = instance.Source;
        modifier.AddParameter("ProvokerCharacter", provoker);

        modifier.SetCallbacks(
            onApply: (character) =>
            {
                // AI에게 도발자를 우선 타겟으로 설정
                if (character is EnemyCharacter enemy)
                {
                    enemy.ForcedTarget = provoker;
                    Debug.Log($"[INFO] Provocation - {character.CharacterName} is provoked by {provoker.CharacterName}");
                }
            },
            onRemove: (character) =>
            {
                if (character is EnemyCharacter enemy)
                {
                    enemy.ForcedTarget = null;
                    Debug.Log($"[INFO] Provocation - {character.CharacterName} provocation removed");
                }
            }
        );
    }

    /// <summary>
    /// ID 4: Exhausted (탈진) - AP 회복 불가
    /// </summary>
    private static void ConfigureExhausted(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        var target = instance.Target.GetEffectable<BaseCharacter>();
        int baseApRecovery = target.characterStat.ApRecovery;

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_ApRecovery",
            targetStat: StatType.ApRecovery,
            modifierType: StatModifierType.FlatBaseAdd,
            value: -baseApRecovery, // ApRecovery를 0으로 만듦
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureExhausted - AP recovery disabled");
    }

    /// <summary>
    /// ID 5: Accelerate (가속) - 이동 횟수 +1
    /// </summary>
    private static void ConfigureAccelerate(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        // TODO: StatType에 AdditionalMoveActionCount 추가 시 StatModifier로 변경
        // 현재는 StatuseffectController.AdditionalMoveActionCount 직접 조작
        modifier.SetCallbacks(
            onApply: (character) =>
            {
                var statuseffectParticipant = instance.Target.GetEffectable<IStatuseffectParticipant>();
                statuseffectParticipant.StatuseffectController.AdditionalMoveActionCount += 1;
                Debug.Log($"[INFO] Accelerate - {character.CharacterName} +1 move action");
            },
            onRemove: (character) =>
            {
                var statuseffectParticipant = instance.Target.GetEffectable<IStatuseffectParticipant>();
                statuseffectParticipant.StatuseffectController.AdditionalMoveActionCount -= 1;
                Debug.Log($"[INFO] Accelerate - {character.CharacterName} move action restored");
            }
        );
    }

    /// <summary>
    /// ID 7: Toxic (맹독) - 공격 시 중독 부여
    /// </summary>
    private static void ConfigureToxic(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetOnAttackCallback((attacker, target) =>
        {
            // 중독 상태이상 부여 (ID 8: Poisoned)
            int poisonedId = instance.InstanceData.Get<int>("StatuseffectId");
            var poisonedInstance = CreateInstance(poisonedId, attacker, target, instance.InstanceData);
            poisonedInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);

            Debug.Log($"[INFO] Toxic - {attacker.CharacterName} applies Poisoned to target");
        });
    }

    /// <summary>
    /// ID 8: Poisoned (중독) - 턴 시작 시 N 피해
    /// </summary>
    private static void ConfigurePoisoned(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        int damageValue = instance.InstanceData.Get<int>("DamageValue");
        modifier.SetDamagePerTurn(damageValue);

        modifier.SetOnTurnStartCallback((character) =>
        {
            character.TakeDamage(instance.Source, damageValue);
            Debug.Log($"[INFO] Poisoned - {character.CharacterName} takes {damageValue} poison damage");
        });
    }

    /// <summary>
    /// ID 9: Ignite (점화) - 공격 시 화상 부여
    /// </summary>
    private static void ConfigureIgnite(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetOnAttackCallback((attacker, target) =>
        {
            // 화상 상태이상 부여 (ID 10: Burnt)
            int burntId = instance.InstanceData.Get<int>("StatuseffectId");
            var burntInstance = CreateInstance(burntId, attacker, target, instance.InstanceData);
            burntInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);

            Debug.Log($"[INFO] Ignite - {attacker.CharacterName} applies Burnt to target");
        });
    }

    /// <summary>
    /// ID 10: Burnt (화상) - 턴 시작 시 N 피해
    /// </summary>
    private static void ConfigureBurnt(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        int damageValue = instance.InstanceData.Get<int>("DamageValue");
        modifier.SetDamagePerTurn(damageValue);

        modifier.SetOnTurnStartCallback((character) =>
        {
            character.TakeDamage(instance.Source, damageValue);
            Debug.Log($"[INFO] Burnt - {character.CharacterName} takes {damageValue} burn damage");
        });
    }

    /// <summary>
    /// ID 11: Weaken (쇠약) - 공격력 감소
    /// </summary>
    private static void ConfigureWeaken(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        int weakenValue = instance.InstanceData.Get<int>("StatValue");

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_Atk",
            targetStat: StatType.Atk,
            modifierType: StatModifierType.FlatBaseAdd,
            value: -weakenValue,
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureWeaken - Atk -{weakenValue}");
    }

    /// <summary>
    /// ID 12: Glacial (빙하) - 공격 시 빙결 부여
    /// </summary>
    private static void ConfigureGlacial(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetOnAttackCallback((attacker, target) =>
        {
            // 빙결 상태이상 부여 (ID 13: Frozen)
            int frozenId = instance.InstanceData.Get<int>("StatuseffectId");
            var frozenInstance = CreateInstance(frozenId, attacker, target, instance.InstanceData);
            frozenInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);

            Debug.Log($"[INFO] Glacial - {attacker.CharacterName} applies Frozen to target");
        });
    }

    /// <summary>
    /// ID 13: Frozen (빙결) - 이동 불가 + 지속 피해 (복합 효과)
    /// </summary>
    private static void ConfigureFrozen(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        // 속박 효과
        modifier.SetRestrictions(canMove: false);

        // 지속 피해
        int damageValue = instance.InstanceData.Get<int>("DamageValue");
        modifier.SetDamagePerTurn(damageValue);

        modifier.SetOnTurnStartCallback((character) =>
        {
            character.TakeDamage(instance.Source, damageValue);
            Debug.Log($"[INFO] Frozen - {character.CharacterName} is frozen and takes {damageValue} damage");
        });
    }

    /// <summary>
    /// ID 14: Lifedrain (흡혈) - 공격 시 피해의 50% 회복
    /// </summary>
    private static void ConfigureLifedrain(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        modifier.SetOnAttackCallback((attacker, target) =>
        {
            // 마지막 공격 피해량 조회
            int lastDamage = attacker.LastDamageDealt;
            int healAmount = Mathf.FloorToInt(lastDamage * 0.5f);

            // 회복 (최대 체력 초과 불가)
            int currentHp = attacker.Hp;
            int maxHp = attacker.MaxHp;

            if (currentHp + healAmount <= maxHp)
            {
                attacker.characterStat.Hp += healAmount;
                Debug.Log($"[INFO] Lifedrain - {attacker.CharacterName} heals {healAmount} HP");
            }
            else
            {
                int actualHeal = maxHp - currentHp;
                attacker.characterStat.Hp = maxHp;
                Debug.Log($"[INFO] Lifedrain - {attacker.CharacterName} heals {actualHeal} HP (capped at max)");
            }
        });
    }

    /// <summary>
    /// ID 15: Unresistable (저항불가) - 피해저항을 0으로 설정
    /// </summary>
    private static void ConfigureUnresistable(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        var target = instance.Target.GetEffectable<BaseCharacter>();
        float baseAvd = target.characterStat.Avd;

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_Avd",
            targetStat: StatType.Avd,
            modifierType: StatModifierType.FlatBaseAdd,
            value: -baseAvd, // Avd를 0으로 만듦
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureUnresistable - Avd set to 0");
    }

    /// <summary>
    /// ID 16: PowerUp (공격력 강화) - 공격력 증가
    /// </summary>
    private static void ConfigurePowerUp(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        int atkBonus = instance.InstanceData.Get<int>("StatValue");

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_Atk",
            targetStat: StatType.Atk,
            modifierType: StatModifierType.FlatBaseAdd,
            value: atkBonus,
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigurePowerUp - Atk +{atkBonus}");
    }

    /// <summary>
    /// ID 17: Resistable (피해저항 강화) - 피해저항 % 증가
    /// </summary>
    private static void ConfigureResistable(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        float avdBonus = instance.InstanceData.Get<float>("StatValue");

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_Avd",
            targetStat: StatType.Avd,
            modifierType: StatModifierType.FlatBaseAdd,
            value: avdBonus,
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureResistable - Avd +{avdBonus}");
    }

    /// <summary>
    /// ID 18: TimeDistortion (시간 왜곡) - AP 회복 감소
    /// 캐릭터 상태이상으로 재설계: SkillStateType 제거됨
    /// </summary>
    private static void ConfigureTimeDistortion(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        var target = instance.Target.GetEffectable<BaseCharacter>();
        int baseApRecovery = target.characterStat.ApRecovery;

        // AP 회복을 50% 감소
        int apReduction = Mathf.FloorToInt(baseApRecovery * 0.5f);

        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_ApRecovery",
            targetStat: StatType.ApRecovery,
            modifierType: StatModifierType.FlatBaseAdd,
            value: -apReduction,
            priority: 100
        ));

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureTimeDistortion - ApRecovery -{apReduction} (50% reduction)");
    }

    /// <summary>
    /// ID 19: Rotten (부패) - 복합 디버프 (AP 회복 불가 + 지속 피해)
    /// 캐릭터 상태이상으로 재설계: SkillStateType 제거됨
    /// Exhausted + Poisoned 효과 결합
    /// </summary>
    private static void ConfigureRotten(StatuseffectModifierGroup modifier, StatuseffectInstance instance)
    {
        var target = instance.Target.GetEffectable<BaseCharacter>();
        int baseApRecovery = target.characterStat.ApRecovery;
        int damageValue = instance.InstanceData.Get<int>("DamageValue");

        // 1. Exhausted 효과: AP 회복 완전 차단
        modifier.AddStatModifier(new StatModifier(
            id: $"{modifier.GroupId}_ApRecovery",
            targetStat: StatType.ApRecovery,
            modifierType: StatModifierType.FlatBaseAdd,
            value: -baseApRecovery,
            priority: 100
        ));

        // 2. Poisoned 효과: 지속 피해
        modifier.SetDamagePerTurn(damageValue);

        modifier.SetOnTurnStartCallback((character) =>
        {
            character.TakeDamage(instance.Source, damageValue);
            Debug.Log($"[INFO] Rotten - {character.CharacterName} takes {damageValue} damage (corrupted)");
        });

        Debug.Log($"[INFO] StatuseffectModifierFactory::ConfigureRotten - AP recovery blocked + {damageValue} damage per turn");
    }

    #endregion
}
