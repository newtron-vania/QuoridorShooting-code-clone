using System.Collections.Generic;
using CharacterDefinition;

[System.Serializable]
public class CharacterData
{
    public int Index;                 // 캐릭터 idx
    public string Name;            // 캐릭터 이름
    public string Description;     // 캐릭터 설명
    public CharacterClass Class;               // 캐릭터 속성

    public CharacterType Type;                    // 캐릭터 역할(변수명 수정 예정) 1:탱커, 2:딜러, 3:서포터

    public int Hp;                 // 캐릭터 최대 체력

    public float Atk;              // 캐릭터 공격려

    public float Avd;          // 피해저항

    public int ApRecovery;          // 턴 당 증가 행동력(변수명 수정 예정)

    public int SkillId;                     // 캐릭터 스킬 id

    public int MoveRangeId;                 // 캐릭터 이동 가능 범위 id

    public int AttackRangeId;               // 캐릭터 공격 가능 범위 id   

    public CharacterData CloneCharacterData()
    {
        return new CharacterData
        {
            Index = this.Index,
            Name = this.Name,
            Description = this.Description,
            Class = this.Class,
            Type = this.Type,
            Hp = this.Hp,
            Atk = this.Atk,
            Avd = this.Avd,
            ApRecovery = this.ApRecovery,
            SkillId = this.SkillId,
            MoveRangeId = this.MoveRangeId,
            AttackRangeId = this.AttackRangeId
        };
    }
}