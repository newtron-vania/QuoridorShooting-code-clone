using System;
using UnityEngine;

public class DynamicStat : BaseStat
{
    public DynamicStat(BaseStat baseStat) : base(baseStat)
    {


    }


    public void SetHp(int hp)
    {
        if (hp < 0) hp = 0; // HP는 0 이상으로 설정
        if (hp > MaxHp) hp = MaxHp; // HP는 MaxHp 이하로 설정
        Hp = hp;
    }

    public void SetMaxHp(int maxHp)
    {
        //if (maxHp < 1) throw new ArgumentException("MaxHp must be greater than 0.");
        MaxHp = maxHp;

        // MaxHp가 변경되면 현재 HP 조정
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }

    public void SetAtk(int atk)
    {
        if (atk < 0) atk = 0;
        Atk = atk;
    }

    public void SetAvd(float damageResist)
    {
        Avd = damageResist;
    }

    public void SetBaseAp(int baseAP)
    {
        if (baseAP < 0) baseAP = 0;
        BaseAp = Mathf.Clamp(baseAP, 0, 100);
    }

    public void SetApRecovery(int apPerTurn)
    {
        if (apPerTurn < 0) apPerTurn = 0;
        ApRecovery = apPerTurn;
    }

    public void SetType(CharacterDefinition.CharacterType characterType)
    {
        Type = characterType;
    }

    public void SetClass(CharacterDefinition.CharacterClass characterClass)
    {
        Class = characterClass;
    }

    public void SetSkillId(int skillIndex)
    {
        if (skillIndex < 0) throw new ArgumentException("Skill index must be non-negative.");
        SkillId = skillIndex;
    }

    public void SetMovablePositions(System.Collections.Generic.List<UnityEngine.Vector2Int> positions)
    {
        MovablePositions = positions ?? new System.Collections.Generic.List<UnityEngine.Vector2Int>();
    }

    public void SetAttackablePositions(System.Collections.Generic.List<UnityEngine.Vector2Int> positions)
    {
        AttackablePositions = positions ?? new System.Collections.Generic.List<UnityEngine.Vector2Int>();
    }

}