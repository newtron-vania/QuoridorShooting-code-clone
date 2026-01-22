using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterDefinition;

public class BaseStat
{
    public virtual int Index { get; protected set; }
    public virtual string Name { get; protected set; }
    public virtual int Hp { get; protected set; }
    public virtual int MaxHp { get; protected set; }
    public virtual int Atk { get; protected set; }
    public virtual float Avd { get; protected set; }
    public virtual int BaseAp { get; protected set; }
    public virtual int ApRecovery { get; protected set; }
    public virtual CharacterType Type { get; protected set; }
    public virtual CharacterClass Class { get; protected set; }
    public virtual int SkillId { get; protected set; }
    public virtual List<Vector2Int> MovablePositions { get; protected set; }
    public virtual List<Vector2Int> AttackablePositions { get; protected set; }

    public BaseStat(int index, string name, int hp, int maxHp, int atk, float avd, int baseAp, int apRecovery,
        CharacterType type = CharacterType.Successor, CharacterClass characterClass = CharacterClass.Tanker,
        int skillId = 0, List<Vector2Int> movablePositions = null, List<Vector2Int> attackablePositions = null)
    {
        Index = index;
        Name = name;
        Hp = hp;
        MaxHp = maxHp;
        Atk = atk;
        Avd = avd;
        BaseAp = baseAp;
        ApRecovery = apRecovery;
        Type = type;
        Class = characterClass;
        SkillId = skillId;
        MovablePositions = movablePositions ?? new List<Vector2Int>();
        AttackablePositions = attackablePositions ?? new List<Vector2Int>();
    }

    public BaseStat(BaseStat baseStat)
    {
        Index = baseStat.Index;
        Name = baseStat.Name;
        Hp = baseStat.Hp;
        MaxHp = baseStat.MaxHp;
        Atk = baseStat.Atk;
        Avd = baseStat.Avd;
        BaseAp = baseStat.BaseAp;
        ApRecovery = baseStat.ApRecovery;
        Type = baseStat.Type;
        Class = baseStat.Class;
        SkillId = baseStat.SkillId;
        MovablePositions = new List<Vector2Int>(baseStat.MovablePositions);
        AttackablePositions = new List<Vector2Int>(baseStat.AttackablePositions);
    }

    public override string ToString()
    {
        return $"[Index: {Index}] {Name}, MaxHP: {MaxHp}, ATK: {Atk}, Avd: {Avd}, BaseAP: {BaseAp}, AP Recovery: {ApRecovery}";
    }
}