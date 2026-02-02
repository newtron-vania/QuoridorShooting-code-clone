using System;

/// <summary>
/// 상태이상에 의한 행동 제한 플래그
/// Flags 특성으로 비트 연산을 통한 다중 제한 관리 가능
/// </summary>
[Flags]
public enum ActionRestrictionFlags
{
    None = 0,
    CannotAttack = 1 << 0,   // 1 - Blinded (일반 공격 불가)
    CannotMove = 1 << 1,     // 2 - Holded (이동 불가)
    CannotUseSkill = 1 << 2, // 4 - 스킬 사용 불가
    CannotBuild = 1 << 3     // 8 - 벽 건설 불가
}
