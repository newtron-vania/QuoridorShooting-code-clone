public enum StatModifierType
{
    FlatBaseAdd = 100,      // BaseStat 기준 덧셈
    PercentBaseAdd = 200,   // BaseStat 기준 곱셈
    FlatCurrentAdd = 300,   // 현재 스탯 기준 덧셈
    PercentCurrentAdd = 400 // 현재 스탯 기준 곱셈
}

public enum StatType
{
    MaxHp,
    Hp,
    Atk,
    Avd,
    Ap,
    ApRecovery,
    CharacterType,
    CharacterClass,
    SkillId
}
