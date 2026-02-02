using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterDefinition;

public class CharacterStat
{
    public BaseStat BaseStat { get; private set; }
    private DynamicStat DynamicStat;
    private StatModifierController _modifierController;
    private Dictionary<string, ModifierGroup> _modifierGroups;

    public CharacterStat(BaseStat baseStats, IStatRemovalStrategy removalStrategy = null)
    {
        BaseStat = baseStats;
        DynamicStat = new DynamicStat(baseStats);
        _modifierController = new StatModifierController(baseStats, DynamicStat, removalStrategy);
        _modifierGroups = new Dictionary<string, ModifierGroup>();
    }

    public CharacterStat()
    {
    }

    public override string ToString()
    {
        return $"Base Stats: {BaseStat}\nDynamic Stats: {DynamicStat}";
    }

    // 개별 정보 반환 프로퍼티
    public int Index => DynamicStat.Index;
    public string Name => DynamicStat.Name;

    public int Hp
    {
        get => DynamicStat.Hp;
        set
        {
            DynamicStat.SetHp(value);

            if (Hp < 0) Hp = 0; // HP는 0 이상으로 설정
            if (Hp > MaxHp) Hp = MaxHp; // HP는 MaxHp 이하로 설정
        }
    }

    public int MaxHp
    {
        get => DynamicStat.MaxHp;
        set => DynamicStat.SetMaxHp(value);
    }

    public int Atk
    {
        get => DynamicStat.Atk;
        set => DynamicStat.SetAtk(value);
    }

    public float Avd
    {
        get => DynamicStat.Avd;
        set => DynamicStat.SetAvd(value);
    }

    public int Ap
    {
        get => DynamicStat.BaseAp;
        set => DynamicStat.SetBaseAp(value);
    }

    public int ApRecovery
    {
        get => DynamicStat.ApRecovery;
        set => DynamicStat.SetApRecovery(value);
    }

    public CharacterType Type
    {
        get => DynamicStat.Type;
        set => DynamicStat.SetType(value);
    }

    public CharacterClass Class
    {
        get => DynamicStat.Class;
        set => DynamicStat.SetClass(value);
    }

    public int SkillId
    {
        get => DynamicStat.SkillId;
        set => DynamicStat.SetSkillId(value);
    }

    public List<Vector2Int> MovablePositions
    {
        get => DynamicStat.MovablePositions;
        set => DynamicStat.SetMovablePositions(value);
    }

    public List<Vector2Int> AttackablePositions
    {
        get => DynamicStat.AttackablePositions;
        set => DynamicStat.SetAttackablePositions(value);
    }

    public BaseStat GetInitializedStat()
    {
        return BaseStat;
    }

    // 현재 상태 반환 프로퍼티
    public string CurrentState =>
        $"Index: {Index}, Name: {Name}, " +
        $"HP: {Hp}/{MaxHp}, " +
        $"Attack: {Atk}, " +
        $"Avoidance: {Avd}, " +
        $"Base AP: {Ap}, " +
        $"AP Recovery: {ApRecovery}";

    public void AddModifier(StatModifier modifier)
    {
        _modifierController.AddModifier(modifier);
    }

    public bool RemoveModifier(string modifierId)
    {
        return _modifierController.RemoveModifier(modifierId);
    }

    public void ClearModifiers(StatType statType)
    {
        _modifierController.ClearModifiers(statType);
    }

    public void ClearAllModifiers()
    {
        _modifierController.ClearAllModifiers();
    }

    public bool HasModifier(string modifierId)
    {
        return _modifierController.HasModifier(modifierId);
    }

    // ModifierGroup 관리
    public void RegisterModifierGroup(ModifierGroup group)
    {
        if (_modifierGroups.ContainsKey(group.GroupId))
        {
            throw new ArgumentException($"ModifierGroup with ID '{group.GroupId}' already registered.");
        }

        _modifierGroups[group.GroupId] = group;
    }

    public bool UnregisterModifierGroup(string groupId)
    {
        if (_modifierGroups.TryGetValue(groupId, out var group))
        {
            group.Remove(this);
            _modifierGroups.Remove(groupId);
            return true;
        }

        return false;
    }

    public void TryApplyModifierGroup(string groupId)
    {
        if (_modifierGroups.TryGetValue(groupId, out var group))
        {
            if (group.CanApply(this))
            {
                group.Apply(this);
            }
        }
    }

    public void TryApplyAllModifierGroups()
    {
        foreach (var group in _modifierGroups.Values)
        {
            if (group.CanApply(this))
            {
                group.Apply(this);
            }
        }
    }

    public bool HasModifierGroup(string groupId)
    {
        return _modifierGroups.ContainsKey(groupId);
    }

    public ModifierGroup GetModifierGroup(string groupId)
    {
        _modifierGroups.TryGetValue(groupId, out var group);
        return group;
    }
}