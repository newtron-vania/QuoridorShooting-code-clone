using UnityEngine;

/// <summary>
/// BaseCharacter의 GroupModifier 지원을 위한 확장 (partial class)
/// StatuseffectGroupModifier 시스템에 필요한 필드와 메서드 제공
/// </summary>
public partial class BaseCharacter
{
    /// <summary>
    /// 도발(Provocation) 효과를 위한 강제 타겟
    /// AI가 이 값이 설정되어 있으면 해당 캐릭터를 우선 공격
    /// </summary>
    public BaseCharacter ForcedTarget { get; set; }

    /// <summary>
    /// 흡혈(Lifedrain) 효과를 위한 마지막 공격 피해량
    /// TakeDamage() 메서드에서 자동으로 업데이트됨
    /// </summary>
    public int LastDamageDealt { get; private set; }

    /// <summary>
    /// TakeDamage 메서드를 오버라이드하여 LastDamageDealt 추적
    /// 기존 코드에 영향을 주지 않도록 partial 메서드 활용
    /// </summary>
    partial void OnDamageDealt(int damageDealt)
    {
        LastDamageDealt = damageDealt;
    }
}
